﻿using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.Tool;
using WitchCompany.Toolkit.Editor.Tool.API;

namespace WitchCompany.Toolkit.Editor
{
    public static class KmTest
    {
        [MenuItem("WitchToolkit/Test_Login")]
        public static async void Login()
        {
            var json = await WitchAPI.Login("kmkim@witchcompany.io", "1998Kimin!");
            Debug.Log(JsonConvert.SerializeObject(json));
        }
        
        [MenuItem("WitchToolkit/Test_Refresh")]
        public static async void Refresh()
        {
            var json = await WitchAPI.Refresh();
            Debug.Log(JsonConvert.SerializeObject(json));
        }
        
        [MenuItem("WitchToolkit/Test_GetUserInfo")]
        public static async void GetUserInfo()
        {
            var json = await WitchAPI.GetUserInfo();
            Debug.Log(JsonConvert.SerializeObject(json));
        }
        
        [MenuItem("WitchToolkit/Test_BuildBundle")]
        public static async void BuildBundle()
        {
            var option = AssetTool.GetSelectedAsset() as BlockPublishOption;
            WitchToolkitPipeline.UploadWithValidation(option);
            
        }
        
        [MenuItem("WitchToolkit/Test_KM")]
        public static async void Test_KM()
        {
            var option = AssetTool.GetSelectedAsset() as BlockPublishOption;
            var path = AssetTool.GetAssetPath(option.TargetScene);
            Debug.Log(path);
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
    }
}