using System;

namespace WitchCompany.Toolkit.Editor.DataStructure
{
    [Serializable]
    public class JReview
    {
        public int reviewId;
        public DateTime createdAt;
        public string message;
        public ReviewStatus status;
    }
}