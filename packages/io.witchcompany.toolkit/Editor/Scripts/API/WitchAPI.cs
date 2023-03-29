﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Proyecto26;
using UnityEngine;
using UnityEngine.Networking;
using WitchCompany.Core.Runtime;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.Configs.Enum;
using WitchCompany.Toolkit.Editor.DataStructure;

namespace WitchCompany.Toolkit.Editor.Tool.API
{
    public static partial class WitchAPI
    {
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

            if (!response.success) return null;
            
            AuthConfig.Auth = response.payload;
            return response.payload;
        }
        
        public static void Logout()
        {
            AuthConfig.Auth = new JAuth();
        }

        public static async UniTask<JUserInfo> GetUserInfo()
        {
            var auth = AuthConfig.Auth;
            if (string.IsNullOrEmpty(auth?.accessToken)) return null;
            
            var response = await AuthSafeRequest<JUserInfo>(new RequestHelper
            {
                Method = "POST",
                Uri = ApiConfig.URL("user/auth/unity"),
                Headers = ApiConfig.TokenHeader(auth.accessToken)
            });

            return response.success ? response.payload : null;
        }

        public static async UniTask<JAuth> Refresh()
        {
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

        public static async UniTask<bool> UploadBlock(BlockPublishOption option, JManifest manifest)
        {
            var auth = AuthConfig.Auth;
            if (string.IsNullOrEmpty(auth?.accessToken)) return false;
            
            var bundlePath = Path.Combine(AssetBundleConfig.BuildExportPath, option.BundleKey);
            var thumbnailPath = Path.Combine(AssetBundleConfig.BuildExportPath, option.ThumbnailKey);
            
            var bundleData = await File.ReadAllBytesAsync(bundlePath);
            var thumbnailData = await File.ReadAllBytesAsync(thumbnailPath);
            var form = new List<IMultipartFormSection>
            {
                //new MultipartFormDataSection("json", body, "application/json"),
                new MultipartFormFileSection("file1", bundleData, option.BundleKey, "application/octet-stream"),
                new MultipartFormFileSection("file2", thumbnailData, option.ThumbnailKey, "image/jpg")
            };
            //
            // var response = await Request<JAuth>(new RequestHelper
            // {
            //     Method = "POST",
            //     Uri = ApiConfig.URL("toolkit/create"),
            //     Headers = ApiConfig.TokenHeader(auth.refreshToken),
            //     BodyString = JsonConvert.SerializeObject(new Dictionary<string, string>
            //     {
            //         ["accessAt"] = "world"
            //     }),
            //     ContentType = ApiConfig.ContentType.Json
            // });
            //
            //return response.success ? response.payload : null;

            return false;
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
                    res = await Request<T>(helper);
                }
            }
            // 만료가 아닐 경우,
            return res;
        }

        private static async UniTask<JResponse<T>> Request<T>(RequestHelper helper)
        {
            using var request = new UnityWebRequest();
            
            request.method = helper.Method;
            request.url = helper.Uri;
            foreach (var (key, value) in helper.Headers) 
                request.SetRequestHeader(key, value);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (!string.IsNullOrEmpty(helper.BodyString))
            {
                var bytes = Encoding.UTF8.GetBytes(helper.BodyString.ToCharArray());
                request.uploadHandler = new UploadHandlerRaw(bytes);

                var contentType = string.IsNullOrEmpty(helper.ContentType)
                    ? ApiConfig.ContentType.Json
                    : helper.ContentType; 
                request.uploadHandler.contentType = contentType;
            }

            var baseRequestLog = $"{helper.Method} Request ({helper.Uri})\n";
            
            Log( baseRequestLog + $"{helper.BodyString}");

            await request.SendWebRequest();
            
            if(request.result == UnityWebRequest.Result.Success)
                Log(baseRequestLog + $"{request.downloadHandler?.text}");
            else
                LogErr(baseRequestLog + $"Failed: {request.error}");

            if (request.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(request.downloadHandler?.text))
                return JsonConvert.DeserializeObject<JResponse<T>>(request.downloadHandler.text);
            else
                return null;
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