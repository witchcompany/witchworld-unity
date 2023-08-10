﻿using UnityEngine;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    [RequireComponent(
        typeof(MeshCollider), 
        typeof(MeshRenderer),
        typeof(MeshFilter))]
    public class WitchPaintWall : WitchBehaviourUnique
    {
        public override string BehaviourName => "전시: 페인트 벽";
        public override string Description => "유저가 자유롭게 낙서할 수 있는 벽입니다.\n" +
                                              "매쉬콜라이더 및 매쉬렌더러가 필요합니다.\n" +
                                              "낙서가 되는 벽면의 텍스쳐 및 색상은 낙서 설정을 따릅니다.\n" +
                                              "브러쉬가 없다면 검은색, 있다면 첫브러쉬의 색상으로 시작합니다.";
        public override string DocumentURL => "https://www.notion.so/witchcompany/WitchPaintWall-d8f0382e1e234a3b912987f91bf6c954?pvs=4";

        public override int MaximumCount => 1;

        [Header("낙서장 비율")] 
        [SerializeField] private Ratio paintRatioX = Ratio._512;
        [SerializeField] private Ratio paintRatioY = Ratio._512;

        [Header("기본 브러쉬 색상")] 
        [SerializeField] private Color brushColor = Color.black;
        [Header("기본 브러쉬 반지름")] 
        [SerializeField, Range(0.01f, 0.5f)] private float brushRadius = 0.1f;
        [Header("기본 텍스쳐 (빈 값 가능)")]
        [SerializeField] private Texture2D baseTexture;

        private enum Ratio
        {
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024
        }

        public Vector2Int PaintRatio => new((int)paintRatioX, (int)paintRatioY);
        public Color BrushColor => brushColor;
        public float BrushRadius => brushRadius;
        public Texture2D BaseTex => baseTexture;

#if UNITY_EDITOR
        public override ValidationError ValidationCheck()
        {
            if (!TryGetComponent(out MeshCollider col)) return NullError(nameof(MeshCollider));
            if (!TryGetComponent(out MeshRenderer _)) return NullError(nameof(MeshRenderer));
            if (!TryGetComponent(out MeshFilter _)) return NullError(nameof(MeshFilter));
            
            if (col.isTrigger) return Error("콜라이더는 IsTrigger 체크될 수 없습니다.");
            if (col.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")) 
                return RayLayerError(col.gameObject);

            if (TryGetComponent<WitchPaintBrush>(out var b))
                return Error($"{b.BehaviourName}는 {BehaviourName}의 자식이어야 합니다.");

            return null;
        }
#endif
    }
}