using System;
using UnityEngine;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchSpecificDisplay : WitchDisplayBase
    {
        public override string BehaviourName => "전시: 지정 아이템을 배치하는 액자";
        public override string Description => "지정 아이템을 등록하여 전시할 수 있는 액자입니다.";
        public override string DocumentURL => "";
        public override int MaximumCount => 20;
        
        [field: Header("구매 페이지 Url")]
        [field: SerializeField] public String TargetUrl { get; private set; }
#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (String.IsNullOrWhiteSpace(TargetUrl)) return Error("targetUrl이 비어있습니다.");
            
            return null;
        }
#endif
    }
}