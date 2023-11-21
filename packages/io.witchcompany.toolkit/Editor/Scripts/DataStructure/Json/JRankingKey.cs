using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WitchCompany.Toolkit.Editor.DataStructure
{
    [System.Serializable]
    public class JRankingKey
    {
        public int gameInfoId;
        public string level;
        public string key;
        [JsonProperty(PropertyName="type")]
        public string sortType;
        public string dataType;
    }
}