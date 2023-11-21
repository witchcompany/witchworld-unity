using System.Collections.ObjectModel;
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
        private const string Prefs_ThumbnailPath = "publish_thumnail";
        private const string Prefs_Level = "publish_level";
        private const string Prefs_Capacity = "publish_capacity";
        private const string Prefs_Official = "publish_official";
        private const string Prefs_Hide = "publish_private";
        private const string Prefs_SalesType = "publish_salestype";
        private const string Prefs_Quantity = "publish_quantity";
        private const string Prefs_Price = "publish_price";
        private const string Prefs_Collection = "publish_collection";
        private const string Prefs_ItemCA = "publish_itemCA";
        private const string Prefs_ItemLocationId = "publish_item_location_id";
        
        private const string Prefs_Name_Ko = "publish_name_ko";
        private const string Prefs_Name_En = "publish_name_en";
        private const string Prefs_Description_Ko = "publish_description_ko";
        private const string Prefs_Description_En = "publish_description_en";
        
        
        
        public static SceneAsset Scene
        {
            get => AssetDatabase.LoadAssetAtPath<SceneAsset>(EditorPrefs.GetString(Prefs_Scene, ""));
            set => EditorPrefs.SetString(Prefs_Scene, AssetDatabase.GetAssetPath(value));
        }
        
        public static string ThumbnailPath
        {
            get => EditorPrefs.GetString(Prefs_ThumbnailPath, "");
            set => EditorPrefs.SetString(Prefs_ThumbnailPath, value);
        }
        
        
        public static BlockType BlockType
        {
            get => (BlockType)EditorPrefs.GetInt(Prefs_Theme, 0);
            set => EditorPrefs.SetInt(Prefs_Theme, (int)value);
        }
        
        public static GameLevel Level
        {
            get => (GameLevel)EditorPrefs.GetInt(Prefs_Level, 0);
            set => EditorPrefs.SetInt(Prefs_Level, (int)value);
        }
        
        public static int Capacity
        {
            get => EditorPrefs.GetInt(Prefs_Capacity, 20);
            set => EditorPrefs.SetInt(Prefs_Capacity, (int)value);
        }

        public static bool Official
        {
            get => EditorPrefs.GetBool(Prefs_Official, false);
            set => EditorPrefs.SetBool(Prefs_Official, value);
        }
        
        public static bool Hide
        {
            get => EditorPrefs.GetBool(Prefs_Hide, true);
            set => EditorPrefs.SetBool(Prefs_Hide, value);
        }
        
        public static SalesType SalesType
        {
            get => (SalesType)EditorPrefs.GetInt(Prefs_SalesType, 0);
            set => EditorPrefs.SetInt(Prefs_SalesType, (int)value);
        }
        
        public static int Quantity
        {
            get => EditorPrefs.GetInt(Prefs_Quantity, 1);
            set => EditorPrefs.SetInt(Prefs_Quantity, value);
        }
        
        public static int Price
        {
            get => EditorPrefs.GetInt(Prefs_Price, 100);
            set => EditorPrefs.SetInt(Prefs_Price, value);
        }
        
        public static int Collection
        {
            get => EditorPrefs.GetInt(Prefs_Collection, 0);
            set => EditorPrefs.SetInt(Prefs_Collection, value);
        }
        
        public static string NameKo
        {
            get => EditorPrefs.GetString(Prefs_Name_Ko, "");
            set => EditorPrefs.SetString(Prefs_Name_Ko, value);
        }
        
        public static string NameEn
        {
            get => EditorPrefs.GetString(Prefs_Name_En, "");
            set => EditorPrefs.SetString(Prefs_Name_En, value);
        }
        
        public static string DescriptionKo
        {
            get => EditorPrefs.GetString(Prefs_Description_Ko, "");
            set => EditorPrefs.SetString(Prefs_Description_Ko, value);
        }
        
        public static string DescriptionEn
        {
            get => EditorPrefs.GetString(Prefs_Description_En, "");
            set => EditorPrefs.SetString(Prefs_Description_En, value);
        }

        public static string ItemCa
        {
            get => EditorPrefs.GetString(Prefs_ItemCA, "");
            set => EditorPrefs.SetString(Prefs_ItemCA, value);
        }
        
        public static int ItemLocationId
        {
            get => EditorPrefs.GetInt(Prefs_ItemLocationId, 0);
            set => EditorPrefs.SetInt(Prefs_ItemLocationId, value);
        }
    }
}