using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WitchCompany.Toolkit.Editor.API;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.Tool;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public static class CustomWindowPublish
    {
        private const string successMsg = "Upload Result : Success\n블록을 서버에 업로드했습니다";
        private const string failedMsg = "Upload Result : Failed\n블록을 서버에 업로드하지 못했습니다\n다시 시도해주세요";

        private static BuildTargetGroup blockPlatform;
        private static JBuildReport buildReport;
        private static UploadState uploadResult = UploadState.None;
        
        private static string[] bundleTypes =
        {
            AssetBundleConfig.Standalone,
            AssetBundleConfig.WebGL,
            AssetBundleConfig.Android,
            AssetBundleConfig.Ios
        };
        
        private enum UploadState
        {
            None,
            Uploading,
            Success,
            Failed
        }

        public static void ShowPublish()
        {
            // 빌드 정보
            DrawPublish();
            
            GUILayout.Space(10);

            if (buildReport != null)
            {
                DrawReport();
                GUILayout.Space(10);
                DrawUpload();
            }
        }

        public static BlockPublishOption GetOption()
        {
            return new BlockPublishOption { targetScene = PublishConfig.Scene, theme = PublishConfig.Theme };
        }

        /// <summary>
        /// 빌드 설정
        /// - Scriptable Object 적용 칸
        /// - 빌드 버튼
        /// </summary>
        private static async UniTaskVoid DrawPublish()
        {
            GUILayout.Label("Publish", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            // todo : 현재 씬 지정 버튼
            // todo : blockscene 에디터 저장

            using (new EditorGUILayout.HorizontalScope())
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var blockScene = EditorGUILayout.ObjectField("Scene", PublishConfig.Scene, typeof(SceneAsset), false) as SceneAsset;
                    if (check.changed)
                        PublishConfig.Scene = blockScene;

                    if (GUILayout.Button("Active Scene", GUILayout.Width(100)))
                    {
                        var activeScenePath = SceneManager.GetActiveScene().path;
                        PublishConfig.Scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScenePath);
                    }
                }
            }
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var blockTheme = (BlockTheme)EditorGUILayout.EnumPopup("Theme", (BlockTheme)PublishConfig.Theme);

                if (check.changed)
                    PublishConfig.Theme = blockTheme;
            } 

            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Publish"))
            {
                // 입력 제한
                CustomWindow.IsInputDisable = true;
                
                buildReport = WitchToolkitPipeline.PublishWithValidation(GetOption());
                
                if (buildReport.result == JBuildReport.Result.Success)
                {
                    // 썸네일 캡쳐
                    var thumbnailPath = Path.Combine(AssetBundleConfig.BundleExportPath, GetOption().ThumbnailKey);
                    CaptureTool.CaptureAndSave(thumbnailPath);
                    
                    // 업로드
                    EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Uploading to server...", 1.0f);
                    var result = await Upload();
                    EditorUtility.ClearProgressBar();
                    
                    var resultMsg = result ? successMsg : failedMsg;
                    EditorUtility.DisplayDialog("Witch Creator Toolkit", resultMsg, "OK");
                }

                // 입력 제한 해제
                CustomWindow.IsInputDisable = false;
            }
        }
        
        /// <summary>
        /// 빌드 결과
        /// - 방금 빌드 결과(성공/취소/실패)
        /// - 빌드파일 아웃풋 경로 (./WitchToolkit/Bundles)
        /// - 최종 빌드파일 용량
        /// - 빌드 종료 시간
        /// </summary>
        private static void DrawReport()
        {
            GUILayout.Label("Report", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Result", buildReport.result.ToString());
            
            if (buildReport.result == JBuildReport.Result.Success)
            {
                // TODO: 플랫폼에 따라 분류
                //EditorGUILayout.LabelField("ExportPath", buildReport.exportPath);
                //EditorGUILayout.LabelField("TotalSize", $"{CommonTool.ByteToMb(buildReport.totalSizeByte, 2)} MB");
                // 시작시간
                EditorGUILayout.LabelField("StartTime", $"{buildReport.BuildStatedAt.ToString()}");
                // 종료시간
                EditorGUILayout.LabelField("EndTime", buildReport.BuildEndedAt.ToString());
                // 소요시간 

                var time = buildReport.BuildEndedAt - buildReport.BuildStatedAt;
                EditorGUILayout.LabelField("Duration", (int)time.TotalSeconds + "s");;

            }
            
            EditorGUILayout.EndVertical();
        }

        private static void DrawUpload()
        {
            GUILayout.Label("Upload", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Result", uploadResult.ToString());

            EditorGUILayout.EndVertical();
        }

        private static async UniTask<bool> Upload()
        {
            var option = GetOption();

            var manifests = new Dictionary<string, JManifest>();
            foreach (var bundleType in bundleTypes)
            {
                var manifest = new JManifest()
                {
                    unityVersion = ToolkitConfig.UnityVersion,
                    toolkitVersion = ToolkitConfig.WitchToolkitVersion,
                    crc = AssetBundleTool.ReadManifest(bundleType, option.BundleKey)
                };
                
                manifests.Add(bundleType, manifest);
            }
            
            uploadResult = UploadState.Uploading;
            
            var response = await WitchAPI.UploadBundle(option, manifests);
            return response;
        }
    }
}