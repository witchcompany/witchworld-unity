using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.Tool;
using WitchCompany.Toolkit.Editor.Validation;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public static class CustomWindowValidation
    {
        private static ValidationReport validationReport;
        private static Vector2 scrollPos = Vector2.zero;

        public static void ShowValidation()
        {
            DrawSceneVital();
            GUILayout.Space(10);
            DrawAssetVital();
            GUILayout.Space(10);

            if (GUILayout.Button("Check"))
            {
                //OnClickCheck().Forget();
                OnClickCheck();
                // todo : window 열릴 때 초기화하도록 변경 -> showWitchToolkit()
                CustomWindow.InitialStyles();
            }
            
            if (validationReport != null) 
                DrawReport();
        }

        private static void DrawSceneVital()  
        {
            GUILayout.Label("Scene Vital", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Vertex", OptimizationValidator.GetAllMeshes().Item2.ToString());
            EditorGUILayout.LabelField("Unique Material", OptimizationValidator.GetUniqueMaterialCount().ToString());
            EditorGUILayout.LabelField("Texture", OptimizationValidator.GetTextureMB(OptimizationValidator.FindTextureList())+ " MB");
            EditorGUILayout.LabelField("Light Map", OptimizationValidator.GetLightMapMB()+ " MB");
            EditorGUILayout.LabelField("Audio Clip", OptimizationValidator.GetAudioClipMB(OptimizationValidator.FindAudioClipList())+ " MB");

            EditorGUILayout.EndVertical();
        }

        private static void DrawAssetVital()
        {
            GUILayout.Label("Asset Vital", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            var assetData = AssetDataValidator.GetAssetData();
            
            EditorGUILayout.LabelField("Art", assetData["art"].count.ToString());
            EditorGUILayout.LabelField("Video", assetData["video"].count.ToString());
            EditorGUILayout.LabelField("Doodling", assetData["doodling"].count.ToString());
            EditorGUILayout.LabelField("Posting", assetData["posting"].count.ToString());
            EditorGUILayout.LabelField("Ranking", assetData["ranking"].count.ToString());

            EditorGUILayout.EndVertical();
        }

        private static void ValidationCheck()
        {
            //var task = AwaitValidator.ValidationCheck();
            
            validationReport = OptimizationValidator.ValidationCheck();
            validationReport.Append(ScriptRuleValidator.ValidationCheck(CustomWindowPublish.GetOption()));
            validationReport.Append(ObjectValidator.ValidationCheck());
            validationReport.Append(WhiteListValidator.ValidationCheck());
            validationReport.Append(EssentialComponentValidator.ValidationCheck());
            //validationReport.Append(await task);

            // BlockDataValidator.GetBlockData();
        }


        private static void OnClickCheck()
        {
            // 업로드
            CustomWindow.IsInputDisable = true;
            EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Checking to validation...", 1.0f);

            ValidationCheck();
                    
            EditorUtility.ClearProgressBar();
            CustomWindow.IsInputDisable = false;
        }
        
        private static void DrawReport()
        {
            // Scene 변경사항 실시간 반영됨
            // validationReport = OptimizationValidator.ValidationCheck();
            
            GUILayout.Label("Report", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("Result", validationReport.result.ToString());
            EditorGUILayout.EndVertical();
            
            if (validationReport.result == ValidationReport.Result.Failed)
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
                            
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label(error.tag, EditorStyles.boldLabel);   
                            
                            
                            if (error.tag == ValidationTag.TagBatchingStatic && GUILayout.Button("Clear", GUILayout.Width(100)))
                            {
                                StaticRevertTool.ClearBatchingStatics();
                            }
                            
                            EditorGUILayout.EndHorizontal();
                            
                            preErrorTag = error.tag;
                        }

                        // todo : message의 길이에 따라 log Height 변경
                        // 로그 종류에 따라 버튼 style 변경
                        if (error.context == null)
                        {
                            GUILayout.Label(error.message, CustomWindow.LogTextStyle);
                        }
                        else
                        {
                            // 에러 Log 출력
                            if (GUILayout.Button(error.message, CustomWindow.LogButtonStyle))
                            {
                                EditorGUIUtility.PingObject(error.context);
                            }
                        }
                    }
                    
                    EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }
    }
}