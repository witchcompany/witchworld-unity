using System.IO;
using Cysharp.Threading.Tasks;

namespace WitchCompany.Toolkit.Editor.Tool
{
    public static class FileTool
    {
        /// <summary> 파일 경로 존재할 때 파일 반환 </summary>
        public static async UniTask<byte[]> GetByte(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            
            var bytes = await File.ReadAllBytesAsync(filePath);
            return bytes;
        }
    }
}