using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using WitchCompany.Toolkit.Scripts.WitchBehaviours;
using WitchCompany.Toolkit.Scripts.WitchBehaviours.Interface;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchGoodsDisplay : WitchBehaviour, ICollectionDisplay
    {
        public override string BehaviourName => "전시: 3D 지정 에셋";
        public override string Description => "판매 상품 ID를 입력해 구매한 상품을 전시할 수 있습니다.\n" +
                                              "전시 에셋과 전시대 오브젝트를 설정해주세요\n" +
                                              "전시 에셋과 전시대 오브젝트에는 Collider가 있어야 합니다.";
        public override string DocumentURL => "";
        
        [Header("Index")]
        [SerializeField, ReadOnly] public int Index;
        
        [field: Header("전시 될 미디어 타입")]
        // [field: SerializeField] public MediaType MediaType { get; private set; }

        [Header("판매 상품 ID")] 
        [SerializeField] private int salesItemId;
        [SerializeField] private int salesItemIdDeb;
        
        [field: Header("전시 이벤트")]
        [field: SerializeField] public UnityEvent OnDisplayEvent { get; private set; }
        [field: SerializeField] public UnityEvent OnHideDisplayEvent { get; private set; }

        [field: Header("전시 에셋"), SerializeField] public GameObject DisplayAsset { get; private set; }
        
        [field: Header("구매 유도 오브젝트"), SerializeField] public GameObject NonObject { get; private set; }
        
        [field: Header("앨범 재생 음악")]
        [field: SerializeField] public AudioClip AlbumMusic { get; private set; }

#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (!DisplayAsset) return NullError("DisplayAsset");
            if (!NonObject) return NullError("NonObject");
            if (!DisplayAsset.TryGetComponent(out Collider _))
                return new ValidationError("전시 에셋에 Collider가 있어야 합니다.", context: DisplayAsset);
            if (!NonObject.TryGetComponent(out Collider _))
                return new ValidationError("전시대에 Collider가 있어야 합니다.", context: NonObject);
            if (string.IsNullOrWhiteSpace(salesItemIdDeb.ToString()))
                return NullError("SalesIdDeb");
            if (string.IsNullOrWhiteSpace(salesItemId.ToString()))
                return NullError("SalesId");

            // if (MediaType == MediaType.Video && AlbumMusic == null)
            //     return NullError("AlbumMusic");
            
            return base.ValidationCheck();
        } 
#endif

        public int SalesItemId => salesItemId;
        public int SalesItemIdDev => salesItemIdDeb;
        public AssetType AssetType => AssetType.Goods;
        public int BehaviourIndex => Index;
        
        public void Editor_SetIndex(int idx) => Index = idx;
    }
}