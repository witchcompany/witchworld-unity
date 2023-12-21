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
    public class CustomWindowExportBundle
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
                OnClickBuild();
                OnClickUpload().Forget();
            }

            if (validationReport != null)
            {
                DrawReport();
            }
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

            // Bundle Folder
            // using (new GUILayout.HorizontalScope())
            // {
            //     EditorGUILayout.TextField("Bundle Folder", UploadBundleConfig.BundleFolderPath);
            //     if (GUILayout.Button("Select", GUILayout.Width(100)))
            //     {
            //         UploadBundleConfig.BundleFolderPath = EditorUtility.OpenFolderPanel("Witch Creator Toolkit", UploadBundleConfig.BundleFolderPath,"");
            //     }
            // }
            
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
                var modelType = (GearType)EditorGUILayout.EnumPopup("Parts Type", UploadBundleConfig.GearType);

                if (check.changed)
                {
                    UploadBundleConfig.GearType = modelType;
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
                var salesId = EditorGUILayout.IntField("Sales Id", UploadBundleConfig.SalesItemId);

                if (check.changed)
                {
                    UploadBundleConfig.SalesItemId = salesId;
                }
            }
            EditorGUILayout.EndVertical();
        }


        private static void DrawReport()
        {
            GUILayout.Label("Export Report", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Result", validationReport.result.ToString());

            if (validationReport.result == ValidationReport.Result.Success)
            {
                var path = Path.Combine(ExportBundleConfig.BundleExportPath, ExportBundleConfig.Prefab.name);
                EditorGUILayout.LabelField("Path", path);
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
                        GUILayout.Label(error.message, CustomWindow.LogTextStyle);
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
            
        private static void OnClickBuild()
        {
            validationReport = null;
            
            CustomWindow.IsInputDisable = true;  
            EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Build...", 1.0f);

            validationReport = ItemBuildValidator.ValidationCheck();

            if (validationReport.result == ValidationReport.Result.Success)
            {
                buildReport = PrefabBuildPipeline.BuildReport(AssetBundleConfig.Standalone);
                buildReport = PrefabBuildPipeline.BuildReport(AssetBundleConfig.Webgl);
                buildReport = PrefabBuildPipeline.BuildReport(AssetBundleConfig.WebglMobile);
                buildReport = PrefabBuildPipeline.BuildReport(AssetBundleConfig.Android);
                buildReport = PrefabBuildPipeline.BuildReport(AssetBundleConfig.Ios);
            }
            
            EditorUtility.ClearProgressBar();
            CustomWindow.IsInputDisable = false;

            if(validationReport != null) return;
            
            var msg = buildReport.result == JBuildReport.Result.Success ? "빌드 성공" : "빌드 실패";
            EditorUtility.DisplayDialog("Witch Creator Toolkit", msg, "OK");
        }
        
        private static async UniTaskVoid OnClickUpload()
        {
            EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Uploading to server...", 1.0f);
            try
            {
                var result = await UploadItem();
                var msg = result ? AssetBundleConfig.SuccessMsg : AssetBundleConfig.FailedPublishMsg;

                EditorUtility.DisplayDialog("Witch Creator Toolkit", msg, "OK");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
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
            var typeName = UploadBundleConfig.GearType.ToString().Replace("Accessory", "Accessory_").ToLower();
            
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
    }
    
}