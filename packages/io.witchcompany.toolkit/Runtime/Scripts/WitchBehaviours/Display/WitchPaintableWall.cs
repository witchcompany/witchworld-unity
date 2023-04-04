﻿using System.Collections.Generic;
using UnityEngine;
using WitchCompany.Toolkit.Extension;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    [RequireComponent(
        typeof(MeshCollider), 
        typeof(MeshRenderer),
        typeof(MeshFilter))]
    public class WitchPaintableWall : WitchBehaviourUnique
    {
        public override string BehaviourName => "낙서 벽";
        public override string Description => "유저가 자유롭게 낙서할 수 있는 벽입니다.\n" +
                                              "매쉬콜라이더 및 매쉬렌더러가 필요합니다.\n" +
                                              "낙서가 되는 벽면의 텍스쳐 및 색상은 낙서 설정을 따릅니다.\n" +
                                              "브러쉬가 없다면 검은색, 있다면 첫브러쉬의 색상으로 시작합니다.";
        public override string DocumentURL => "";

        public override int MaximumCount => 1;

        [Header("브러쉬 리스트"), SerializeReference] 
        private List<WitchPaintableBrush> brushes;

        [Header("낙서장 비율")] 
        [SerializeField] private Ratio paintRatioX = Ratio._512;
        [SerializeField] private Ratio paintRatioY = Ratio._512;
        
        [Header("기본 텍스쳐 설정")]
        [SerializeField] private Texture2D baseTexture;
        [SerializeField] private Color baseColor = Color.white;

        private enum Ratio
        {
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024
        }

        public List<WitchPaintableBrush> Brushes => brushes;
        public Vector2Int PaintRatio => new((int)paintRatioX, (int)paintRatioY);
        public Texture2D BaseTex => baseTexture;
        public Color BaseColor => baseColor;

#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (!TryGetComponent(out MeshCollider _)) return NullError(nameof(MeshCollider));
            if (!TryGetComponent(out MeshRenderer _)) return NullError(nameof(MeshRenderer));
            if (!TryGetComponent(out MeshFilter _)) return NullError(nameof(MeshFilter));

            if (TryGetComponent<WitchPaintableBrush>(out var b))
                return Error($"{b.BehaviourName}는 {BehaviourName}의 자식이어야 합니다.");

            foreach (var brush in brushes)
                if (!transform.HasChild(brush, false))
                    return ChildError(nameof(brush));

            return null;
        }
#endif
    }
}