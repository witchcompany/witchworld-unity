using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.Validation;
using WitchCompany.Toolkit.Validation;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public static class CustomWindowValidation
    {
        private static ValidationReport validationReport;
        
        public static void ShowValidation()
        {
            GUILayout.Label("현재 유효성 검사는 빌드 진행시 자동으로 진행됩니다.");
            GUILayout.Label("추후 3가지 유효성 검사를 제공할 예정입니다.");
            GUILayout.Label("  - 최적화 검사");
            GUILayout.Label("  - 씬 규칙 검사");
            GUILayout.Label("  - 업로드 규칙 검사");
            
            
            GUILayout.Space(30);
            DrawSceneVital();
            GUILayout.Space(10);

            if (validationReport != null) 
                DrawReport();
        }

        private static void DrawSceneVital()
        {

            GUILayout.Label("Scene Vital", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            // validation tap에서 계속 확인
            EditorGUILayout.LabelField("Vertex", OptimizationValidator.GetAllMeshes().Item2.ToString());
            EditorGUILayout.LabelField("Unique Material", OptimizationValidator.GetUniqueMaterialCount().ToString());
            EditorGUILayout.LabelField("Texture", OptimizationValidator.GetTextureMB()+ " MB");
            EditorGUILayout.LabelField("Light Map", OptimizationValidator.GetLightMapMB()+ " MB");

            EditorGUILayout.EndVertical();
            if (GUILayout.Button("Check"))
            {
                validationReport = OptimizationValidator.ValidationCheck();
                validationReport.Append(ScriptRuleValidator.ValidationCheck(CustomWindowPublish.GetOption()));
                
                // todo : window 열릴 때 초기화하도록 변경 -> showWitchToolkit()
                CustomWindow.InitialStyles();
            }
        }
        
        private static Vector2 scrollPos = Vector2.zero;
        
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
                            GUILayout.Label(error.tag, EditorStyles.boldLabel);   
                            preErrorTag = error.tag;
                        }

                        // todo : message의 길이에 따라 log Height 변경
                        // 로그 종류에 따라 버튼 style 변경
                        if (error.context == null)
                        {
                            GUILayout.Label(error.message, CustomWindow.logTextStyle);
                        }
                        else
                        {
                            // 에러 Log 출력
                            if (GUILayout.Button(error.message, CustomWindow.logButtonStyle))
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