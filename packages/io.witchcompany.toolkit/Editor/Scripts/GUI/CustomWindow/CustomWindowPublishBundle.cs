using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.API;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.DataStructure.Item;
using WitchCompany.Toolkit.Editor.Tool;
using WitchCompany.Toolkit.Editor.Validation;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public class CustomWindowPublishBundle
    {
        private static Vector2 scrollPos;
        private static ValidationReport validationReport;
        private static JBuildReport buildReport;
        
        private static string[] bundleTypes =
        {
            AssetBundleConfig.Standalone,
            AssetBundleConfig.Webgl,
            AssetBundleConfig.WebglMobile,
            AssetBundleConfig.Android,
            AssetBundleConfig.Ios,
            // AssetBundleConfig.Vr
        };
        
        public static void ShowExportAndUploadBundle()
        {
            DrawExportBundle();
            DrawUploadBundle();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Export & Upload"))
            {
                // CustomWindow.IsInputDisable = true;  
                // EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Validation...", 0.3f);
                //
                // // 리포트 초기화
                // validationReport = null;
                // buildReport = null;
                //
                // // 1. 검사
                // validationReport = ItemBuildValidator.ValidationCheck();
                //
                //
                // // 2. 번들 추출
                // // 검사 결과 true일 경우 번들 추출
                // if (TryGetBundle())
                // {
                //     EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Upload Bundle...", 0.9f);
                //     // 3. 번들 업로드
                //     // 번들 추출 성공한 경우 번들 업로드
                //     UploadBundle().Forget();
                // }
                // else
                // {
                //     var msg = "빌드 실패";
                //     EditorUtility.DisplayDialog("Witch Creator Toolkit", msg, "OK");
                // }
                //
                // EditorUtility.ClearProgressBar();
                // CustomWindow.IsInputDisable = false;

                OnClickPublish().Forget();

            }
            
            if(validationReport != null)
            {
                DrawReport();
            }
        }

        private static async UniTaskVoid OnClickPublish()
        {
            CustomWindow.IsInputDisable = true;  
                
            // 리포트 초기화
            validationReport = null;
            buildReport = null;

            // 1. 검사
            EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Validation...", 0.3f);

            validationReport = ItemBuildValidator.ValidationCheck();

            var isSuccessValidation = validationReport.result == ValidationReport.Result.Success;
            var message = "";
            
            // 2. 번들 추출
            // 검사 결과 true일 경우 번들 추출
            if (isSuccessValidation)
            {
                EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Build...", 0.6f);
                var isSuccessBuild = TryGetBundle();
                
                // 3. 번들 업로드
                // 번들 추출 성공한 경우 번들 업로드
                if (isSuccessBuild)
                {
                    EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Upload Bundle...", 0.9f);
                    
                    isSuccessBuild = await TryUploadBundle();
                    message = isSuccessBuild ? AssetBundleConfig.SuccessMsg : AssetBundleConfig.FailedPublishMsg;
                }
                else
                    message = "번들 추출 실패했습니다.";
            }
            else
                message = "유효성 검사 실패\n\nReport를 확인해주세요.";
            
            EditorUtility.DisplayDialog("Witch Creator Toolkit", message, "OK");
            
            EditorUtility.ClearProgressBar();
            CustomWindow.IsInputDisable = false;
        }
        
        
        private static void DrawExportBundle()
        {
            GUILayout.Label("Export Bundle", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            var check = new EditorGUI.ChangeCheckScope();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (check)
                {
                    var prefab = EditorGUILayout.ObjectField("Prefab", ExportBundleConfig.Prefab, typeof(GameObject), false) as GameObject;
                    if (check.changed)
                    {
                        // prefab 이름 저장
                        ExportBundleConfig.Prefab = prefab;
                        validationReport = null;
                    }
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                using (check)
                {
                    if (ExportBundleConfig.Prefab != null)
                    {
                        var bytes = AssetTool.GetFileSizeByte(ExportBundleConfig.PrefabPath);
                        var sizeKb = Math.Round((double)bytes / 1024, 3);
                        EditorGUILayout.LabelField("File Size", $"{sizeKb} / {ExportBundleConfig.MaxProductSizeKb} KB");
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private static void DrawUploadBundle()
        {
            GUILayout.Label("Upload Bundle", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            // Model File
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("Gltf File", UploadBundleConfig.GltfPath);
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                {
                    UploadBundleConfig.GltfPath = EditorUtility.OpenFilePanel("Witch Creator Toolkit", "", "gltf");
                }
            } 
            
            // Model Type
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var modelType = (GearType)EditorGUILayout.EnumPopup("Parts Type", UploadBundleConfig.PartsType);

                if (check.changed)
                {
                    UploadBundleConfig.PartsType = modelType;
                }
            }
            
            // Disable Body
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var disableBody = (SkinType)EditorGUILayout.EnumFlagsField("Disable Body", UploadBundleConfig.DisableBody);

                if (check.changed)
                {
                    UploadBundleConfig.DisableBody = disableBody;
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var salesItemId = EditorGUILayout.IntField("Sales Item Id", UploadBundleConfig.SalesItemId);

                if (check.changed)
                {
                    UploadBundleConfig.SalesItemId = salesItemId;
                }
            }
            EditorGUILayout.EndVertical();
        }


        private static void DrawReport()
        {
            GUILayout.Label("Export Report", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Result", buildReport?.result == JBuildReport.Result.Success ? "Success" : "Fail");
            //EditorGUILayout.LabelField("Result", validationReport.result.ToString());

            if (validationReport.result == ValidationReport.Result.Success)
            {
                var path = Path.Combine(ExportBundleConfig.BundleExportPath, ExportBundleConfig.Prefab.name);
                EditorGUILayout.LabelField("Path", path);
                EditorGUILayout.LabelField("UploadAt", buildReport?.BuildEndedAt.ToString());
            }
            else
            {
                GUILayout.Space(5);
                GUILayout.Label("Message", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
        
                // 에러 메시지 출력
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
                var preErrorTag = "";
                foreach (var error in validationReport.errors)
                {
                    if (error == null) return;
                    
                    // 이전 tag와 값이 다르면 tag 출력
                    if (preErrorTag != error.tag)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label(error.tag, EditorStyles.boldLabel);   
                        
                        preErrorTag = error.tag;
                    }

                    // 로그 종류에 따라 버튼 style 변경
                    if (error.context == null)
                        GUILayout.Label(error.message, CustomWindow.LogButtonStyle);
                    else
                    {
                        if (GUILayout.Button(error.message, CustomWindow.LogButtonStyle))
                            EditorGUIUtility.PingObject(error.context);
                    }
                }
                
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();
        }
            
        /// <summary>
        /// 번들 추출 시도 함수
        /// </summary>
        /// <returns></returns>
        private static bool TryGetBundle()
        {
            // 유효성 검사 결과가 성공이 아닐 경우 종료
            // if (validationReport.result != ValidationReport.Result.Success)
            //     return false;

            // 유효성 검사가 성공일 경우 번들 추출 진행
            // CustomWindow.IsInputDisable = true;  
            // EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Build...", 0.6f);

            // todo : 타입별로 빌드 시 중간에 에러난 경우 기존 추출된 파일도 삭제하는 로직 필요할 듯
            foreach (var bundleType in bundleTypes)
            {
                buildReport = PrefabBuildPipeline.BuildReport(bundleType);
                
                if (buildReport == null || buildReport.result != JBuildReport.Result.Success)
                {
                    // 번들 추출 실패한 경우
                    // 팝업 메시지 띄우기
                    // var msg = "빌드 실패";
                    // EditorUtility.DisplayDialog("Witch Creator Toolkit", msg, "OK");
                    
                    // EditorUtility.ClearProgressBar();
                    // CustomWindow.IsInputDisable = false;
                    
                    return false;
                }
            }
            
            // EditorUtility.ClearProgressBar();
            // CustomWindow.IsInputDisable = false;

            return true;
        }
        
        private static async UniTask<bool> TryUploadBundle()
        {
            // EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Uploading to server...", 0.9f);
            
            try
            {
                var result = await UploadItem();
                if (result)
                {
                    DeleteBundleFile();
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return false;
        }
        
        private static async UniTask<bool> UploadItem()
        {
            // 비활성화 신체 인덱스 문자열 추출
            var disableBodyToBinary = Convert.ToString(UploadBundleConfig.DisableBody.GetHashCode(), 2);
            var disableBodyIndexes = new List<int>();
            var maxIndex = disableBodyToBinary.Length - 1;
            for (var i = maxIndex; i >= 0; i--)
            {
                if (disableBodyToBinary[i] == '1')
                    disableBodyIndexes.Add(maxIndex - i);
            }
            
            // 번들 이름
            // var bundleName = UploadBundleConfig.BundleFolderPath.Split("/")[^1];
            var bundleName = ExportBundleConfig.Prefab.name;
            var typeName = UploadBundleConfig.PartsType.ToString().Replace("Accessory", "Accessory_").ToLower();
            
            // 아이템 정보
            var itemData = new JItemData
            {
                name = bundleName,
                type = typeName,
                bodiesToDisable = JsonConvert.SerializeObject(disableBodyIndexes),
                salesItemId = UploadBundleConfig.SalesItemId
            };

            // 번들 정보
            var bundleInfos = new Dictionary<string, JBundleInfo>();
            foreach (var bundleType in bundleTypes)
            {
                var bundlePath = Path.Combine(ExportBundleConfig.BundleExportPath, bundleName, $"{bundleName}_{bundleType}.bundle".ToLower());
                var bundleInfo = new JBundleInfo();
                var crc = AssetBundleTool.ReadManifest(bundlePath);
                if (crc != null)
                {
                    bundleInfo.unityVersion = ToolkitConfig.UnityVersion;
                    bundleInfo.toolkitVersion = ToolkitConfig.WitchToolkitVersion;
                    bundleInfo.crc = crc;
                }
                bundleInfos[bundleType] = bundleInfo;
            }

            var itemBundleData = new JItemBundleData
            {
                itemData = itemData,
                standalone = bundleInfos[AssetBundleConfig.Standalone],
                webgl = bundleInfos[AssetBundleConfig.Webgl],
                webglMobile = bundleInfos[AssetBundleConfig.WebglMobile],
                android = bundleInfos[AssetBundleConfig.Android],
                ios = bundleInfos[AssetBundleConfig.Ios],
                // vr = bundleInfos[AssetBundleConfig.Vr],
            };

            var result = await WitchAPI.UploadItemData(itemBundleData,
                Path.Combine(ExportBundleConfig.BundleExportPath, bundleName), UploadBundleConfig.GltfPath);
            
            return result;
        }

        private static void DeleteBundleFile()
        {
            var bundleName = ExportBundleConfig.Prefab.name;
            var bundleFolderPath = Path.Combine(ExportBundleConfig.BundleExportPath, bundleName);

            foreach (var bundleType in bundleTypes)
            {
                var bundleFileName = $"{bundleName}_{bundleType}.bundle".ToLower();
                var manifestFileName = $"{bundleFileName}.manifest".ToLower();

                var bundleFilePath = Path.Combine(bundleFolderPath, bundleFileName);
                var manifestPath = Path.Combine(bundleFolderPath, manifestFileName);
                
                
                // todo : 해당 경로에 파일이 있을 때만 삭제하도록 수정
                // if(File.Exists(bundleFilePath))
                
                File.Delete(bundleFilePath);
                File.Delete(manifestPath);
            }
        }
    }
}