using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using WitchCompany.Toolkit.Attribute;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Module
{
    public class WitchBlockManager : WitchBehaviourUnique
    {
        [Obsolete("더이상 사용하지 않습니다.")]
        public Transform SpawnPoint => transform;
        
        public override string BehaviourName => "필수: 블록 매니저";

        public override string Description => "블록의 기본이 되는 요소입니다.\n" +
                                              "모든 오브젝트의 최상위 부모이며, 기본 좌표에 있어야 합니다.\n" +
                                              "한 씬에 하나만 배치할 수 있습니다.";
        public override string DocumentURL => "https://www.notion.so/witchcompany/WitchBlockManager-27694686287943d28baa4cfbf1803b28?pvs=4";
        public override int MaximumCount => 1;

        [Header("정보")] 
        [SerializeField, ReadOnly] private string unityVersion;
        [SerializeField, ReadOnly] private string toolkitVersion;
        [SerializeField, ReadOnly] private string updatedDateUtc;

        [Header("연결된 behaviours")]
        [SerializeField, ReadOnly] private List<WitchBehaviour> behaviours;
        
        [field:Header("필수 정보")] 
        [field:SerializeField] public WitchPortal EntrancePortal { get;  set; }
        [field:SerializeField] public WitchPortal ExitPortal { get;  set; }
        
        [Header("선택 정보")]
        [SerializeField] private AudioClip defaultBGM;
        [SerializeField] private PointOfView pov = PointOfView.Free;
        
        [Header("리스폰 이벤트")]
        public UnityEvent respawnEvent;

        // 외부 접근
        public string ToolkitVersion => toolkitVersion;

        public AudioClip DefaultBGM { get => defaultBGM; set => defaultBGM = value; }
        public PointOfView POV => pov;
        public List<WitchBehaviour> Behaviours => behaviours;

        public void Respawn() => respawnEvent.Invoke();

#if UNITY_EDITOR
        /// <summary>위치 요소를 카운팅한 딕셔너리</summary>
        public Dictionary<Type, (WitchBehaviour behaviour, int count)> BehaviourCounter { get; private set; }
        
        public override ValidationReport ValidationCheckReport()
        {
            var report = new ValidationReport();
            
            // 기본 transform 체크
            if (transform.position != Vector3.zero) report.Append(Error("매니저의 좌표는 0이어야 합니다."));
            if (transform.rotation != Quaternion.identity) report.Append(Error("매니저의 회전값은 0이어야 합니다."));
            if (transform.localScale != Vector3.one) report.Append(Error("매니저의 스케일은 1이어야 합니다."));
            
            // 포탈 체크
            if (EntrancePortal == null) report.Append(NullError("entrancePortal"));
            if (ExitPortal == null) report.Append(NullError("exitPortal"));
            
            // 이벤트 체크
            report.Append(EventHandlerCheck(respawnEvent));
            
            return report;
        }

        /// <summary>포함한 위치 요소를 모두 검색하고, 카운팅한다.</summary>
        public void FindWitchBehaviours(string unityVer, string toolkitVer)
        {
            if(Application.isPlaying) return;

            // 날짜 설정
            unityVersion = unityVer;
            toolkitVersion = toolkitVer;
            updatedDateUtc = DateTime.UtcNow.ToString("s");
            
            // 요소 검색
            behaviours = transform.GetComponentsInChildren<WitchBehaviour>(true).ToList();
            behaviours.Remove(this);

            // 카운터 초기화
            BehaviourCounter ??= new Dictionary<Type, (WitchBehaviour obj, int count)>();
            BehaviourCounter.Clear();

            var displayPhotos = new List<WitchDisplayFrame>();
            var displayVideos = new List<WitchDisplayFrame>();
            var displayLive = new List<WitchDisplayFrame>();

            var publicPaintCount = 0;
            var boothCount = 0;            
            var beggingCount = 0;
            var craftingCount = 0;
            var specificCount = 0;
            var goodsCount = 0;

            
            foreach (var behaviour in behaviours)
            {
                // 각 요소별 카운팅
                var type = behaviour.GetType();
                if (!BehaviourCounter.ContainsKey(type))
                    BehaviourCounter.Add(type, (behaviour, 1));
                else
                    BehaviourCounter[type] = (behaviour, BehaviourCounter[type].count + 1);

                // DisplayFrame 카운팅
                if (type == typeof(WitchDisplayFrame))
                {
                    var display = (WitchDisplayFrame) behaviour;
                    switch (display.MediaType)
                    {
                        case MediaType.Picture:
                            displayPhotos.Add(display);
                            break;
                        case MediaType.Video:
                            displayVideos.Add(display);
                            break;
                        case MediaType.LiveVideo:
                            displayLive.Add(display);
                            break;
                    }
                }
                
                // 낙서 카운팅
                if (type == typeof(WitchPaintWall))
                {
                    var paintWall = (WitchPaintWall) behaviour;
                    if (paintWall.DrawPermission == WitchPaintWall.Permission.All)
                    {
                        publicPaintCount++;
                        paintWall.Editor_SetInvalid(publicPaintCount > 1 );
                    }
                    else
                    {
                        paintWall.Editor_SetInvalid(false);
                    }
                }
                
                // 부스 카운팅
                if (type == typeof(WitchBooth))
                {
                    var booth = (WitchBooth) behaviour;
                    booth.Editor_SetIndex(boothCount++);
                }
                // 구걸함 카운팅
                if (type == typeof(WitchBegging))
                {
                    var begging = (WitchBegging)behaviour;
                    begging.Editor_SetIndex(beggingCount++);
                }
                
                // Crafting Display 카운딩
                if (type == typeof(WitchCraftingDisplay))
                {
                    var craftingDisplay = (WitchCraftingDisplay)behaviour;
                    craftingDisplay.Editor_SetIndex(craftingCount++);
                }

                // Specific Display 카운딩
                if (type == typeof(WitchSpecificDisplay))
                {
                    var display = (WitchSpecificDisplay)behaviour;
                    display.Editor_SetIndex(specificCount++);
                }

                if (type == typeof(WitchGoodsDisplay))
                {
                    var display = (WitchGoodsDisplay)behaviour;
                    display.Editor_SetIndex(goodsCount++);
                }
            }
            
            // 액자 인덱스 세팅
            for (var i = 0; i < displayPhotos.Count; i++) 
                displayPhotos[i].Editor_SetIndex(i);
            //비디오 인덱스 세팅
            for (var i = 0; i < displayVideos.Count; i++)
                displayVideos[i].Editor_SetIndex(displayPhotos.Count + i);
            // 라이브 인덱스 세팅
            for (var i = 0; i < displayLive.Count; i++)
                displayLive[i].Editor_SetIndex(displayPhotos.Count + displayVideos.Count + i);
        }

        [UnityEditor.MenuItem("GameObject/WitchToolkit/BlockManager", false, 0)]
        private static void CreateDefaultObject(UnityEditor.MenuCommand menuCommand)
        {
            // 경고
            if (FindObjectOfType<WitchBlockManager>() != null)
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", "이미 블록 매니저가 있습니다.", "OK");
                return;
            }
            
            // 오브젝트 생성
            var blockManager = new GameObject("Witch Block Manager").AddComponent<WitchBlockManager>();
            var behaviours = new GameObject("Behaviours");
            
            // 포탈 생성
            WitchPortal.CreateDefaultObject(menuCommand);
            var entrance = UnityEditor.Selection.activeObject as WitchPortal;
            WitchPortal.CreateDefaultObject(menuCommand);
            var exit = UnityEditor.Selection.activeObject as WitchPortal;
            
            // 부모 설정
            UnityEditor.GameObjectUtility.SetParentAndAlign(blockManager.gameObject, null);
            UnityEditor.GameObjectUtility.SetParentAndAlign(behaviours, blockManager.gameObject);
            UnityEditor.GameObjectUtility.SetParentAndAlign(entrance.gameObject, behaviours);
            UnityEditor.GameObjectUtility.SetParentAndAlign(exit.gameObject, behaviours);
            
            // 설정
            entrance.gameObject.name = "Witch Portal - Entrance";
            exit.gameObject.name = "Witch Portal - Exit";
            exit.transform.localPosition = Vector3.forward * 3f;
            
            blockManager.EntrancePortal = entrance;
            blockManager.ExitPortal = exit;

            // 4. 생성한 오브젝트를 선택한다.
            UnityEditor.Selection.activeObject = blockManager;

            // 위치 비헤이비어 초기화
            blockManager.FindWitchBehaviours("", "");
        }
#endif
    }
}