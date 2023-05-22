﻿using UnityEngine;
using UnityEngine.Events;
using WitchCompany.Toolkit.Attribute;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchTimer : WitchBehaviourUnique
    {
        public override string BehaviourName => "타이머";
        public override string Description => "타이머를 시작, 종료할 수 있는 요소입니다.\n" +
                                              "mm:ss:ms 로 표시됩니다.";
        public override string DocumentURL => "";

        public override int MaximumCount => 1;

        [Header("서버에 결과를 저장할지 여부")] 
        [SerializeField] private bool saveResultToServer = true;
        public bool SaveResultToServer => saveResultToServer;
        
        [Header("리스폰시 처리 방식")] 
        [SerializeField] private RespawnMode mode;
        public RespawnMode Mode => mode;

        [HideInInspector] public UnityEvent timerStartEvent = new ();
        [HideInInspector] public UnityEvent timerEndEvent = new();
        [HideInInspector] public UnityEvent timerCancelEvent = new ();
        [HideInInspector] public UnityEvent timerResetEvent = new();
        
        public void StartTimer() => timerStartEvent.Invoke();
        public void EndTimer() => timerEndEvent.Invoke();
        public void CancelTimer() => timerCancelEvent.Invoke();
        public void ResetTimer() => timerResetEvent.Invoke();

        public enum RespawnMode
        {
            KeepOnRespawn = 0,
            ResetOnRespawn,
            CancelOnRespawn
        }
    }
}