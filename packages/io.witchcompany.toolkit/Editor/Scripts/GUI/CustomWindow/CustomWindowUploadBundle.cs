using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.API;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.DataStructure.Item;
using WitchCompany.Toolkit.Editor.Tool;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public static class CustomWindowUploadBundle
    {
        private static string[] bundleTypes =
        {
            AssetBundleConfig.Standalone,
            AssetBundleConfig.Webgl,
            AssetBundleConfig.WebglMobile,
            AssetBundleConfig.Android,
            AssetBundleConfig.Ios,
            // AssetBundleConfig.Vr
        };
        
        public static void ShowUploadBundle()
        {
            DrawUploadBundle();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Upload"))
            {
                OnClickUpload().Forget();
            }
        }

        private static void DrawUploadBundle()
        {
            GUILayout.Label("Upload Bundle", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            // Bundle Folder
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.TextField("Bundle Folder", UploadBundleConfig.BundleFolderPath);
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                {
                    UploadBundleConfig.BundleFolderPath = EditorUtility.OpenFolderPanel("Witch Creator Toolkit", UploadBundleConfig.BundleFolderPath,"");
                }
            }
            
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
            EditorGUILayout.EndVertical();
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
            var bundleName = UploadBundleConfig.BundleFolderPath.Split("/")[^1];
            var typeName = UploadBundleConfig.PartsType.ToString().Replace("Accessory", "Accessory_").ToLower();
            
            // 아이템 정보
            var itemData = new JItemData
            {
                name = bundleName,
                type = typeName,
                bodiesToDisable = JsonConvert.SerializeObject(disableBodyIndexes)
            };

            // 번들 정보
            var bundleInfos = new Dictionary<string, JBundleInfo>();
            foreach (var bundleType in bundleTypes)
            {
                var bundlePath = Path.Combine(UploadBundleConfig.BundleFolderPath, $"{bundleName}_{bundleType}.bundle".ToLower());
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
            
            var result = await WitchAPI.UploadItemData(itemBundleData, UploadBundleConfig.BundleFolderPath, UploadBundleConfig.GltfPath);
            
            return result;
        }
    }
}