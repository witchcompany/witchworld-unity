using UnityEngine;
using WitchCompany.Toolkit.Module;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit
{
    public class WitchLocalizeObject : WitchBehaviour
    {
        public override string BehaviourName => "로컬라이즈 3D 오브젝트";

        public override string Description => "로컬라이즈가 필요한 3D 오브젝트에 필요한 요소입니다." + 
                                              "한국어/영어 버전의 3D 오브젝트가 필요합니다.";
        public override string DocumentURL => "";
        
        [field: Header("3D 오브젝트")]
        [field: SerializeField] public GameObject KoObject { get; private set; }
        [field: SerializeField] public GameObject EnObject { get; private set; }

#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (KoObject == null) return NullError(nameof(KoObject));
            if (EnObject == null) return NullError(nameof(EnObject));

            return base.ValidationCheck();
        }
    }
#endif
}
