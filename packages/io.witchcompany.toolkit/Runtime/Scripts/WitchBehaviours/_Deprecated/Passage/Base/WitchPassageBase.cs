﻿using System;
using UnityEngine;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module.Base
{
    [Obsolete("더 이상 사용되지 않는 기능입니다.", true)]
    public abstract class WitchPassageBase : WitchBehaviourUnique
    {
        [Header("이동할 블록의 url (witchworld.io/{여기})")] 
        [SerializeField] private string targetUrl;

        public string TargetUrl => targetUrl;
        
#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (string.IsNullOrEmpty(targetUrl))
                return Error($"{nameof(targetUrl)}을 설정해주세요.");
            return null;
        }
#endif
    }
}