using UnityEngine;
using WitchCompany.Toolkit.Attribute;
using WitchCompany.Toolkit.Scripts.WitchBehaviours;
using WitchCompany.Toolkit.Scripts.WitchBehaviours.Interface;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchCraftingDisplay : WitchBehaviour
    {
        public override string BehaviourName => "전시: 크래프팅 액자";
        public override string Description => "로컬에 저장된 이미지나 비디오를 전시할 수 있는 기능입니다.\n" +
                                              "선택한 이미지와 비디오는 위치월드 에셋으로 크래프팅됩니다.\n" +
                                              "해당 오브젝트에는 콜라이더가 있어야 합니다.";
        public override string DocumentURL => "";
        
        [Header("Index")]
        [SerializeField, ReadOnly] private int index;
        
        [Header("미디어 타입")]
        [field: SerializeField] private MediaType MediaType;

        [Header("미디어가 그려질 랜더러")]
        [field: SerializeField] private Renderer MediaRenderer;

        [Header("미디어가 없을 때 보여줄 오브젝트")]
        [field: SerializeField] private GameObject NonObject;
        
        
        [Header("크래프팅 아이템 정보")]
        [field: SerializeField] private string ItemName;
        [field: SerializeField] private JLanguageString ItemDescription;
        [field: SerializeField] private bool IsPrivate;
        

        [System.Serializable]
        public class JLanguageString
        {
            public string en;
            public string kr;
        }
        
#if UNITY_EDITOR

        public override ValidationError ValidationCheck()
        {
            if (MediaRenderer == null) return NullError("Collider");
            if (!TryGetComponent<Collider>(out var col))
                return new ValidationError($"{gameObject.name}에는 콜라이더가 있어야 합니다.", "Null Collider", this);

            if (string.IsNullOrWhiteSpace(ItemName) || ItemName.Length is <= 0 or > 20)
                return new ValidationError($"{gameObject.name}의 itemName은 1~20자여야 합니다."," ",this);
            
            if(ItemDescription.en.Length > 200)
                return new ValidationError($"{gameObject.name}의 description의 en은 200자 이내여야 합니다."," ",this);
            
            if(ItemDescription.kr.Length > 200)
                return new ValidationError($"{gameObject.name}의 description의 kr은 200자 이내여야 합니다."," ",this);
            

            return base.ValidationCheck();
        }
        
        public void Editor_SetIndex(int idx) => index = idx;
#endif
        
    }
}
