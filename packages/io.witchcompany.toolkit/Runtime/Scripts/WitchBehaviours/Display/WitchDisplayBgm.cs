using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using WitchCompany.Toolkit.Module;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit
{
    public class WitchDisplayBgm : WitchBehaviour
    {
        public override string BehaviourName => "전시: 전시 BGM";
        public override string Description => "전시 유무에 따라 공간 BGM을 변경할 수 있는 요소입니다.\n" +
                                              "연결된 전시 툴킷에 아이템이 배치되면 설정한 오디오 클립으로 변경됩니다.\n" +
                                              "해당 요소는 블록 당 1개만 배치할 수 있습니다.";
        public override string DocumentURL => " ";

        public override int MaximumCount => 1;

        [Header("Display")]
        [SerializeField] private List<WitchBehaviour> displayBehaviours;
        
        [Header("Audio Clip")]
        [SerializeField] private AudioClip audioClip;

        public List<WitchBehaviour> DisplayBehaviours => displayBehaviours;
        public AudioClip AudioClip => audioClip;

#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (displayBehaviours is {Count: <= 0}) return NullError(nameof(displayBehaviours));
            if (audioClip == null) return NullError(nameof(audioClip));
            
            return base.ValidationCheck();
        }
        
#endif
    }
}
 