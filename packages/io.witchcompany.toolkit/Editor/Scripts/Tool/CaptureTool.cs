﻿using System;
using System.IO;
using UnityEngine;
using WitchCompany.Toolkit.Module;
using Object = UnityEngine.Object;

namespace WitchCompany.Toolkit.Editor.Tool
{
    public static class CaptureTool
    {
        public static void CaptureAndSave(string savePath)
        {
            var offset = new Vector3(0, 1.8f, 0);
            var cameraParent = Object.FindObjectOfType<WitchBlockManager>()?.EntrancePortal?.Exit;
            
            //cameraParent에 카메라 생성
            var cam = new GameObject("Capture Camera").AddComponent<Camera>();
            
            //camera Transform 설정
            if(cameraParent != null)
                cam.transform.SetParent(cameraParent);
            cam.transform.position = cameraParent.position + offset;
            cam.transform.forward = cameraParent.forward;

            //화면 캡쳐
            //카메라 view rt 저장
            var rt = new RenderTexture(1920, 1080, 32);
            cam.targetTexture = rt;
            cam.Render();
            
            //Texture2D 생성
            var newTexture = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
            RenderTexture.active = rt;

            //Texture2D로 읽기
            newTexture.ReadPixels(new Rect(0,0,rt.width,rt.height), 0,0);
            newTexture.Apply();

            //rt리셋, 카메라 파괴
            RenderTexture.active = null;
            Object.DestroyImmediate(cam.gameObject);
            
            //캡쳐 파일 저장
            var bytes = newTexture.EncodeToJPG(75);
            
            File.WriteAllBytes(savePath, bytes);
        }
    }
}