using System.Collections.Generic;
using UnityEngine;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Module;
using WitchCompany.Toolkit.Validation;
namespace WitchCompany.Toolkit.Editor.Validation
{
    public static class ExistComponentValidator
    {
        private const string CommonText = "모든";
        private static Dictionary<string, List<Component>> allComponents = new();
        
        
        // 모든 테마 필수 컴포넌트
        private static readonly string[] CommonEssentialComponents =
        {
            nameof(WitchBlockManager),
            //nameof(WitchPortal)
            //"WitchSpawnPoint"
        };
        
        // 게임 테마 필수 컴포넌트
        private static readonly string[] GameEssentialComponents =
        {
            nameof(WitchLeaderboard)
            //"WitchLeaderboard"
        };
        
        // 유료 테마 컴포넌트
        private static readonly string[] PremiumComponents =
        {
            nameof(WitchBooth),
            nameof(WitchTimer),
            nameof(WitchLeaderboard)
        };
        
        public static ValidationReport ValidationCheck()
        {
            var report = new ValidationReport();
            var allTransforms = GameObject.FindObjectsOfType<Transform>(true);
            
            // 딕셔너리 초기화
            allComponents = new Dictionary<string, List<Component>>();
            
            // 모든 Transform을 순회하면서
            foreach (var tr in allTransforms)
            {
                var trComponents = tr.GetComponents<Component>();
                // transform의 컴포넌트 순회
                foreach (var component in trComponents)
                {
                    var fullType = component.GetType();
                    if (fullType.FullName != null)
                    {
                        // 컴포넌트 이름 가져옴
                        var type = fullType.FullName.Split(".")[^1];
                        
                        // 포함된 컴포넌트가 아니라면 키값에 리스트 추가
                        if (allComponents.ContainsKey(type) == false)
                            allComponents[type] = new List<Component> { component };
                        // 포함된 컴포넌트라면 컴포넌트 리스트에 추가
                        else
                            allComponents[type].Add(component);
                    }
                }
            }

            // 모든 블록 타입 공통 컴포넌트 보유 여부 검사
            CheckEssentialComponent(report, CommonEssentialComponents, CommonText);
            
            // 블록 타입이 게임일 경우 필수 게임 컴포넌트 보유 여부 검사
            if(PublishConfig.BlockType == BlockType.Game)
                CheckEssentialComponent(report, GameEssentialComponents, BlockType.Game.ToString());
            
            // // 무료 테마일 경우 유료 에셋 포함 여부 검사
            // if(PublishConfig.SalesType == SalesType.Free)
            //     CheckPremiumComponent(report);
            
            return report;
        }

        /// <summary>
        /// 필수 컴포넌트 보유 검사
        /// </summary>
        /// <param name="report"></param>
        /// <param name="allComponents"></param>
        /// <param name="targetComponents"></param>
        /// <param name="blockType"></param>
        private static void CheckEssentialComponent(ValidationReport report, string[] targetComponents, string blockType = "")
        {
            foreach (var component in targetComponents)
            {
                if(allComponents.ContainsKey(component)) continue;

                // 공통 컴포넌트 존재하지 않을 경우
                var error = new ValidationError($"{blockType} 블록에서 필수로 배치되어야 하는 {component}가 없습니다.",
                    ValidationTag.TagEssentialList);
                
                report.Append(error);
            }
        }
        /// <summary>
        /// 유료 에셋 포함 여부 검사
        /// </summary>
        /// <param name="report"></param>
        private static void CheckPremiumComponent(ValidationReport report)
        {
            foreach (var premiumComponent in PremiumComponents)
            {
                if (allComponents.ContainsKey(premiumComponent))
                {
                    if(allComponents[premiumComponent] == null) continue;
                    
                    foreach (var component in allComponents[premiumComponent])
                    {
                        var error = new ValidationError($"{component.transform.name}\n무료 테마에는 {premiumComponent}가 포함될 수 없습니다.", "Pay Asset", component);
                        report.Append(error);
                    }
                }
            }
            
        }
    }
}