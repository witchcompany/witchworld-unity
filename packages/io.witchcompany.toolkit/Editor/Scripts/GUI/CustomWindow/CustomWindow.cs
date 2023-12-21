using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.API;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using Color = UnityEngine.Color;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public class CustomWindow : EditorWindow
    {
        
        private const float MinWindowWidth = 700;
        private const float MinWindowHeight = 700;
        private const float MaxWindowWidth = 700;
        private const float MaxWindowHeight = 2000;
        private const float LogHeight = 40;
        private static GUIStyle logTextStyle;
        private static GUIStyle logButtonStyle;
        private static GUIStyle labelTextStyle;
        private static bool isInputDisable;
        private static bool isAdmin;

        public static GUIStyle LogTextStyle => logTextStyle;
        public static GUIStyle LogButtonStyle => logButtonStyle;
        // public static GUIStyle LabelTextStyle => labelTextStyle;
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
            logTextStyle = new GUIStyle
            {
                fixedWidth = MinWindowWidth - 20,
                fixedHeight = LogHeight,
                margin = new RectOffset(0, 0, 0, 3),
                padding = new RectOffset(5, 5, 5, 5),
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                active = { textColor = Color.white }
            };
            
            // 로그 버튼 스타일
            logButtonStyle = new GUIStyle
            {
                fixedWidth = MinWindowWidth-20,
                fixedHeight = LogHeight,
                margin = new RectOffset(0, 0, 0, 3),
                padding = new RectOffset(5, 5, 5, 5),
                alignment = TextAnchor.MiddleLeft,
                normal=
                {
                    background = CreateBackgroundColorImage(new Color(0.3f, 0.3f, 0.3f)),
                    // background = Texture2D.grayTexture,
                    textColor = Color.white,
                },
                active =
                {
                    background = Texture2D.grayTexture,
                    textColor = Color.white
                }
            };
            
            // labelTextStyle = EditorStyles.label;
            // labelTextStyle.fixedWidth = 120;
        }

        private static GUIContent[] defalutToolbarLabels = new GUIContent[4]
        {
            new ("Authentication"),
            new ("Validation"),
            new ("Publish"),
            new ("Settings"),
        };
        
        private static GUIContent[] adminToolbarLabels = new GUIContent[5]
        {
            new ("Authentication"),
            new ("Validation"),
            new ("PublishBlock"),
            // new ("Admin"),
            // new("Export Bundle"),
            new("PublishGearItem"),
            new ("Setting"),
        };

        private void OnGUI()
        {
            InitialStyles();
            
            // 윈도우 비활성화 그룹 지정
            EditorGUI.BeginDisabledGroup(isInputDisable);

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var label = AuthConfig.Admin ? adminToolbarLabels : defalutToolbarLabels;
                var controlPanelType = (ControlPanelType)GUILayout.Toolbar((int)ToolkitConfig.CurrControlPanelType, label);

                if (check.changed)
                {
                    if (!AuthConfig.Admin && (int)controlPanelType == 3)
                        controlPanelType += adminToolbarLabels.Length - defalutToolbarLabels.Length;
                    
                    ToolkitConfig.CurrControlPanelType = controlPanelType;
                }
            }
            
            // 선택한 메뉴에 따라 다른 함수 호출
            switch (ToolkitConfig.CurrControlPanelType)
            {
                case ControlPanelType.Auth: 
                    CustomWindowAuth.ShowAuth();
                    break;
                case ControlPanelType.Validation :
                    CustomWindowValidation.ShowValidation();
                    break;
                case ControlPanelType.PublishBlock: 
                    CustomWindowPublish.ShowPublish();
                    break;
                case ControlPanelType.Setting : 
                    CustomWindowSetting.ShowSetting();
                    break;
                // case ControlPanelType.Admin :
                //     CustomWindowAdmin.ShowAdmin();
                //     break;
                case ControlPanelType.PublishGearItem :
                    CustomWindowPublishGearItem.ShowPublishGearItem();
                    break;
                // case ControlPanelType.UploadBundle:
                //     CustomWindowUploadBundle.ShowUploadBundle();
                //     break;
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