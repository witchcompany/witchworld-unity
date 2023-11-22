using UnityEngine.Serialization;

namespace WitchCompany.Toolkit.Editor.DataStructure.Admin
{
    [System.Serializable]
    public class JUnityKey
    {
        [FormerlySerializedAs("bundles")] public DataStructure.JUnityKey unityKeys;
        public string[] platform;
        public string creatorNickName;
        public string pathName;
        public string theme;
        public string thumbnailUrl;
        public int unityKeyId;
    }
}