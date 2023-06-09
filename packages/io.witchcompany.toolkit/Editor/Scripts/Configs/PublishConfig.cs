using UnityEditor;
using WitchCompany.Toolkit.Editor.DataStructure;

namespace WitchCompany.Toolkit.Editor.Configs
{
    /// <summary>
    /// 툴킷 publish 설정
    /// </summary>
    public static class PublishConfig
    {
        private const string Prefs_Scene = "publish_scene";
        private const string Prefs_Theme = "publish_theme";
        private const string Prefs_Platform = "publish_platform";

        public static SceneAsset Scene
        {
            get => AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorPrefs.GetString(Prefs_Scene, ""));
            set => EditorPrefs.SetString(Prefs_Scene, AssetDatabase.GetAssetPath(value));
        }
        
        public static BundleTheme Theme
        {
            get => (BundleTheme)EditorPrefs.GetInt(Prefs_Theme, 0);
            set => EditorPrefs.SetInt(Prefs_Theme, (int)value);
        }
        
        public static PlatformType Platform
        {
            get => (PlatformType)EditorPrefs.GetInt(Prefs_Platform, (int)PlatformType.None);
            set => EditorPrefs.SetInt(Prefs_Platform, (int)value);
        }
    }
}