using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.DataStructure.Admin;
using WitchCompany.Toolkit.Editor.DataStructure.Item;
using WitchCompany.Toolkit.Editor.Tool;
using WitchCompany.Toolkit.Editor.Validation;
using JUnityKey = WitchCompany.Toolkit.Editor.DataStructure.JUnityKey;

namespace WitchCompany.Toolkit.Editor.API
{
    public static partial class WitchAPI
    {
        /// <summary>
        /// 로그인 api
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async UniTask<JAuth> Login(string email, string password)
        {
            var response = await Request<JAuth>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("user/login/ww"),
                BodyString = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    ["accessAt"] = "world",
                    ["blockName"] = "",
                    ["ip"] = "unity_editor",
                    ["email"] = email,
                    ["password"] = password
                }),
                ContentType = ApiConfig.ContentType.Json
            });

            return !response.success ? null : response.payload;
        }
        
        public static void Logout()
        {
            AuthConfig.Auth = new JAuth();
        }

        /// <summary>
        /// 유저 정보 조회 api
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        public static async UniTask<JUserInfo> GetUserInfo(JAuth auth)
        {
            if (string.IsNullOrEmpty(auth?.accessToken)) return null;
            
            var response = await AuthSafeRequest<JUserInfo>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("user/auth/unity"),
                Headers = ApiConfig.TokenHeader(auth.accessToken)
            });

            return response.success ? response.payload : null;
        }


        /// <summary>
        /// 1성공, -1로그인 필요, -2권한 필요
        /// </summary>
        public static async UniTask<int> CheckPermission()
        {
            var auth = AuthConfig.Auth;
            if (string.IsNullOrEmpty(auth?.accessToken)) return -1;
            
            var response = await AuthSafeRequest<JPermission>(new RequestHelper
            {
                Method = "GET",
                Uri = ApiConfig.URL("v2/toolkits/permission"),
                Headers = ApiConfig.TokenHeader(auth.accessToken)
            });

            if (response.success && response.payload != null)
                return response.payload.isUploadAble ? 1 : -2;
            
            return -2;
        }



        /// <summary>
        /// 유니티 키 생성 (번들 업로드)
        /// 성공(1), 실패(-1), pathName 중복(-2)
        /// </summary>
        public static async UniTask<int> UploadBundle(BlockPublishOption option, List<JBundleInfo> bundleInfos, JRankingKey rankingKey)
        {
            var auth = AuthConfig.Auth;
            if (string.IsNullOrEmpty(auth?.accessToken)) return -1;
            
            var blockData = new JUnityKeyInfo
            {
                pathName = option.Key,
                theme = option.blockType.ToString().ToLower(),
                capacity = PublishConfig.Capacity,
                isOfficial = PublishConfig.Official ? 1 : 0,
                unityKeyDetail = AssetDataValidator.GetAssetData().Values.ToList(),
                isPrivate = option.blockType == BlockType.Brand,
                gameUnityKey = rankingKey
            };
            
            var bundleData = new JUnityKey
            {
                blockData = blockData,
                bundleInfoList = bundleInfos
            };
            
            // Json
            var jsonData = JsonConvert.SerializeObject(bundleData);

            // thumbnail
            var thumbnailData = await File.ReadAllBytesAsync(PublishConfig.ThumbnailPath);

            // bundle
            var standaloneBundlePath = Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Standalone, option.BundleKey);
            var webglBundlePath = Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Webgl, option.BundleKey);
            var webglMobileBundlePath = Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.WebglMobile, option.BundleKey);
            var androidBundlePath = Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Android, option.BundleKey);
            var iosBundlePath = Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Ios, option.BundleKey);
            
            var standaloneBundleData = await FileTool.GetByte(standaloneBundlePath);
            var webglBundleData = await FileTool.GetByte(webglBundlePath);
            var webglMobileBundleData = await FileTool.GetByte(webglMobileBundlePath);
            var androidBundleData = await FileTool.GetByte(androidBundlePath);
            var iosBundleData = await FileTool.GetByte(iosBundlePath);
            
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("json", jsonData, "application/json"),
                new MultipartFormFileSection("image", thumbnailData, option.ThumbnailKey, "image/jpg"),
                new MultipartFormFileSection(AssetBundleConfig.Standalone, standaloneBundleData, option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.Webgl, webglBundleData, option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.WebglMobile, webglMobileBundleData, option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.Android, androidBundleData, option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.Ios, iosBundleData, option.BundleKey, "")
            };

            #region Vr 주석
            
            // var vrBundlePath = Path.Combine(AssetBundleConfig.BundleExportPath, AssetBundleConfig.Vr, option.BundleKey);
            // var vrBundleData = await GetByte(vrBundlePath);
            // form.Add(new MultipartFormFileSection(AssetBundleConfig.Vr, vrBundleData, option.BundleKey, ""));
            
            #endregion
            
            var response = await Request<JUnityKey>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("v3/toolkits/unity-key"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                FormSections = form
            });
            
            if (response.statusCode == 200) return 1;
            if (response.statusCode == 409) return -2;  
            return -1;
        }


        /// <summary>
        /// 유니티 키 생성 api (v4)
        /// </summary>
        /// <returns></returns>
        public static async UniTask<JUnityKey> CreateUnityKey(JUnityKey unityKey, Dictionary<string, byte[]> bundleDict, byte[] thumbnail, BlockPublishOption option)
        {
            var auth = AuthConfig.Auth;
            var jsonData = JsonConvert.SerializeObject(unityKey);

            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("json", jsonData, "application/json"),
                new MultipartFormFileSection("image", thumbnail, option.ThumbnailKey, "image/jpg"),
                new MultipartFormFileSection(AssetBundleConfig.Standalone, bundleDict[AssetBundleConfig.Standalone], option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.Webgl, bundleDict[AssetBundleConfig.Webgl], option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.WebglMobile, bundleDict[AssetBundleConfig.WebglMobile], option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.Android, bundleDict[AssetBundleConfig.Android], option.BundleKey, ""),
                new MultipartFormFileSection(AssetBundleConfig.Ios, bundleDict[AssetBundleConfig.Ios], option.BundleKey, "")
            };
            
            var response = await AuthSafeRequest<JUnityKey>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("v4/toolkits/unity-key"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                FormSections = form
            });

            return response.success ? response.payload : null;
        }

        /// <summary>
        /// 유니티 키 번들 수정 api
        /// </summary>
        /// <param name="auth"></param>
        /// <param name="unityKeyId"></param>
        /// <param name="fileName"></param>
        /// <param name="bundleInfos"></param>
        /// <param name="bundleBytes"></param>
        /// <returns></returns>
        public static async UniTask<JUnityKey> UpdateBundle(int unityKeyId, string fileName, List<JBundleInfo> bundleInfos, Dictionary<string, byte[]> bundleBytes)
        {
            var auth = AuthConfig.Auth;
            var unityKey = new JUnityKey { bundleInfoList = bundleInfos };
            var jsonData = JsonConvert.SerializeObject(unityKey);
            
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("json", jsonData, "application/json"),
                new MultipartFormFileSection(AssetBundleConfig.Standalone, bundleBytes[AssetBundleConfig.Standalone], fileName, ""),
                new MultipartFormFileSection(AssetBundleConfig.Webgl, bundleBytes[AssetBundleConfig.Webgl], fileName, ""),
                new MultipartFormFileSection(AssetBundleConfig.WebglMobile, bundleBytes[AssetBundleConfig.WebglMobile], fileName, ""),
                new MultipartFormFileSection(AssetBundleConfig.Android, bundleBytes[AssetBundleConfig.Android], fileName, ""),
                new MultipartFormFileSection(AssetBundleConfig.Ios, bundleBytes[AssetBundleConfig.Ios], fileName, "")
            };
            
            var response = await AuthSafeRequest<JUnityKey>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL($"v4/toolkits/unity-key/{unityKeyId}/bundles"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                FormSections = form
            });
            
            return response.success ? response.payload : null;
        }

        public static async UniTask<bool> UpdateThumbnail(int unityKeyId, BlockPublishOption option, byte[] thumbnail)
        {
            var auth = AuthConfig.Auth;
            var unityKeyInfo = new JUnityKeyInfo
            {
                unityKeyId = unityKeyId,
                pathName = option.Key
            };
            var jsonData = JsonConvert.SerializeObject(unityKeyInfo);

            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("json", jsonData, "application/json"),
                new MultipartFormFileSection("image", thumbnail, option.ThumbnailKey, "image/jpg"),
            };
            
            var response = await AuthSafeRequest<JUnityKey>(new RequestHelper
            {
                Method = "PUT",
                Uri = ApiConfig.URL("v4/toolkits/unity-key"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                FormSections = form
            });

            return response.success;
        }


        /// <summary>
        /// 유니티 키 디테일 수정
        /// </summary>
        /// <param name="unityKeyId"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public static async UniTask<bool> UpdateUnityKeyDetail(int unityKeyId, List<JUnityKeyDetail> details, bool isUpdate)
        {
            var auth = AuthConfig.Auth;
            var unityKeyInfo = new JUnityKeyInfo { unityKeyDetail = details };
            
            var jsonData = JsonConvert.SerializeObject(unityKeyInfo);
            
            var response = await AuthSafeRequest<JUnityKeyInfo>(new RequestHelper
            {
                Method = isUpdate ? "PUT" : "POST",
                Uri = ApiConfig.URL($"v4/toolkits/unity-key/{unityKeyId}/detail"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                BodyString = jsonData
            });

            return response.success;
        }
        
        /// <summary>
        /// 유니티 키 랭킹 키 수정
        /// </summary>
        /// <returns></returns>
        public static async UniTask<bool> UpdateRankingKey(int unityKeyId, JRankingKey rankingKey)
        {
            var auth = AuthConfig.Auth;

            var data = new JUnityKeyInfo
            {
                gameUnityKey = rankingKey
            };

            var response = await AuthSafeRequest<JUnityKeyInfo>(new RequestHelper
            {
                Method = "PUT",
                Uri = ApiConfig.URL($"v4/toolkits/unity-key/{unityKeyId}/game"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                BodyString = JsonConvert.SerializeObject(data),
            });

            return response.success;
        }
        
        
        /// <summary>
        /// 유니티 키 컬렉션, 콘서트 정보 수정
        /// </summary>
        /// <returns></returns>
        public static async UniTask<JUnityKey> UpdateCollectionAndConcert(int unityKeyId, JCollectionData collectionData, JBlockLocationInfo blockLocationInfo)
        {
            var auth = AuthConfig.Auth;

            var data = new JUnityKey
            {
                collectionBundleData = collectionData,
                concertBundleData = blockLocationInfo
            };
            
            var response = await AuthSafeRequest<JUnityKey>(new RequestHelper
            {
                Method = "PUT",
                Uri = ApiConfig.URL($"v4/toolkits/unity-key/{unityKeyId}/theme"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                BodyString = JsonConvert.SerializeObject(data),
            });

            return response.success ? response.payload : null;
        }
        
        
        /// <summary>
        /// 유니티 키 이름으로 조회 (v4)
        /// 0 : 유니티 키 존재하지 않을 경우
        /// 1 : 유니티 키 존재할 경우
        /// </summary>
        public static async UniTask<JUnityKey> GetUnityKey(string pathname)
        {
            var response = await AuthSafeRequest<JUnityKey>(new RequestHelper
            {
                Method = "GET",
                Uri = ApiConfig.URL($"v4/toolkits/unity-key/{pathname}")
            });
            
            return response.success ? response.payload :  null;
        }
        
        
        
        
        /// <summary>
        /// 랭킹보드 keys 업로드
        /// </summary>
        /// <param name="rankingData"></param>
        /// <returns></returns>
        public static async UniTask<bool> SetRankingKeys(int blockId, List<JRankingKey> rankingKeys)
        {
            var auth = AuthConfig.Auth;
            if (string.IsNullOrEmpty(auth?.accessToken)) return false;

            var rankingData = new JRankingData
            {
                blockId = blockId,
                rankingKeys = rankingKeys
            };
            
            var response = await Request<JRankingData>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("v2/blocks/rank/keys"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                BodyString = JsonConvert.SerializeObject(rankingData)
            });
            
            return response.success;
        }

        public static async UniTask<bool> CheckValidItem(int itemKey)
        {
            
            var response = await Request<JItemDetail>(new RequestHelper
            {
                Method = "GET",
                Uri = ApiConfig.URL($"item/detail/{itemKey}")
            });
            
            return response.success;
        }
        

        private static async UniTask<(string key, byte[] data)> GetBundleData(string name, string folderPath, string platform)
        {
            var key = $"{name}_{platform}.bundle".ToLower();
            var bundlePath = Path.Combine(folderPath, key);

            var data = await FileTool.GetByte(bundlePath);

            if (data == null || data.Length == 0)
            {
                throw new FileNotFoundException("번들 파일을 찾을 수 없습니다.");
            }
            
            return (key, data);
        }
        
        
        /// <summary>
        /// 아이템 유니티 키 생성 및 수정 api
        /// </summary>
        /// <param name="itemBundleData"></param>
        /// <param name="bundleFolderPath"></param>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public static async UniTask<bool> UploadItemData(JItemBundleData itemBundleData, string bundleFolderPath, string modelPath)
        {
            var auth = AuthConfig.Auth;
            var jsonData = JsonConvert.SerializeObject(itemBundleData);
            var itemData = itemBundleData.itemData;
            
            // bundle
            var standaloneBundle = await GetBundleData(itemData.name, bundleFolderPath, AssetBundleConfig.Standalone);
            var webglBundle = await GetBundleData(itemData.name, bundleFolderPath, AssetBundleConfig.Webgl);
            var webglMobileBundle = await GetBundleData(itemData.name, bundleFolderPath, AssetBundleConfig.WebglMobile);
            var androidBundle = await GetBundleData(itemData.name, bundleFolderPath, AssetBundleConfig.Android);
            var iosBundle = await GetBundleData(itemData.name, bundleFolderPath, AssetBundleConfig.Ios);

            return false;
            
            // gltf
            var gltfName = modelPath.Split("/")[^1];
            var gltfData = await FileTool.GetByte(modelPath);
            
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormDataSection("json", jsonData, "application/json"),
                new MultipartFormFileSection("model", gltfData, gltfName, ""),
                new MultipartFormFileSection(AssetBundleConfig.Standalone, standaloneBundle.data, standaloneBundle.key, ""),
                new MultipartFormFileSection(AssetBundleConfig.Webgl, webglBundle.data, webglBundle.key, ""),
                new MultipartFormFileSection(AssetBundleConfig.WebglMobile, webglMobileBundle.data, webglMobileBundle.key, ""),
                new MultipartFormFileSection(AssetBundleConfig.Android, androidBundle.data, androidBundle.key, ""),
                new MultipartFormFileSection(AssetBundleConfig.Ios, iosBundle.data, iosBundle.key, ""),
            };
            
            var response = await Request<JBlockStatus>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("v2/toolkits/item/unity-key"),
                Headers = ApiConfig.TokenHeader(auth.accessToken),
                FormSections = form
            });

            return response != null && response.success;
        }
    }
    
    public static partial class WitchAPI
    {
        private static async UniTask<JResponse<T>> AuthSafeRequest<T>(RequestHelper helper)
        {
            var res = await Request<T>(helper);
            
            // 토큰 만료일 경우,
            if (res.statusCode == 401)
            {
                var auth = await Refresh();
                
                // 실패
                if (auth == null)
                {
                    LogErr("토큰 리프래쉬 실패");
                    return res;
                }
                // 성공
                else
                {
                    Log("토큰 리프래쉬 성공");
                    
                    // 토큰 저장
                    AuthConfig.Auth = auth;
                    
                    // 요청 재시도
                    helper.Headers = ApiConfig.TokenHeader(auth.accessToken);
                    res = await Request<T>(helper);
                }
            }
            // 만료가 아닐 경우,
            return res;
        }
        
        public static async UniTask<JAuth> Refresh()
        {
            Log("토큰 리프래쉬 요청");

            var auth = AuthConfig.Auth;

            if (string.IsNullOrEmpty(auth.refreshToken))
                return null;

            var response = await Request<JAuth>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("user/refresh/ww"),
                Headers = ApiConfig.TokenHeader(auth.refreshToken),
                BodyString = JsonConvert.SerializeObject(new Dictionary<string, string>
                {
                    ["accessAt"] = "world"
                }),
                ContentType = ApiConfig.ContentType.Json
            });

            return response.success ? response.payload : null;
        }


        private static string GetFormSectionsContentType(out byte[] bodyRaw, RequestHelper options)
        {
            var boundary = UnityWebRequest.GenerateBoundary();
            var formSections = UnityWebRequest.SerializeFormSections(options.FormSections, boundary);
            var terminate = Encoding.UTF8.GetBytes(string.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));
            
            bodyRaw = new byte[formSections.Length + terminate.Length];
            
            Buffer.BlockCopy(formSections, 0, bodyRaw, 0, formSections.Length);
            Buffer.BlockCopy(terminate, 0, bodyRaw, formSections.Length, terminate.Length);
            
            return string.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
        }
        
        private static async UniTask<JResponse<T>> Request<T>(RequestHelper helper)
        {
            using var request = new UnityWebRequest();
            
            try
            {
                // 리퀘스트 생성
                request.method = helper.Method;
                request.url = helper.Uri;
                foreach (var (key, value) in helper.Headers)
                    request.SetRequestHeader(key, value);
                request.downloadHandler = new DownloadHandlerBuffer();

                var contentType = ApiConfig.ContentType.Json;
                var bodyRaw = helper.BodyRaw;
                
                if (!string.IsNullOrEmpty(helper.BodyString))
                {
                    bodyRaw = Encoding.UTF8.GetBytes(helper.BodyString.ToCharArray());

                    contentType = string.IsNullOrEmpty(helper.ContentType)
                        ? ApiConfig.ContentType.Json
                        : helper.ContentType;
                    
                    Log($"{helper.Method} Request ({helper.Uri})\n" + $"{contentType}\n{helper.BodyString}");
                }
                else if (helper.FormSections is {Count: > 0})
                {
                    contentType = GetFormSectionsContentType(out bodyRaw, helper);

                    var builder = new StringBuilder();
                    builder.Append($"{helper.Method} Request ({helper.Uri})\n");
                    builder.Append($"{contentType}\n");
                    foreach (var section in helper.FormSections)
                    {
                        var sizeMb = CommonTool.ByteToMb(section.sectionData.LongLength, 4);
                        if (section.sectionName == "json")
                            builder.Append($"{section.sectionName}({section.contentType}):\n" + Encoding.UTF8.GetString(section.sectionData));
                        else
                            builder.Append($" {section.sectionName}: {section.fileName}({sizeMb}mb); {section.contentType}\n");
                    }

                    Log(builder.ToString());
                }

                //request.SetRequestHeader("Content-Type", contentType);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.uploadHandler.contentType = contentType;

                // 웹 리퀘스트 전송
                await request.SendWebRequest();

                // 예외처리
                if (request.result != UnityWebRequest.Result.Success || string.IsNullOrEmpty(request.downloadHandler?.text)) 
                    throw new UnityWebRequestException(request);
               
                // 성공
                Log($"{helper.Method} Response ({helper.Uri})\n" + $"{request.downloadHandler?.text}");
                return JsonConvert.DeserializeObject<JResponse<T>>(request.downloadHandler.text);
            }
            catch (Exception)
            {
                LogErr($"{helper.Method} Response ({helper.Uri})\n" + $"Failed: {request.error}");

                Log(JsonConvert.DeserializeObject<JResponse<T>>(request.downloadHandler.text).message);
                
                return new JResponse<T>
                {
                    message = request.error,
                    statusCode = (int)request.responseCode,
                    success = false
                };
            }
        }

        private static void Log(string msg)
        {
            if(ToolkitConfig.CurrLogLevel.HasFlag(LogLevel.API))
                Debug.Log(msg);
        }
        
        private static void LogErr(string msg)
        {
            if(ToolkitConfig.CurrLogLevel.HasFlag(LogLevel.API))
                Debug.LogError(msg);
        }
    }
}