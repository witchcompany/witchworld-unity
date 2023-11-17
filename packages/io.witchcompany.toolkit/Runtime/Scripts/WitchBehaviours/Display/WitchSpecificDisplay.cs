using System;
using UnityEngine;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchSpecificDisplay : WitchDisplayBase
    {
        public override string BehaviourName => "전시: 지정 에셋을 배치하는 액자";
        public override string Description => "지정 에셋을 등록하여 전시할 수 있는 액자입니다.";
        public override string DocumentURL => "";
        public override int MaximumCount => 20;
        [field: Header("에셋을 구매하지 않았을 때 보여질 오브젝트")]
        [field: SerializeField] public GameObject NonObject { get; private set; }
        
        [field: Header("구매 페이지 Url")]
        [field: SerializeField] public String TargetUrl { get; private set; }
#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (String.IsNullOrWhiteSpace(TargetUrl)) return Error("targetUrl이 비어있습니다.");
            if (NonObject == null) return NullError(nameof(NonObject));
            if (NonObject.TryGetComponent(out Collider _))
                return new ValidationError("NonObject에 Collider가 있어야 합니다", context: NonObject);
            
            return null;
        }
#endif
    }
}