using UnityEngine;
using UnityEngine.Events;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchGoodsDisplay : WitchBehaviour
    {
        public override string BehaviourName => "전시: 지정 상품";
        public override string Description => "판매 상품 ID를 입력해 구매한 상품을 전시할 수 있습니다.\n" +
                                              "전시 에셋과 전시대 오브젝트를 설정해주세요\n" +
                                              "전시 에셋과 전시대 오브젝트에는 Collider가 있어야 합니다.";
        public override string DocumentURL => "";

        [Header("판매 상품 ID")] 
        [SerializeField] private int salesId;
        [SerializeField] private int salesIdDeb;

        [field: Header("판매 URL")] 
        [field: SerializeField] public string TargetUrl { get; private set; }
        
        [field: Header("구매한 아이템 전시 이벤트")]
        [field: SerializeField] public UnityEvent OnDisplayEvent { get; private set; }
        
        [field: Header("전시 에셋")]
        [field: SerializeField] public GameObject DisplayAsset { get; private set; }

        [field: Header("전시대")]
        [field: SerializeField] public GameObject DisplayShelf { get; private set; }

#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (!DisplayAsset.TryGetComponent(out Collider _))
                return new ValidationError("전시 에셋에 Collider가 있어야 합니다.", context: DisplayAsset);
            if (!DisplayShelf.TryGetComponent(out Collider _))
                return new ValidationError("전시대에 Collider가 있어야 합니다.", context: DisplayShelf);
            if (string.IsNullOrWhiteSpace(TargetUrl)) return Error("targetUrl이 비어있습니다.");
            
            return base.ValidationCheck();
        } 
#endif
        
    }
}