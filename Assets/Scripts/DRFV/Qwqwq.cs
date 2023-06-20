using System;
using System.Collections.Generic;
using System.IO;
using DRFV.Global;
using UnityEngine;

public class Qwqwq : MonoBehaviour
{
    public void Test()
    {
        Debug.Log("直接获取：\n\n" + string.Join("\n", GetVisibleFileSystemEntries(StaticResources.Instance.dataPath, false)));
        Debug.Log("手动剔除Hidden：\n\n" + string.Join("\n", GetVisibleFileSystemEntries(StaticResources.Instance.dataPath, true)));
    }
    private string[] GetVisibleFileSystemEntries(string path, bool enableCheck)
    {
        if (!Directory.Exists(path)) return Array.Empty<string>();
        var fileSystemEntries = Directory.GetFileSystemEntries(path);
        List<string> tmp = new List<string>();
        foreach (string fileSystemEntry in fileSystemEntries)
        {
            if (File.Exists(fileSystemEntry))
            {
                if (enableCheck && (new FileInfo(fileSystemEntry).Attributes & FileAttributes.Hidden) ==
                    FileAttributes.Hidden) continue;
                tmp.Add(Path.GetRelativePath(path, fileSystemEntry));
            }
            else
            {
                if (enableCheck && (new DirectoryInfo(fileSystemEntry).Attributes & FileAttributes.Hidden) ==
                    FileAttributes.Hidden) continue;
                tmp.Add(Path.GetRelativePath(path, fileSystemEntry));
            }
        }

        return tmp.ToArray();
    }
}