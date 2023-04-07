﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;

namespace WitchCompany.Toolkit.Editor.Tool
{
    /// <summary>
    /// 에셋번들 관련 툴 모음
    /// </summary>
    public static class AssetBundleTool
    {
        /// <summary>assetPath에 있는 에셋을 bundleName을 가진 에셋번들로 할당한다.</summary>
        /// <param name="assetPath">"Assets/..." 에 해당하는 경로</param>
        /// <param name="bundleName">번들 이름</param>
        public static void AssignAssetBundle(string assetPath, string bundleName)
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new Exception("assetPath is null or empty");
            if (string.IsNullOrEmpty(bundleName))
                throw new Exception("bundleName is null or empty");
            
            // 에셋 가져오기
            var assetImporter = AssetTool.GetAssetImporterAtPath(assetPath);
            if (assetImporter == null)
                throw new Exception($"{assetPath}에 에셋이 없습니다.");

            // 에셋 번들 중복 확인
            if (!string.IsNullOrEmpty(assetImporter.assetBundleName)) 
                Debug.LogWarning($"에셋 번들을 덮어 씁니다. {assetImporter.assetBundleName} -> {bundleName}");
            // 에셋 번들 쓰기
            assetImporter.assetBundleName = bundleName;
        }

        /// <summary>프로젝트의 모든 에셋 번들을 지운다.</summary>
        public static void ClearAllBundles()
        {
            // 모든 에셋 번들 이름 가져오기
            var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();

            if (allAssetBundleNames.Length <= 0)
                return;

            // 각 에셋 번들 이름에 대해
            foreach (var assetBundleName in allAssetBundleNames)
            {
                // 에셋 번들 이름으로 지정된 모든 에셋 경로 가져오기
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);

                // 각 에셋 경로에 대해
                foreach (var assetPath in assetPaths)
                {
                    // 에셋의 AssetImporter 가져오기
                    var assetImporter = AssetImporter.GetAtPath(assetPath);

                    // 에셋의 에셋 번들 이름 지우기
                    assetImporter.assetBundleName = null;
                    assetImporter.SaveAndReimport();
                }
            }

            // 모든 사용되지 않는 에셋 번들 이름 지우기
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }

        /// <summary>번들을 타겟 플렛폼으로 빌드한다.</summary>
        public static Dictionary<string, string[]> BuildAssetBundle()
        {
            try
            {
                var result = new Dictionary<string, string[]>();

                // WebGL 빌드
                result.Add(AssetBundleConfig.WebGL, BuildBundle(AssetBundleConfig.WebGL, BuildTarget.WebGL));
                result.Add(AssetBundleConfig.Standalone, BuildBundle(AssetBundleConfig.Standalone, BuildTarget.StandaloneWindows64));

                return result;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private static string[] BuildBundle(string targetName, BuildTarget target)
        {
            try
            {
                const BuildAssetBundleOptions option = BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                                       BuildAssetBundleOptions.ChunkBasedCompression;
                
                // 경로
                var path = Path.Combine(AssetBundleConfig.BundleExportPath, targetName);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                // 빌드
                var result = BuildPipeline.BuildAssetBundles(path, option, target);

                // 삭제
                File.Delete(Path.Combine(path, targetName));
                File.Delete(Path.Combine(path, targetName + ".manifest"));

                return result.GetAllAssetBundles();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
        
        /// <summary> manifest 파일에서 crc 읽음 </summary>
        public static string ReadManifest(string bundleKey)
        {
            // 번들 경로/씬이름.bundle.manifest
            var manifest = Path.Combine(AssetBundleConfig.BundleExportPath, bundleKey)+".manifest";
            
            var crcLine = File.ReadAllLines(manifest)[1];
            var crc = crcLine.Split(" ")[1];
            
            return crc;
        }
    }
}