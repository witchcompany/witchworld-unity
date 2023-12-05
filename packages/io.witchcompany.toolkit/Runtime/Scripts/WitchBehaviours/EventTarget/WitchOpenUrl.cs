using UnityEngine;
using UnityEngine.Events;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchOpenUrl : WitchBehaviour
    {
        public override string BehaviourName => "이동: 새 탭에서 URL 열기";
        public override string Description => "지정한 url을 새 탭에서 열 수 있는 요소입니다.";
        public override string DocumentURL => "https://www.notion.so/witchcompany/WitchOpenUrl-50adfa83eea141e08c58953d09c6f73b?pvs=4";

        [Header("운영 Url")]
        [SerializeField, TextArea] private string targetUrl;
        [Header("개발 Url")]
        [SerializeField, TextArea] private string targetUrlDev;

        public UnityAction<string> OpenUrlAction;
        public void OpenUrl() => OpenUrlAction?.Invoke(targetUrl);
        
        public string TargetUrl => targetUrl;
        public string TargetUrlDev => targetUrlDev;
        
#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (string.IsNullOrEmpty(targetUrl)) return NullError(nameof(targetUrl));
            if (string.IsNullOrEmpty(targetUrlDev)) return NullError(nameof(targetUrlDev));

            return base.ValidationCheck();
        }
#endif
    }
}
