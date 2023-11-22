namespace WitchCompany.Toolkit.Editor.DataStructure
{
    [System.Serializable]
    public class JUnityKeyDetail
    {
        public int unityKeyDetailId;
        public string unityKeyType;
        public int count;

        public JUnityKeyDetail(string type)
        {
            unityKeyDetailId = 0;
            unityKeyType = type;
            count = 0;
        }
    }
}