using System;
using UnityEngine;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Editor.Validation
{
    public static class WhiteListValidator
    {
        private static readonly string[] ComponentTypeWhiteList = 
        {
            "Transform",
            "ParticleSystem",
            "Collider",
            "Animator",
            "Rigidbody",
            "MeshFilter",
            // 렌더러 그룹
            "ParticleSystemRenderer",
            "ParticleRenderer",
            "SkinnedMeshRenderer",
            "MeshRenderer",
            "LineRenderer",
            "OcclusionPortal",
            "OcclusionArea",
            // 빛 그룹
            "Light",
            "LightProbeGroup",
            "LightProbeProxyVolume",
            "ReflectionProbe",
            "LensFlare",
            "Skybox",
            // UI 그룹
            "Canvas",
            "CanvasGroup",
            "CanvasRenderer",
            "RectTransform",
            "SpriteMask",
            "CanvasScaler",
            "GraphicRaycaster",
            "Image",
            "Button",
            "RawImage",
            "Mask",
            "ScrollRect",
            "Scrollbar",
            "HorizontalLayoutGroup",
            "VerticalLayoutGroup",
            "ContentSizeFitter",
            "LayoutElement",
            "TextMesh",
            // Text Mesh Pro
            "InlineGraphic",
            "InlineGraphicManager",
            "TMP_Dropdown",
            "TMP_InputField",
            "TMP_ScrollbarEventHandler",
            "TMP_SelectionCaret",
            "TMP_SpriteAnimator",
            "TMP_SubMesh",
            "TMP_SubMeshUI",
            "TMP_Text",
            "TextMeshPro",
            "TextMeshProUGUI",
            "TextContainer",
            "TMP_Dropdown",
            // 툴킷
            "WitchBehaviour",
            // 포스트 프로세싱
            "PostProcessVolume",
            // 파티클 에셋
            //"CFXR_Effect"
            // bifrost
            // "FaceCamera",
            // "DialogueIndicator",
            // "BifrostIndicator",
            // "DialogueButtonSystem",
            // "DOTweenVisualManager",
            // "DOTweenAnimation",
            // "DialogueTriggerClick",
            // "DialogueSystem",
            // "BifrostQuiz",
            // "BifrostServiceIntroduction",
            // "OnClickOpenLink",
            // "MoveCamera",
            // "TrueShadow",
            // "EventTrigger",
            // "Logic_ConditionChecker",
            // "EggDataManager"
            
            "SoftMaskable",
            "UIGradient",
            "SoftMask",
            "GridLayoutGroup",
            
            // Rig 그룹
            "RigBuilder",
            "Rig",
            "ChainIKConstraint",
            
            // 메시 최적화
            "MeshCombiner",
            "LODGroup",
        };
        public static ValidationReport ValidationCheck()
        {
            var report = new ValidationReport();
            var allTransforms = GameObject.FindObjectsOfType<Transform>(true);
            
            // 오브젝트 단위
            foreach (var tr in allTransforms)
            {
                var components = tr.GetComponents<Component>();
                
                // 컴포넌트 단위
                foreach (var c in components)
                {
                    var fullType = c.GetType();
                    // whitelist에서 컴포넌트 존재 여부 탐색
                    while (fullType != null)
                    {
                        var type = fullType.FullName.Split(".")[^1];
                        var check = Array.Exists(ComponentTypeWhiteList, x => x == type);
                        
                        // white list에 있으면 통과
                        if (check) break;
                        
                        fullType = fullType.BaseType;
                    }

                    if (fullType != null) continue;
                    
                    var typeName = c.GetType().FullName.Split(".")[^1];
                    var error = new ValidationError($"Object : {c.name}\n{typeName} 컴포넌트는 배치할 수 없습니다.", ValidationTag.TagComponent, c);
                    report.Append(error);
                }
            }
            
            return report;
        }
    }
}