﻿using System;

namespace WitchCompany.Toolkit.Module
{
    [Obsolete("사용 불가!!", true)]
    public class WitchPostItWall : WitchBehaviour
    {
        public override string BehaviourName => "포스트잇 벽";

        public override string Description => "포스트잇을 붙일 수 있는 벽입니다.";

        public override string DocumentURL => "";
    }
}