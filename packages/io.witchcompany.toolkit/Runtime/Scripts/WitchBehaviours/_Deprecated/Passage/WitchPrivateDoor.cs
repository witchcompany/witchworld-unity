using System;
using UnityEngine;
using WitchCompany.Toolkit.Module.PhysicsEffect;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    [RequireComponent(typeof(WitchDoor))]
    [RequireComponent(typeof(WitchDoorEffect))]
    [DisallowMultipleComponent]
    [Obsolete("더이상 사용되지 않는 기능입니다.", true)]
    public class WitchPrivateDoor : WitchBehaviour
    {
        public override string BehaviourName => "통로: 프라이빗 문";
        public override string Description => "특정 아이템을 가지고 있을 때 프라이빗 공간으로 연결되는 문입니다.\n" +
                                              "입장 권한을 확인할 아이템을 지정해야 합니다.";
        public override string DocumentURL => "https://www.notion.so/witchcompany/WitchPrivateDoor-1c0168060cc740d9883fd383104804cc?pvs=4";

        [Header("입장 권할을 지정할 아이템의 ID (개발용)"), SerializeField]
        private int itemIdDev;
        [Header("입장 권할을 지정할 아이템의 ID (운영용)"), SerializeField]
        private int itemIdProd;
        [Header("설명 제목"), SerializeField]
        private string popupDescriptionTitle;
        [Header("설명"), SerializeField, TextArea]
        private string popupDescription;

        public int ItemIdDev => itemIdDev;
        public int ItemIdProd => itemIdProd;
        
        public string PopupDescriptionTitle => popupDescriptionTitle;
        public string PopupDescription => popupDescription;
        

#if UNITY_EDITOR
        public override ValidationError ValidationCheck() => Error("더이상 사용되지 않는 기능입니다.");
#endif
    }
}
