#if UNITY_EDITOR
using System;
using System.IO;
using DRFV.Data;
using DRFV.Global;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DRFV.Editor
{
    [ScriptedImporter(1, new[] { ".drb", ".rawwav" })]
    public class DrbImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            switch (ctx.assetPath.Substring(ctx.assetPath.LastIndexOf(".", StringComparison.Ordinal)))
            {
                case ".drb":
                    var txt = File.ReadAllText(ctx.assetPath);
                    var assetsText = new TextAsset(txt);
                    ctx.AddObjectToAsset("main obj", assetsText);
                    ctx.SetMainObject(assetsText);
                    break;
                case ".rawwav":
                    FileStream fileStream = new FileStream(ctx.assetPath, FileMode.Open, FileAccess.Read);
                    byte[] data = new byte[fileStream.Length];
                    if (fileStream.Read(data) != data.Length) throw new ArgumentException();
                    var wav = ScriptableObject.CreateInstance<RawWav>();
                    wav.SetData(data);
                    ctx.AddObjectToAsset("main obj", wav);
                    ctx.SetMainObject(wav);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class DR3Postprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                if (str.EndsWith(".drb"))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(str);
                    AssetDatabase.SetLabels(obj, new[] { "DR3 Bitmap" });
                }

                if (str.EndsWith(".scenecontrol.json"))
                {
                    var obj = AssetDatabase.LoadAssetAtPath<Object>(str);
                    AssetDatabase.SetLabels(obj, new[] { "DR3FV Scenecontrol File" });
                }
            }
        }
    }
}
#endif