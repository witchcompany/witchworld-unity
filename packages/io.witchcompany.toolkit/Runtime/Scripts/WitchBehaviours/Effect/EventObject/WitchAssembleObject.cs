using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using WitchCompany.Toolkit.Attribute;
using WitchCompany.Toolkit.Extension;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchAssembleObject : WitchBehaviour
    {
        public override string BehaviourName => "효과: 여러 파츠 모으기";
        public override string Description => "여러개의 파츠를 모아주는 기능입니다.\n" +
                                              "* 파츠의 시작점, 끝점이 지정되어 있어야 합니다.\n" +
                                              "이벤트 오픈 이후 오브젝트가 움직이기 시작합니다.\n" +
                                              "* 툴킷에서 이벤트 날짜 설정시 날짜 형식은 yyyy-MM-ddTHH:mm:ssZ 입니다.";
        public override string DocumentURL => "https://www.notion.so/witchcompany/WitchAssembleObject-8df9a398c67b4a11b8dae954bc65eb93?pvs=4";

        public override int MaximumCount => 1;

        [field: Header("파츠")] 
        [field: SerializeField] public List<Transform> startParts { get; private set; }
        [field: SerializeField] public List<Transform> endParts { get; private set; }

        [field: Header("운영 salesId")]
        [field: SerializeField] public int saleId { get; private set; }
        [field: Header("개발 salesId")]
        [field: SerializeField] public int salesIdDev { get; private set; }
        [field: Header("이름표")]
        [field: SerializeField] public List<GameObject> nameTags { get; private set; }
        [SerializeField, ReadOnly] public TMP_Text[] names;

        [field: Header("진행도 UI")]
        [field: SerializeField] public Transform uiPos { get; private set; }
        
        [field:Header("툴킷에서 이벤트 날짜 설정")] 
        [field: SerializeField] public bool eventSettingInTookit { get; private set; } = true;
        [field: SerializeField] public string startDate;
        [field: SerializeField] public string endDate;
        // [HideInInspector] public UnityEvent openEventUI = new();
        // public bool eventOpenTest;


#if UNITY_EDITOR
        private DateTime checkDate;
        public override ValidationError ValidationCheck()
        {
            names = new TMP_Text[nameTags.Count];
            
            for (int i = 0; i < nameTags.Count; ++i)
            {
                names[i] = nameTags[i].GetComponentInChildren<TMP_Text>();
                nameTags[i].SetActive(false);
            }

            // // 날짜 형식
            // if (eventSettingInTookit && 
            //     (!DateTime.TryParseExact(startDate, "yyyy-MM-ddTHH:mm:ssZ", 
            //         System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out checkDate)
            //     || !DateTime.TryParseExact(endDate, "yyyy-MM-ddTHH:mm:ssZ", 
            //         System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out checkDate)))
            // {
            //     return Error("이벤트 날짜 형식이 올바르지 않습니다. 0000-00-00T00:00:00Z 형식으로 작성해주세요");
            // }

            return null;
        }
        
#endif
    }
}
