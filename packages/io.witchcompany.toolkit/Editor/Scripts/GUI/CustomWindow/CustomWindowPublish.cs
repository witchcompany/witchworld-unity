using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WitchCompany.Toolkit.Editor.API;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.Tool;
using WitchCompany.Toolkit.Editor.Validation;
using WitchCompany.Toolkit.Module;
using WitchCompany.Toolkit.Scripts.WitchBehaviours.Interface;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public static class CustomWindowPublish
    {
        private static BuildTargetGroup blockPlatform;
        private static JBuildReport buildReport;
        private static string[] bundleTypes =
        {
            AssetBundleConfig.Standalone,
            AssetBundleConfig.Webgl,
            AssetBundleConfig.WebglMobile,
            AssetBundleConfig.Android,
            AssetBundleConfig.Ios,
            // AssetBundleConfig.Vr
        };

        private static PlatformType[] platformTypes =
        {
            PlatformType.Standalone,
            PlatformType.Webgl,
            PlatformType.WebglMobile,
            PlatformType.Android,
            PlatformType.Ios,
            PlatformType.Vr,
        };
        
        public static void ShowPublish()
        {
            // 빌드 정보
            DrawPublish();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Publish"))
            {
                OnClickPublish().Forget();
            }

            if (buildReport != null)
                DrawReport();
        }

        /// <summary>
        /// 빌드 설정
        /// </summary>
        private static void DrawPublish()
        {
            GUILayout.Label("Publish", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Scene
                var blockScene = EditorGUILayout.ObjectField("Scene", PublishConfig.Scene, typeof(SceneAsset), false) as SceneAsset;
                    PublishConfig.Scene = blockScene;

                if (GUILayout.Button("Active Scene", GUILayout.Width(100)))
                {
                    var activeScenePath = SceneManager.GetActiveScene().path;
                    PublishConfig.Scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScenePath);
                }
            }

            // 썸네일
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Thumbnail", PublishConfig.ThumbnailPath, EditorStyles.textField);
                if (GUILayout.Button("Select", GUILayout.Width(100)))
                {
                    PublishConfig.ThumbnailPath = EditorUtility.OpenFilePanel("Witch Creator Toolkit", "", "jpg");
                }
            } 
            
            // 인원 
            using (new EditorGUILayout.HorizontalScope())
            {
                PublishConfig.Capacity = EditorGUILayout.IntField("Capacity ", PublishConfig.Capacity);
                if (PublishConfig.Capacity < 1) PublishConfig.Capacity = 1;
                if (PublishConfig.Capacity > 20) PublishConfig.Capacity = 20;
            
                // GUILayout.Label("1~20", CustomWindow.LabelTextStyle);
            }
            
            // 공식 여부
            PublishConfig.Official = EditorGUILayout.Toggle("Official", PublishConfig.Official);
            
            // 테마
            var blockTheme = (BlockType)EditorGUILayout.EnumPopup("Block Type", PublishConfig.BlockType);
            PublishConfig.BlockType = blockTheme;
            
            
            // 테마 타입에 따른 옵션 
            EditorGUI.indentLevel++;
            switch (PublishConfig.BlockType)
            {
                case BlockType.Game :
                    var blockLevel = (GameLevel)EditorGUILayout.EnumPopup("Level", PublishConfig.Level);
                    PublishConfig.Level = blockLevel;
                    break;
                case BlockType.Collection :
                    PublishConfig.Collection = EditorGUILayout.IntField("Collection", PublishConfig.Collection);
                    break;
                case BlockType.Concert:
                    PublishConfig.ItemCa = EditorGUILayout.TextField("ItemCA", PublishConfig.ItemCa);
                    PublishConfig.ItemLocationId = EditorGUILayout.IntField("Item Location Id", PublishConfig.ItemLocationId);
                    break;
                default: break;
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Publish 버튼 클릭 후 실행되는 함수
        /// - Scene 번들 추출
        /// - 유니티 키 업로드
        /// - 업로드 결과 표시
        /// </summary>
        private static async UniTaskVoid OnClickPublish()
        {
            try
            {
                // 썸네일 지정 확인
                if (string.IsNullOrEmpty(PublishConfig.ThumbnailPath))
                {
                    EditorUtility.DisplayDialog("Witch Creator Toolkit", AssetBundleConfig.ThumbnailMsg, "OK");
                    return;
                }
            
                // 입력 제한 실행
                CustomWindow.IsInputDisable = true;
            
                // 번들 추출
                buildReport = WitchToolkitPipeline.PublishWithValidation(GetOption());
            
                if (buildReport.result == JBuildReport.Result.Success)
                {
                    EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Uploading to server...", 1.0f);
            
                    // 번들 업로드
                    var result = await UploadBundle();
                    var resultMsg = result ? AssetBundleConfig.SuccessMsg : AssetBundleConfig.FailedPublishMsg; 
                    
                    EditorUtility.DisplayDialog("Witch Creator Toolkit", resultMsg, "OK");
                    EditorUtility.ClearProgressBar();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
            // 입력 제한 해제 
            CustomWindow.IsInputDisable = false;
        }
        
        /// <summary>
        /// 유니티 키 업로드
        /// </summary>
        /// <returns></returns>
        private static async UniTask<bool> UploadBundle()
        {
            var option = GetOption();

            // 유니티 키 조회
            var curUnityKey = await WitchAPI.GetUnityKey(option.Key);

            var result = false;
            // 유니티 키 없을 경우 생성
            if (curUnityKey == null)
            {
                result = await CreateUnityKey(option);
            
                Debug.Log($"유니티 키 생성 결과 : {result}");
            }
            // 유니티 키 존재할 경우 수정
            else
            {
                result = await UpdateUnityKey(curUnityKey, option);
                Debug.Log($"유니티 키 수정 결과 : {result}");
            }
            
            DeleteBundleFile(option);
            
            return result;
        }
        
        /// <summary>
        /// 유니티 키 생성
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static async UniTask<bool> CreateUnityKey(BlockPublishOption option)
        {
            // 썸네일
            var thumbnail = await FileTool.GetByte(PublishConfig.ThumbnailPath); 
            // 번들 정보 및 파일 
            var bundleData = await GetBundleData(null, option.BundleKey);
            // 게임 랭킹 키
            var rankingKey = GetRankingKey(null);
            // 에셋 개수 정보
            var details = AssetDataValidator.GetUnityKeyDetails();
            // 컬렉션 배치 아이템 정보
            var collectionData = GetCollectionData();
            // 콘서트 배치 아이템 정보
            var concertData = GetConcertData();
            
            var newKey = new JUnityKey
            {
                blockData = new JUnityKeyInfo
                {
                    pathName = option.Key,
                    theme = option.blockType.ToString().ToLower(),
                    capacity = PublishConfig.Capacity,
                    isOfficial = PublishConfig.Official ? 1 : 0,
                    isPrivate = option.blockType == BlockType.Brand,
                    gameUnityKey = rankingKey,
                    unityKeyDetail = details,
                },
                bundleInfoList = bundleData.bundleInfos,
                collectionBundleData = collectionData,
                concertBundleData = concertData,
            };
            
            var result = await WitchAPI.CreateUnityKey(newKey, bundleData.bundleBytes, thumbnail, option);
            
            return result != null;
        }

        /// <summary>
        /// 유니티 키 수정
        /// </summary>
        /// <param name="curUnityKey"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private static async UniTask<bool> UpdateUnityKey(JUnityKey curUnityKey, BlockPublishOption option)
        {
            var isSuccess = false;
            var unityKeyId = curUnityKey.unityKeyId;

            try
            {
                // 번들 업데이트
                var bundleData = await GetBundleData(curUnityKey.bundleInfoList, option.BundleKey);
                var bundleResult = await WitchAPI.UpdateBundle(unityKeyId, option.BundleKey,
                    bundleData.bundleInfos, bundleData.bundleBytes);

                if (bundleResult == null)
                    throw new Exception("번들 업데이트 실패!");
                
                // 썸네일 변경
                var thumbnail = await FileTool.GetByte(PublishConfig.ThumbnailPath);
                var thumbnailResult = await WitchAPI.UpdateThumbnail(unityKeyId, option, thumbnail);

                if (!thumbnailResult)
                    throw new Exception("썸네일 업데이트 실패");
                
                // 유니티 키 에셋 개수 정보 업데이트
                // todo : 현재 데이터랑 비교해서 추가까지??
                var unityKeyDetails = AssetDataValidator.GetUnityKeyDetails(curUnityKey.blockData.unityKeyDetail);
                if (unityKeyDetails != null)
                {
                    var detailResult = await WitchAPI.UpdateUnityKeyDetail(unityKeyId, unityKeyDetails);
                    if (!detailResult)
                        throw new Exception("유니티 키 에셋 개수 업데이트 실패!");
                }
                
                // 유니티 키 게임 랭킹 키 업데이트
                if (PublishConfig.BlockType == BlockType.Game)
                {
                    var rankingKey = GetRankingKey(curUnityKey.blockData.gameUnityKey);
                    var rankingKeyResult = await WitchAPI.UpdateRankingKey(unityKeyId, rankingKey);

                    if (!rankingKeyResult) 
                        throw new Exception("유니티 키 랭킹 키 업데이트 실패!");
                }
                
                // 유니티 콘서트, 컬렉션 정보 업데이트
                if (PublishConfig.BlockType == BlockType.Collection)
                {
                    // 컬렉션 배치 아이템 정보
                    var collectionData = GetCollectionData();
                    if(collectionData == null)
                        throw new Exception("컬렉션에 필요한 툴킷 설정 없음!");
                    
                    var result = await WitchAPI.UpdateCollectionAndConcert(unityKeyId, collectionData, null);

                    if (result == null)
                        throw new Exception("유니티 키 컬렉션 업데이트 실패!");
                }
                if (PublishConfig.BlockType == BlockType.Concert)
                {
                    // 콘서트 배치 아이템 정보
                    var concertData = GetConcertData();
                    var result = await WitchAPI.UpdateCollectionAndConcert(unityKeyId, null, concertData);

                    if (result == null)
                        throw new Exception("유니티 키 콘서트 배치 정보 업데이트 실패!");
                }

                isSuccess = true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return isSuccess;
        }
        

        /// <summary>
        /// 번들 정보 및 번들 파일 얻기
        /// </summary>
        /// <param name="bundleKey"></param>
        /// <returns></returns>
        private static async UniTask<(List<JBundleInfo> bundleInfos, Dictionary<string, byte[]> bundleBytes)> GetBundleData(List<JBundleInfo> bundleInfos, string bundleKey)
        {
            // 번들 정보
            if (bundleInfos == null)
                bundleInfos = CreateBundleInfos(bundleKey);
            else
                UpdateBundleInfos(bundleInfos, bundleKey);

            // 번들 파일
            var bundlePaths = new Dictionary<string, string>
            {
                { AssetBundleConfig.Standalone, Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Standalone, bundleKey) },
                { AssetBundleConfig.Webgl, Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Webgl, bundleKey) },
                { AssetBundleConfig.WebglMobile, Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.WebglMobile, bundleKey) },
                { AssetBundleConfig.Android, Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Android, bundleKey) },
                { AssetBundleConfig.Ios, Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Ios, bundleKey) }
            };

            var bundleDict = new Dictionary<string, byte[]>();
            foreach (var (type, path) in bundlePaths)
            {
                var bundleBytes = await FileTool.GetByte(path);
                bundleDict.Add(type, bundleBytes);
            }
            
            return (bundleInfos, bundleDict);
        }

        private static void UpdateBundleInfos(List<JBundleInfo> bundleInfos, string bundleKey)
        {
            foreach (var bundleType in bundleTypes)
            {
                var index = bundleInfos.FindIndex(info => info.bundleType == bundleType);

                UpdateBundleInfo(bundleInfos[index], bundleType, bundleKey);
            }
        }

        private static List<JBundleInfo> CreateBundleInfos(string bundleKey)
        {
            var newBundleInfos = new List<JBundleInfo>();
    
            foreach (var bundleType in bundleTypes)
            {
                var bundleInfo = new JBundleInfo();
                UpdateBundleInfo(bundleInfo, bundleType, bundleKey);
                newBundleInfos.Add(bundleInfo);
            }

            return newBundleInfos;
        }
        
        private static void UpdateBundleInfo(JBundleInfo bundleInfo, string bundleType, string bundleKey)
        {
            var manifestPath = Path.Combine(AssetBundleConfig.BundleExportPath, bundleType, bundleKey);
            var crc = AssetBundleTool.ReadManifest(manifestPath);
            
            if (crc == null) return;
            
            bundleInfo.bundleType = bundleType;
            bundleInfo.unityVersion = ToolkitConfig.UnityVersion;
            bundleInfo.toolkitVersion = ToolkitConfig.WitchToolkitVersion;
            bundleInfo.crc = crc;
        }
        

        private static JCollectionData GetCollectionData()
        {
            // 컬렉션 테마가 아닐 경우 종료
            if (PublishConfig.BlockType != BlockType.Collection) return null;
            
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            if (!rootObjects[0].TryGetComponent<WitchBlockManager>(out var manager))
                return null;
            
            // 컬렉션 테마에 자동 배치될 아이템 정보 설정
            var blockLocationInfos = new List<JBlockLocationInfo>();
            
            // 매니저의 behaviour에서 iCollection 가져오기
            foreach (var behaviour in manager.Behaviours)
            {
                if (behaviour.TryGetComponent<ICollectionDisplay>(out var collection))
                {
                    var salesId = ToolkitConfig.DeveloperMode ? collection.SalesItemIdDev : collection.SalesItemId;
                    var info = new JBlockLocationInfo
                    {
                        salesItemId = salesId,
                        blockLocationId = collection.BlockLocationId,
                    };
                    blockLocationInfos.Add(info);
                }
            }

            return new JCollectionData
            {
                collectionId = PublishConfig.Collection,
                blockLocationInfos = blockLocationInfos,
            };
        }

        private static JBlockLocationInfo GetConcertData()
        {
            // 콘서트 테마가 아닐 경우 종료
            if (PublishConfig.BlockType != BlockType.Concert) return null;
            
            return new JBlockLocationInfo
            {
                itemCa = PublishConfig.ItemCa,
                blockLocationId = PublishConfig.ItemLocationId
            };
        }
        
        public static BlockPublishOption GetOption()
        {
            return new BlockPublishOption
            {
                targetScene = PublishConfig.Scene,
                blockType = PublishConfig.BlockType,
            };
        }

        private static JRankingKey GetRankingKey(JRankingKey curRankingKey)
        {
            // 테마가 게임이 아닐 경우
            if (PublishConfig.BlockType != BlockType.Game) return null;
            
            // 랭킹 키값 설정
            var dataManager = GameObject.FindObjectOfType<WitchDataManager>(true);
            
            // 데이터 매니저 없으면 랭킹 키값 확인 안함
            if (dataManager == null || dataManager.RankingKeys.Count < 1) return null;
            
            var rankingKey = dataManager.RankingKeys[0];
            return new JRankingKey
            {
                gameInfoId = curRankingKey?.gameInfoId ?? 0,
                level = PublishConfig.Level.ToString().ToLower(),
                key = rankingKey.key,
                sortType = rankingKey.alignment.ToString().ToLower(),
                dataType = rankingKey.dataType.ToString().ToLower()
            };
        }

        private static void DeleteBundleFile(BlockPublishOption option)
        {
            // 번들 파일 삭제
            foreach (var type in bundleTypes)
            {
                var bundlePath = Path.Combine(AssetBundleConfig.BundleExportPath, type, option.BundleKey);
                var manifestPath = bundlePath + ".manifest";
                
                var typePath = Path.Combine(AssetBundleConfig.BundleExportPath, type, type);
                var typeManifestPath = typePath + ".manifest";
                
                File.Delete(bundlePath);
                File.Delete(manifestPath);
                
                File.Delete(typePath);
                File.Delete(typeManifestPath);
            }
        }

        /// <summary>
        /// 빌드 결과
        /// - 방금 빌드 결과(성공/취소/실패)
        /// - 빌드파일 아웃풋 경로 (./WitchToolkit/Bundles)
        /// - 최종 빌드파일 용량
        /// - 빌드 종료 시간
        /// </summary>
        private static void DrawReport()
        {
            GUILayout.Label("Bundle Report", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Result", buildReport.result.ToString());
            
            if (buildReport.result == JBuildReport.Result.Success)
            {
                // TODO: 플랫폼에 따라 분류
                //EditorGUILayout.LabelField("ExportPath", buildReport.exportPath);
                //EditorGUILayout.LabelField("TotalSize", $"{CommonTool.ByteToMb(buildReport.totalSizeByte, 2)} MB");
                // 시작시간
                EditorGUILayout.LabelField("StartTime", $"{buildReport.BuildStatedAt.ToString()}");
                // 종료시간
                EditorGUILayout.LabelField("EndTime", buildReport.BuildEndedAt.ToString());
                // 소요시간 
                var time = buildReport.BuildEndedAt - buildReport.BuildStatedAt;
                EditorGUILayout.LabelField("Duration", (int)time.TotalSeconds + "s");;

            }
            EditorGUILayout.EndVertical();
        }
    }
}