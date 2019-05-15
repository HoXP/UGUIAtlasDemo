using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 实现调用 TexturePacker 的命令行来打图集，不用单独打开TP的GUI界面取打图集。
/// </summary>
public class TPPacker
{
    public string tpEXE { get; private set; }
    public string tpCMD { get; private set; }
    public string tpOUT { get; private set; }
    public string sheetName { get; private set; }
    public Dictionary<string, Vector4> borderMap { get; private set; }

    public TPPacker(string folderPath)
    {
        ReadConfig();
        _folderSelected = folderPath;
        int __index_ = folderPath.LastIndexOf('/') + 1;
        string __folderName = folderPath.Substring(__index_, folderPath.Length - __index_);
        sheetName = Path.Combine(Path.Combine(Application.dataPath, tpOUT), __folderName);
        _sheetUnityPath = "Assets/" + tpOUT + "/" + __folderName;
    }

    ~TPPacker()
    {
    }

    public bool ValidateTPExePath()
    {
        bool __exists = File.Exists(tpEXE);
        if (!__exists)
        {
            EditorUtility.DisplayDialog("提示", string.Format("目录{0}中不存在TexturePacker.exe。请更新项目！", tpEXE), "确定");
        }
        return __exists;
    }

    public StringBuilder ExtractPNG(string folder)
    {
        string[] __allFiles = Directory.GetFiles(folder.Replace("Assets", Application.dataPath));
        if (__allFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "你所选的文件夹下面没有图片，确定你没有选错吗？？", "确定");
            return null;
        }
        StringBuilder __stringBd = new StringBuilder("");
        for (int i = 0; i < __allFiles.Length; i++)
        {
            string __fileName = __allFiles[i];
            if (__fileName.Contains(" "))
            {
                EditorUtility.DisplayDialog("提示", string.Format("图片{0}的文件名含有空格，请把空格删除再试！", __fileName), "确定");
                return null;
            }
            string __extenstion = Path.GetExtension(__fileName);
            if (__extenstion.Equals(".png"))
            {
                __stringBd.Append(__fileName);
                __stringBd.Append("  ");
            }
        }
        return __stringBd;
    }

    public bool ExcuteTPCommand()
    {
        StringBuilder __stringBd = ExtractPNG(_folderSelected);
        if (__stringBd == null) return false;
        ProcessStartInfo start = new ProcessStartInfo(tpEXE);
        start.Arguments = string.Format(tpCMD, sheetName, sheetName, __stringBd.ToString());
        start.CreateNoWindow = false;
        start.ErrorDialog = true;
        start.UseShellExecute = false;

        if (start.UseShellExecute)
        {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        }
        else
        {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }

        Process p = Process.Start(start);
        if (!start.UseShellExecute)
        {
            UnityEngine.Debug.Log(p.StandardOutput.ReadToEnd());
            string __err = p.StandardError.ReadToEnd();
            if (__err.Contains("TexturePacker:: error:"))
            {
                UnityEngine.Debug.LogError(p.StandardError.ReadToEnd());
                EditorUtility.DisplayDialog("TP出错了", "TexturePacker 出错了，请勿清空控制台log信息，等待古光仁救援，先去叫他！！", "确定");
                return false;
            }
        }

        p.WaitForExit();
        p.Close();
        return true;
    }

    /// <summary>
    /// 在打图集之前，要先把原来的图集的九宫信息存储
    /// 以防丢失
    /// </summary>
    public int SaveBorders()
    {
        string __sheetFullPath = _sheetUnityPath + ".png";
        if (!File.Exists(__sheetFullPath)) return 0;
        TextureImporter asetImp = TextureImporter.GetAtPath(__sheetFullPath) as TextureImporter;
        if (asetImp == null)
        {
            EditorUtility.DisplayDialog("错误", "保存九宫失败，将终止打图集操作，请勿清空控制台log。联系 古光仁！！", "确定");
            return -1;
        }
        SpriteMetaData[] __metaData = asetImp.spritesheet;
        borderMap = new Dictionary<string, Vector4>();
        for (int i = 0; i < __metaData.Length; i++)
        {
            SpriteMetaData __data = __metaData[i];
            borderMap.Add(__data.name, __data.border);
        }
        return 1;
    }

    private void ReadConfig()
    {
        DirectoryInfo __dir = new DirectoryInfo(Application.dataPath);
        string _path = __dir.Parent.FullName;
        tpEXE = _path + @"/command/TexuturePacker/exe/TexturePacker.exe";
        tpCMD = @" --sheet {0}.png --data {1}.xml --format sparrow --trim-mode None --pack-mode Best  --algorithm MaxRects --max-size 2048 --opt RGBA8888 --size-constraints POT --disable-rotation --scale 1 {2}";
        tpOUT = @"Resources/appdata/res/atlas";
    }

    private string _folderSelected;
    private string _sheetUnityPath;
}