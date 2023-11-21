using System.Collections.Generic;
using Newtonsoft.Json;

namespace WitchCompany.Toolkit.Editor.DataStructure
{
    [System.Serializable]
    public class JCollectionData
    {
        public int collectionId;
        [JsonProperty(PropertyName = "salesItemAndBlockLocation")]
        public List<JBlockLocationInfo> blockLocationInfos;
    }
}