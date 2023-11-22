using UnityEditor;
using UnityEngine.Serialization;

namespace WitchCompany.Toolkit.Editor.DataStructure
{
    [System.Serializable]
    public class BlockPublishOption
    {
        public string Key => targetScene.name;
        public string BundleKey => Key + ".bundle";
        public string ThumbnailKey => Key + ".jpg";
        
        public SceneAsset targetScene;
        [FormerlySerializedAs("theme")] public BlockType blockType;
        public bool isOfficial;
    }
}