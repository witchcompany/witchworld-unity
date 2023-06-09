using System;
using System.IO;
using PlasticGui;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.Tool;
using WitchCompany.Toolkit.Editor.Validation;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public class CustomWindowProduct
    {
        private static Vector2 scrollPos;
        private static ValidationReport validationReport;
        private static JBuildReport buildReport;
        public static void ShowProduct()
        {
            DrawProduct();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Publish"))
            {
                OnClickPublish();
            }

            if (validationReport != null)
            {
                DrawReport();
            }
        }

        private static void DrawProduct()
        {
            GUILayout.Label("Product", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            var check = new EditorGUI.ChangeCheckScope();
            using (new EditorGUILayout.HorizontalScope())
            {
                using (check)
                {
                    var product = EditorGUILayout.ObjectField("Prefab", ProductConfig.Prefab, typeof(GameObject), false) as GameObject;
                    if (check.changed)
                        ProductConfig.Prefab = product;
                }
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                using (check)
                {
                    if (ProductConfig.Prefab != null)
                    {
                        var bytes = AssetTool.GetFileSizeByte(ProductConfig.PrefabPath);
                        var sizeKb = Math.Round((double)bytes / 1024, 3);
                        EditorGUILayout.LabelField("File Size", $"{sizeKb} / {ProductConfig.MaxProductSizeKb} KB");
                    }
                }
            }
            
            EditorGUILayout.EndVertical();
        }


        private static void DrawReport()
        {
            GUILayout.Label("Report", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Result", validationReport.result.ToString());

            if (validationReport.result == ValidationReport.Result.Success)
            {
                var path = Path.Combine(ProductConfig.BundleExportPath, ProductConfig.Prefab.name);
                EditorGUILayout.LabelField("Path", path);
            }
            else
            {
                GUILayout.Space(5);
                GUILayout.Label("Message", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
        
                // 에러 메시지 출력
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            
                var preErrorTag = "";
                foreach (var error in validationReport.errors)
                {
                    if (error == null) return;
                    
                    // 이전 tag와 값이 다르면 tag 출력
                    if (preErrorTag != error.tag)
                    {
                        GUILayout.Space(10);
                        GUILayout.Label(error.tag, EditorStyles.boldLabel);   
                        
                        preErrorTag = error.tag;
                    }

                    // 로그 종류에 따라 버튼 style 변경
                    if (error.context == null)
                        GUILayout.Label(error.message, CustomWindow.LogTextStyle);
                    else
                    {
                        if (GUILayout.Button(error.message, CustomWindow.LogButtonStyle))
                            EditorGUIUtility.PingObject(error.context);
                    }
                }
                
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();
        }
            
        private static void OnClickPublish()
        {
            validationReport = null;
            
            CustomWindow.IsInputDisable = true;  
            EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Publish...", 1.0f);

            validationReport = ProductValidator.ValidationCheck();

            if (validationReport.result == ValidationReport.Result.Success)
            {
                buildReport = ProductBuildPipeline.PublishWithBuildReport(AssetBundleConfig.Webgl);
                buildReport = ProductBuildPipeline.PublishWithBuildReport(AssetBundleConfig.WebglMobile);
            }
            
            EditorUtility.ClearProgressBar();
            CustomWindow.IsInputDisable = false;

            if(validationReport != null) return;
            
            var msg = buildReport.result == JBuildReport.Result.Success ? "빌드 성공" : "빌드 실패";
            EditorUtility.DisplayDialog("Witch Creator Toolkit", msg, "OK");
        }
    }
}