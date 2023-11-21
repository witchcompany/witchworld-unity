using System.Collections.Generic;

namespace WitchCompany.Toolkit.Editor.DataStructure
{
    [System.Serializable]
    public class JUnityKey
    {
        public int unityKeyId;
        public JUnityKeyInfo blockData;
        public List<JBundleInfo> bundleInfoList;
        public JCollectionData collectionBundleData;
        public JBlockLocationInfo concertBundleData;
        // public JSalesUnityKeyData salesUnityKey;
    }
}