﻿using Newtonsoft.Json;

namespace WitchCompany.Toolkit.Editor.DataStructure
{
    [System.Serializable]
    public class JBundleInfo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string bundleType;
        public string unityVersion;
        public string toolkitVersion;
        public string crc;
    }
}