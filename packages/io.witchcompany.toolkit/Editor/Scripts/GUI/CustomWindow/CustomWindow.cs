using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using Color = UnityEngine.Color;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public class CustomWindow : EditorWindow
    {
        
        private const float MinWindowWidth = 600;
        private const float MinWindowHeight = 700;
        private const float MaxWindowWidth = 600;
        private const float MaxWindowHeight = 2000;
        private const float LogHeight = 40;
        private static GUIStyle logTextStyle;
        private static GUIStyle logButtonStyle;
        private static GUIStyle clearButtionStyle;
        private static bool isInputDisable;

        public static GUIStyle LogTextStyle => logTextStyle;
        public static GUIStyle LogButtonStyle => logButtonStyle;
        public static GUIStyle ClearButtionStyle => clearButtionStyle;
        public static bool IsInputDisable { set => isInputDisable = value; }
        
        
        [MenuItem ("WitchToolkit/Witch Creator Toolkit")]
        private static void WitchToolKit () {
            EditorWindow window = GetWindow(typeof(CustomWindow));
            window.titleContent = new GUIContent("Witch Creator Toolkit");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.maxSize = new Vector2(MaxWindowWidth, MaxWindowHeight);
            InitialStyles();
            window.Show();
        }
        
        
        public static void InitialStyles()
        {
            // 로그 텍스트 스타일
            logTextStyle = new GUIStyle();
            logTextStyle.normal.textColor = Color.white;
            logTextStyle.active.textColor = Color.white;
            logTextStyle.fixedWidth = MinWindowWidth-20;
            logTextStyle.fixedHeight = LogHeight;
            logTextStyle.margin = new RectOffset(0, 0, 0, 3);
            logTextStyle.padding = new RectOffset(5, 5, 5, 5);
            logTextStyle.alignment = TextAnchor.MiddleLeft;
            
            // 로그 버튼 스타일
            logButtonStyle = new GUIStyle();
            logButtonStyle.normal.background = CreateBackgroundColorImage(new Color(0.3f, 0.3f, 0.3f));
            logButtonStyle.active.background = CreateBackgroundColorImage(new Color(0.4f, 0.4f, 0.4f));
            logButtonStyle.normal.textColor = Color.white;
            logButtonStyle.active.textColor = Color.white;
            logButtonStyle.fixedWidth = MinWindowWidth-20;
            logButtonStyle.fixedHeight = LogHeight;
            logButtonStyle.margin = new RectOffset(0, 0, 0, 3);
            logButtonStyle.padding = new RectOffset(5, 5, 5, 5);
            logButtonStyle.alignment = TextAnchor.MiddleLeft;
            
            // clear static button
            clearButtionStyle = new GUIStyle();
            clearButtionStyle.normal.background = CreateBackgroundColorImage(new Color(0.3f, 0.3f, 0.3f));
            clearButtionStyle.active.background = CreateBackgroundColorImage(new Color(0.4f, 0.4f, 0.4f));
            clearButtionStyle.normal.textColor = Color.white;
            clearButtionStyle.active.textColor = Color.white;
            clearButtionStyle.fixedWidth = 70f;
            clearButtionStyle.margin = new RectOffset(0, 6, 0, 10);
            
            clearButtionStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        
        private static readonly GUIContent[] toolbarLabels = new GUIContent[5]
        {
            new ("Authentication"),
            new ("Validation"),
            new ("Publish"),
            new ("Admin"),
            new ("Settings")
        };
        
        private void OnGUI()
        {
            // 윈도우 비활성화 그룹 지정
            EditorGUI.BeginDisabledGroup (isInputDisable);
            
            ToolkitConfig.CurrControlPanelType = (ControlPanelType)GUILayout.Toolbar((int)ToolkitConfig.CurrControlPanelType, toolbarLabels);
            
            // 선택한 메뉴에 따라 다른 함수 호출
            switch (ToolkitConfig.CurrControlPanelType)
            {
                case ControlPanelType.Auth: 
                    CustomWindowAuth.ShowAuth();
                    break;
                case ControlPanelType.Validate :
                    CustomWindowValidation.ShowValidation();
                    break;
                case ControlPanelType.Publish: 
                    CustomWindowPublish.ShowPublish();
                    break;
                case ControlPanelType.Admin :
                    CustomWindowAdmin.ShowAdmin();
                    break;
                case ControlPanelType.Config : 
                    CustomWindowSetting.ShowSetting();
                    break;
                default: break;
            }
            EditorGUI.EndDisabledGroup();
        }
        
        /// <summary> Editor Window가 닫힐 때 호출 </summary>
        private void OnDestroy()
        {
            // input field 값 초기화
            CustomWindowAuth.email = "";
            CustomWindowAuth.password = "";
        }

        
        /// <summary>  텍스처 픽셀 단위로 색 지정 </summary>
        private static Texture2D CreateBackgroundColorImage(Color color)
        {
            int w = 4, h = 4;
            Texture2D back = new Texture2D(w, h);
            Color[] buffer = new UnityEngine.Color[w * h];
            for (int i = 0; i < w; ++i)
            for (int j = 0; j < h; ++j)
                buffer[i + w * j] = color;
            back.SetPixels(buffer);
            back.Apply(false);
            return back;
        }
    }
}