using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.JsonData;
using Newtonsoft.Json;
using UnityEngine;

public class UnlimitedRoadManager : MonoBehaviour
{
    private static Songlist Songlist = null;
    // Start is called before the first frame update
    void Start()
    {
        if (Songlist == null)
        {
            string directory = StaticResources.Instance.dataPath + "unlimited_road";
            if (!Directory.Exists(directory) || !File.Exists(directory + "/Songlist.json") || Directory.GetDirectories(directory).Length == 0)
            {
                FadeManager.Instance.Back();
                return;
            }
        
            Songlist = Util.ReadJson(directory + "/Songlist.json").ToObject<Songlist>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
