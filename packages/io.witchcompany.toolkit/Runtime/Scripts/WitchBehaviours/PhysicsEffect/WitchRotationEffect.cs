using UnityEngine;
using WitchCompany.Toolkit.Attribute;

namespace WitchCompany.Toolkit.Module.PhysicsEffect
{
    [DisallowMultipleComponent]
    public class WitchRotationEffect : WitchPhysicsEffectBase
    {
        public override string BehaviourName => "물리효과: 회전";
        public override string Description => "오브젝트에 회전하는 효과를 줍니다.";
        public override string DocumentURL => "https://www.notion.so/witchcompany/WitchRotationEffect-1ab98bb5a6ed4cec85ee7bffd430121c?pvs=4";
        public override int MaximumCount => 64;

        public enum Mode
        {
            Speed = 0,
            Destination
        }

        public enum EaseType
        {
            Linear = 0,
            EaseIn,
            EaseOut,
            EaseInOut
        }

        [Header("모드")]
        [SerializeField] private Mode rotationMode = Mode.Speed;
        
        [Space(15)]
        
        [ShowIf(nameof(rotationMode), Mode.Speed)][Tooltip("회전 속도 X")]
        [Range(-30f, 30f), SerializeField] private float speedX = 0f;
        [ShowIf(nameof(rotationMode), Mode.Speed)][Tooltip("회전 속도 Y")]
        [Range(-30f, 30f), SerializeField] private float speedY = 0.5f;
        [ShowIf(nameof(rotationMode), Mode.Speed)][Tooltip("회전 속도 Z")]
        [Range(-30f, 30f), SerializeField] private float speedZ = 0;

        [ShowIf(nameof(rotationMode), Mode.Destination)] [Tooltip("목표 회전 값")]
        [SerializeField] private Vector3 endValue;
        [ShowIf(nameof(rotationMode), Mode.Destination)] [Tooltip("목표 회전 시간")] 
        [SerializeField, Range(0.1f, 300f)] private float duration = 1f;
        [ShowIf(nameof(rotationMode), Mode.Destination)] [Tooltip("회전 보간 타입")]
        [SerializeField] private EaseType ease = EaseType.Linear;

        public Mode RotationMode => rotationMode;
        public Vector3 Speed => new(speedX, speedY, speedZ);
        
        public Vector3 EndValue => endValue;
        public float Duration => duration;
        public EaseType Ease => ease;
    }
}