using System.IO;
using UnityEditor;
using UnityEngine;

namespace WitchCompany.Toolkit.Editor.Configs
{
    public static class ExportBundleConfig
    {
        // 빌드 경로
        private static readonly string BundleString = "Products";
        public static readonly string BundleExportPath = Path.Combine(".", "WitchToolkit", BundleString); 
        
        // 검사
        public const uint MaxProductSizeKb = 60;

        public static string PrefabPath => Prefab != null ? AssetDatabase.GetAssetPath(Prefab) : "";
        
        private const string Prefs_Prefab = "export_prefab";
        public static GameObject Prefab
        {
            get => AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString(Prefs_Prefab, ""));
            set => EditorPrefs.SetString(Prefs_Prefab, AssetDatabase.GetAssetPath(value));
        }
    }
}