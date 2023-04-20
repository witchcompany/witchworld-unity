using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WitchCompany.Toolkit.Editor.API;
using WitchCompany.Toolkit.Editor.Configs;
using WitchCompany.Toolkit.Editor.DataStructure;
using WitchCompany.Toolkit.Editor.DataStructure.Admin;
using WitchCompany.Toolkit.Editor.Validation;

namespace WitchCompany.Toolkit.Editor.GUI
{
    public static class CustomWindowAdmin
    {
        private const string successMsg = "Upload Result : Success\n블록을 서버에 업로드했습니다";
        private const string failedMsg = "Upload Result : Failed\n블록을 서버에 업로드하지 못했습니다\n다시 시도해주세요";
        
        private static string _thumbnailPath;
        private static string _pathName;
        private static string _pathNameErrorMsg;
        private static JLanguageString _blockName = new ();
        private static BlockType _blockType;

        private static Texture2D thumbnailImage;
        private static List<string> popupUnityKeys = new ();
        private static List<JUnityKey> unityKeys;
        
        public static void ShowAdmin()
        {
            
            DrawUnityKey();

            GUILayout.Space(10);
            
            DrawBlockConfig();
            
        }
        private static bool isProcessing = false;
        private static async UniTaskVoid DrawUnityKey()
        {
            GUILayout.Label("Unity Key", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var unityKeyIndex = EditorGUILayout.Popup("key list", AdminConfig.UnityKeyIndex, popupUnityKeys.ToArray());

                        if (check.changed)
                        {
                            AdminConfig.UnityKeyIndex = unityKeyIndex;
                        }
                    
                }
                

                using (new EditorGUI.DisabledScope(isProcessing))
                {
                    // unity key list 조회
                    if (GUILayout.Button("Refresh", GUILayout.Width(100)))
                    {
                        // 비동기 처리하는 동안 버튼 비활성화 
                        isProcessing = true;
                        UnityEngine.GUI.enabled = false;
                        unityKeys = await WitchAPI.GetUnityKeys(0, 0);

                        if (unityKeys != null)
                        {
                            // 키 리스트 초기화 및 서버 데이터 반영
                            popupUnityKeys.Clear();
                            
                            
                            foreach (var key in unityKeys)
                            {
                                popupUnityKeys.Add($"{key.pathName} (made by {key.creatorNickName})");
                            }
                            
                            AdminConfig.UnityKeyIndex = 0;
                            
                            UnityEngine.GUI.enabled = true;
                            isProcessing = false;
                            
                        }
                    }
                }
            }
        } 
        
        private static async UniTaskVoid DrawBlockConfig()
        {
            GUILayout.Label("Block Config", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    // todo : 썸네일 미리보기
                    EditorGUILayout.LabelField("thumbnail", AdminConfig.ThumbnailPath, EditorStyles.textField);
                    if (GUILayout.Button("Select", GUILayout.Width(100)))
                    {
                        AdminConfig.ThumbnailPath = EditorUtility.OpenFilePanel("Witch Creator Toolkit", "", "jpg");
                    
                    }
                } 

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    // 정규식에 맞지 않을 경우 이전 값으로 되돌림
                    var prePathName = _pathName;
                    _pathName = EditorGUILayout.TextField("path name", AdminConfig.PathName);

                    if (check.changed)
                        AdminConfig.PathName = _pathName;
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    // 최대 글자수 넘으면 이전값으로 되돌림
                    _blockName.kr = EditorGUILayout.TextField("block name (한글)", AdminConfig.BlockNameKr);

                    if (check.changed)
                        AdminConfig.BlockNameKr = _blockName.kr;
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    _blockName.en = EditorGUILayout.TextField("block name (영문)", AdminConfig.BlockNameEn);
                    
                    if (check.changed)
                        AdminConfig.BlockNameEn = _blockName.en; 
                }

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var blockType = (BlockType)EditorGUILayout.EnumPopup("type", AdminConfig.Type);

                    if (check.changed)
                        AdminConfig.Type = blockType;
                } 
            }

            if (GUILayout.Button("Publish"))
            {
                CustomWindow.IsInputDisable = true;

                var report = AdminPublishValidatior.ValidationCheck();
                if (report.errors.Count > 0)
                {
                    var message = "";
                    foreach (var error in report.errors)
                    {
                        message += error.message + "\n";
                        Debug.Log(error.message);
                    }
                    EditorUtility.DisplayDialog("Publish Failed", message, "OK");
                }
                else
                {
                    var selectUnityKey = unityKeys[AdminConfig.UnityKeyIndex];
                    var blockData = new JBlockData()
                    {
                        unityKeyId = selectUnityKey.unityKeyId,
                        pathName = AdminConfig.PathName,
                        ownerNickname = AuthConfig.NickName,
                        blockName = new JLanguageString(AdminConfig.BlockNameEn, AdminConfig.BlockNameKr),
                        blockType = AdminConfig.Type.ToString().ToLower()
                    };
                    
                    EditorUtility.DisplayProgressBar("Witch Creator Toolkit", "Uploading from server....", 1.0f);
                    var uploadResult = await WitchAPI.UploadBlock(blockData);
                    EditorUtility.ClearProgressBar();

                    var uploadMsg = uploadResult ? successMsg : failedMsg;
                    EditorUtility.DisplayDialog("Witch Creator Toolkit", uploadMsg, "OK");
                }
                
                CustomWindow.IsInputDisable = false;
            }
        }
    }
}