using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Module;

namespace WitchCompany.Toolkit.Editor.Validation
{
    public static class AssetDataValidator
    {
        private static JUnityKeyDetail art = new("art");
        private static JUnityKeyDetail video = new("video");
        private static JUnityKeyDetail freeArt = new("freeArt");
        private static JUnityKeyDetail craftingArt = new("craftingArt");
        private static JUnityKeyDetail posting = new("posting");
        private static JUnityKeyDetail doodling = new("doodling");
        private static JUnityKeyDetail ranking = new("ranking");
        private static JUnityKeyDetail stall = new("stall");
        private static JUnityKeyDetail auctionBooth = new("auctionBooth");
        private static JUnityKeyDetail beggingBooth = new("beggingBooth");
        
        private static Dictionary<string, JUnityKeyDetail> assetData = new ()
        {
            {"art", art},
            {"video", video},
            {"posting", posting},
            {"doodling", doodling},
            {"ranking", ranking},
            {"freeArt", freeArt},
            {"stall", stall},
            {"auctionBooth", auctionBooth},
            {"beggingBooth", beggingBooth},
            {"craftingArt", craftingArt}
        };
        
        private static void Initialize()
        {
            foreach (var key in assetData.Keys.ToList())
                assetData[key].count = 0;
        }
        
        public static Dictionary<string, JUnityKeyDetail> GetAssetData()
        {
            Initialize();
            
            var transforms = GameObject.FindObjectsOfType<Transform>(true);

            foreach (var transform in transforms)
            {
                // 전시
                if (transform.TryGetComponent(out WitchDisplayFrame displayFrame))
                {
                    // 사진
                    if (displayFrame.MediaType == MediaType.Picture)
                        art.count++;
                    // 비디오
                    else if (displayFrame.MediaType is MediaType.Video or MediaType.LiveVideo)
                        video.count++;
                }
                // 자유 배치 에셋
                if (transform.TryGetComponent(out WitchFreeDisplay freeDisplay))
                    freeArt.count++;
                
                // 크래프팅 배치 에셋
                if (transform.TryGetComponent(out WitchCraftingDisplay craftingDisplay))
                    craftingArt.count++;

                // 가판대 에셋
                if (transform.TryGetComponent(out WitchStallDisplay stallDisplay))
                    stall.count++;
                
                // 낙서장
                if (transform.TryGetComponent(out WitchPaintWall paintWall))
                    doodling.count++;
                
                // 포스트잇
                if (transform.TryGetComponent(out WitchPostItWall postItWall))
                    posting.count++;
                
                // 랭킹보드
                if (transform.TryGetComponent(out WitchLeaderboard leaderboard))
                    ranking.count++;
                
                // 구걸 부스, 경매 부스
                if (transform.TryGetComponent(out WitchBooth witchBooth))
                {
                    if (witchBooth.BoothType == BoothType.Market)
                        auctionBooth.count++;
                    else if (witchBooth.BoothType == BoothType.Begging)
                        beggingBooth.count++;
                }
            }
            return assetData;
        }

        public static List<JUnityKeyDetail> GetUnityKeyDetails()
        {
            GetAssetData();

            return assetData.Values.Where(asset => asset.count > 0).ToList();
        }
        
        public static List<JUnityKeyDetail> GetUnityKeyDetails(List<JUnityKeyDetail> details)
        {
            if (details == null) return null;
            
            GetAssetData();
            
            var newDetails = new List<JUnityKeyDetail>();
            // 업데이트
            foreach (var detail in details)
            {
                assetData[detail.unityKeyType].unityKeyDetailId = detail.unityKeyDetailId;
                newDetails.Add( assetData[detail.unityKeyType]);
            }
            return newDetails;
        }

        public static List<JUnityKeyDetail> GetCreateUnityKeyDetails(List<JUnityKeyDetail> details)
        {
            GetAssetData();
            
            var keys = assetData.Keys.ToList();

            if (details != null)
            {
                foreach (var detail in details)
                {
                    keys.Remove(detail.unityKeyType);
                }
            }
            
            // detail에 없는 값을 리스트로 전환
            var unityKeyDetails = new List<JUnityKeyDetail>();
            foreach (var key in keys)
            {
                if(assetData[key].count > 0)
                    unityKeyDetails.Add(assetData[key]);
            }

            return unityKeyDetails;
        }
    }
}