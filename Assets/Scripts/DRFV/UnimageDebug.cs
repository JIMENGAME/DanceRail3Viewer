using System;
using System.Collections.Generic;
using System.IO;
using DRFV.Global;
using UnityEngine;
using UnityEngine.UI;

public class UnimageDebug : MonoBehaviour
{
    // ".jpg", ".png", ".jpeg", ".webp", ".bmp", ".tga", ".gif"
    public Image[] images;

    // Start is called before the first frame update
    void Start()
    {
        string instanceDataPath = StaticResources.Instance.dataPath + "settings/unimage";
        if (!Directory.Exists(instanceDataPath)) Directory.CreateDirectory(instanceDataPath);
        IEnumerable<string> enumerateFiles =
            Directory.EnumerateFiles(instanceDataPath, "base.*", SearchOption.TopDirectoryOnly);
        foreach (string enumerateFile in enumerateFiles)
        {
            string extension = Path.GetExtension(enumerateFile.Replace("\\", "/"));
            int i = extension switch
            {
                ".jpg" => 0,
                ".png" => 1,
                ".jpeg" => 2,
                ".webp" => 3,
                ".bmp" => 4,
                ".tga" => 5,
                ".gif" => 6,
                _ => -1
            };
            if (i < 0) continue;
            FileStream fileStream = new FileStream(enumerateFile.Replace("\\", "/"), FileMode.Open,
                FileAccess.Read, FileShare.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            byte[] data = new byte[fileStream.Length];
            if (fileStream.Read(data) != data.Length) throw new ArgumentException();
            fileStream.Close();
            images[i].sprite = Util.ByteArrayToSprite(data, false);
        }
    }
}