using System;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.Tool;

// ReSharper disable InconsistentNaming

namespace WitchCompany.Toolkit.Editor.Configs
{
    /// <summary>
    /// 현재 툴킷의 설정을 저장한다.
    /// </summary>
    public static class ToolkitConfig
    {
        private const string Prefs_CurrentControlPanelType = "toolkit_current_control_panel_type";
        private const string Prefs_LogOption = "toolkit_log_option";
        private const string Prefs_ValidateLevel = "toolkit_validate_level";
        private const string Prefs_DeveloperMode = "toolkit_develper_mode";

        /// <summary>현재 컨트롤 패널 타입</summary>
        public static ControlPanelType CurrControlPanelType
        {
            get => (ControlPanelType)EditorPrefs.GetInt(Prefs_CurrentControlPanelType, (int)ControlPanelType.Disabled);
            set => EditorPrefs.SetInt(Prefs_CurrentControlPanelType, (int) value);
        }
        
        /// <summary>현재 로그 레벨</summary>
        public static LogLevel CurrLogLevel
        {
            get => (LogLevel)EditorPrefs.GetInt(Prefs_LogOption, (int)LogLevel.None);
            set => EditorPrefs.SetInt(Prefs_LogOption, (int)value);
        }

        /// <summary>현재 유효성 검사 수준</summary>
        public static ValidateLevel CurrValidateLevel
        {
            get => (ValidateLevel)EditorPrefs.GetInt(Prefs_ValidateLevel, (int)ValidateLevel.Essentials);
            set => EditorPrefs.SetInt(Prefs_ValidateLevel, (int) value);
        }

        /// <summary>현재 유니티 버전</summary>
        public static string UnityVersion => Application.unityVersion;

        public const string PackageName = "io.witchcompany.toolkit";
        
        // TODO: 툴킷 패키지 버전 가져오기 
        // public static int WitchToolkitVersion
        // {
        //     get
        //     {
        //         PackageTool.GePackageInfo();
        //         return 72;
        //     }
        // }
        public static string WitchToolkitVersion => "0.2.27";

        /// <summary> 문서 url </summary>
        public const string WitchToolkitDocumentUrl = "https://github.com/witchcompany/witchworld-unity/tree/main/packages/io.witchcompany.toolkit";
        
        
        /// <summary> 개발자 모드 </summary>
        public static bool DeveloperMode
        {
            get => EditorPrefs.GetBool(Prefs_DeveloperMode, false);
            set => EditorPrefs.SetBool(Prefs_DeveloperMode, value);
        }
    }
}