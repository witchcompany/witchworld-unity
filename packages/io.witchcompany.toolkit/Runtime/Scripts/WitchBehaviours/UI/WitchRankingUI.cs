using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using WitchCompany.Toolkit.Runtime.Scripts.WitchBehaviours.Event.Base;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Runtime.Scripts.WitchBehaviours.UI
{
    public class WitchRankingUI : WitchUIBase
    {
        public override string BehaviourName => "UI: 랭킹";
        public override string Description => "랭킹 정보를 가져오는 요소입니다.\n" +
                                              "key에 해당하는 정보를 가져옵니다.";
        public override string DocumentURL => "";

        [Header("랭킹에 표시할 값의 key"), SerializeField]
        private string[] keys;
        [Header("랭킹을 표시할 게임 오브젝트"), SerializeField] 
        private WitchSyncedTextHandlerUI rankingCard;
        [Header("랭킹을 표시할 위치"), SerializeField] 
        private Transform content;
        [Header("첫 화면에 보여줄 페이지"), SerializeField]
        private int page;
        [Header("한 페이지에 표시할 랭킹 개수"), SerializeField]
        private int limit;
        [Header("순위 표시 여부"), SerializeField]
        private bool isRanking;

        [HideInInspector] public UnityEvent show = new();
        [HideInInspector] public UnityEvent hide = new();
        
        public void Show() => show.Invoke();
        public void Hide() => hide.Invoke();

        public string[] Keys => keys;
        public WitchSyncedTextHandlerUI RankingCard => rankingCard;
        public Transform Content => content;
        public int Page => page;
        public int Limit => limit;
        public bool IsRanking => isRanking;
        

#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            return null;
        }
#endif
    }
}