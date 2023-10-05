using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DRFV.Game.SceneControl;
using DRFV.Global;
using DRFV.Global.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace DRFV.Game
{
    public class SceneControlManager : MonoBehaviour
    {
        public TheGameManager theGameManager;

        public Dictionary<string, Sprite> images = new();

        public SceneControl.SceneControl[] SCs = null;

        public bool inited = false;

        public IEnumerator Init()
        {
            if (theGameManager.IsDankai)
            {
                DestroySelf();
            }
            else if (theGameManager.DebugMode || theGameManager.storyMode)
            {
                TextAsset textAsset;
                if (theGameManager.DebugMode)
                {
                    textAsset =
                        Resources.Load<TextAsset>(
                            $"DEBUG/{theGameManager.SongKeyword}.{theGameManager.SongHard}.scenecontrol");
                }
                else
                {
                    textAsset = ExternalResources.LoadText(
                        $"STORY/SONGS/{theGameManager.SongKeyword}.{theGameManager.SongHard}.scenecontrol");
                }

                if (textAsset)
                {
                    yield return ScenecontrolListReadin(JObject.Parse(textAsset.text));
                }
                else
                {
                    DestroySelf();
                }
            }
            else
            {
                string filePath = Path.GetFullPath(StaticResources.Instance.dataPath + "songs/" +
                                                   theGameManager.SongKeyword + "/scenecontrol." +
                                                   theGameManager.SongHard +
                                                   ".json");
                if (File.Exists(filePath))
                {
                    yield return ScenecontrolListReadin(Util.ReadJson(filePath));
                }
                else
                {
                    DestroySelf();
                }
            }
        }

        private void DestroySelf()
        {
            theGameManager.sceneControlManager = null;
            Destroy(gameObject);
        }

        private IEnumerator ScenecontrolListReadin(JObject content)
        {
            string[] keywords = new string[0];
            JArray scenecontrols = content["scenecontrols"].ToObject<JArray>();
            try
            {
                List<string> keywordsList = new List<string>();
                foreach (JToken jToken in scenecontrols)
                {
                    JObject scenecontrol = jToken.ToObject<JObject>();
                    string type = scenecontrol["type"].ToObject<string>();
                    if ("cover".Equals(type))
                    {
                        if (!keywordsList.Contains(scenecontrol["value"].ToObject<string>()))
                            keywordsList.Add(scenecontrol["value"].ToObject<string>());
                    }
                }

                keywords = keywordsList.ToArray();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                yield break;
            }

            yield return GetImage(keywords);

            try
            {
                List<SceneControl.SceneControl> SCs = new List<SceneControl.SceneControl>();
                List<float> SCSongNames = new List<float>();
                List<float> SCSongArtists = new List<float>();
                List<float> SCSongHards = new List<float>();
                List<float> SCSongCovers = new List<float>();
                List<float> SCHPChanges = new List<float>();
                List<float> SCHPMaxes = new List<float>();
                List<float> SCHPRefills = new List<float>();
                List<EnwidenLaneAtrributes> enwidenLane = new List<EnwidenLaneAtrributes>();
                bool aegleseekerExist = false,
                    axiumcrisisExist = false;

                foreach (JToken jToken in scenecontrols)
                {
                    JObject scenecontrol = jToken.ToObject<JObject>();
                    string type = scenecontrol["type"].ToObject<string>();
                    switch (type)
                    {
                        case "title":
                            GameObject go = new GameObject("SongName" + SCSongNames.Count);
                            go.transform.position = Vector3.zero;
                            go.transform.SetParent(gameObject.transform, false);
                            SongName sc = go.AddComponent<SongName>();
                            sc.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()),
                                scenecontrol["value"].ToObject<string>());
                            float time =
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>());
                            if (!SCSongNames.Contains(time))
                            {
                                SCSongNames.Add(time);
                                SCs.Add(sc);
                            }

                            break;
                        case "artist":
                            GameObject go1 = new GameObject("SongArtist" + SCSongArtists.Count);
                            go1.transform.position = Vector3.zero;
                            go1.transform.SetParent(gameObject.transform, false);
                            SongArtist sc1 = go1.AddComponent<SongArtist>();
                            sc1.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()),
                                scenecontrol["value"].ToObject<string>());
                            float time1 =
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>());
                            if (!SCSongArtists.Contains(time1))
                            {
                                SCSongArtists.Add(time1);
                                SCs.Add(sc1);
                            }

                            break;
                        case "tier":
                            GameObject go2 = new GameObject("SongHard" + SCSongHards.Count);
                            go2.transform.position = Vector3.zero;
                            go2.transform.SetParent(gameObject.transform, false);
                            SongHard sc2 = go2.AddComponent<SongHard>();
                            sc2.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()),
                                scenecontrol["value"].ToObject<int>());
                            float time2 =
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>());
                            if (!SCSongHards.Contains(time2))
                            {
                                SCSongHards.Add(time2);
                                SCs.Add(sc2);
                            }

                            break;
                        case "cover":
                            GameObject go3 = new GameObject("SongCover" + SCSongCovers.Count);
                            go3.transform.position = Vector3.zero;
                            go3.transform.SetParent(gameObject.transform, false);
                            SongCover sc3 = go3.AddComponent<SongCover>();
                            sc3.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()),
                                images[scenecontrol["value"].ToObject<string>()]);
                            float time3 =
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>());
                            if (!SCSongCovers.Contains(time3))
                            {
                                SCSongCovers.Add(time3);
                                SCs.Add(sc3);
                            }

                            break;
                        case "hp":
                            GameObject go4 = new GameObject("HPChange" + SCHPChanges.Count);
                            go4.transform.position = Vector3.zero;
                            go4.transform.SetParent(gameObject.transform, false);
                            HPChange sc4 = go4.AddComponent<HPChange>();
                            sc4.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()),
                                scenecontrol["value"].ToObject<float>() *
                                (scenecontrol["setting"].ToObject<string>().Equals("down") ? -1f : 1f),
                                scenecontrol["setting"].ToObject<string>().Equals("set"));
                            float time4 =
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>());
                            if (!SCHPChanges.Contains(time4))
                            {
                                SCHPChanges.Add(time4);
                                SCs.Add(sc4);
                            }

                            break;
                        case "hpMax":
                            GameObject go5 = new GameObject("HPMax" + SCHPMaxes.Count);
                            go5.transform.position = Vector3.zero;
                            go5.transform.SetParent(gameObject.transform, false);
                            HPMax sc5 = go5.AddComponent<HPMax>();
                            sc5.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()),
                                scenecontrol["value"].ToObject<float>());
                            float time5 =
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>());
                            if (!SCHPMaxes.Contains(time5))
                            {
                                SCHPMaxes.Add(time5);
                                SCs.Add(sc5);
                            }

                            break;
                        case "hpRefill":
                            GameObject go6 = new GameObject("HPRefill" + SCHPRefills.Count);
                            go6.transform.position = Vector3.zero;
                            go6.transform.SetParent(gameObject.transform, false);
                            HPRefill sc6 = go6.AddComponent<HPRefill>();
                            sc6.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()));
                            float time6 =
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>());
                            if (!SCHPRefills.Contains(time6))
                            {
                                SCHPRefills.Add(time6);
                                SCs.Add(sc6);
                            }

                            break;
                        case "aegleseeker":
                            if (aegleseekerExist) break;
                            aegleseekerExist = true;
                            theGameManager.AegleseekerObject.SetActive(true);
                            GameObject go7 = new GameObject("AegleseekerSceneControl");
                            go7.transform.position = Vector3.zero;
                            go7.transform.SetParent(gameObject.transform, false);
                            Aegleseeker aegleseeker = go7.AddComponent<Aegleseeker>();
                            aegleseeker.Init(theGameManager, scenecontrol["time"].ToObject<float>(),
                                theGameManager.AegleseekerObject.transform);
                            SCs.Add(aegleseeker);
                            break;
                        case "axiumcrisis":
                            if (axiumcrisisExist) break;
                            axiumcrisisExist = true;
                            GameObject go8 = new GameObject("AxiumCrisisSceneControl");
                            go8.transform.position = Vector3.zero;
                            go8.transform.SetParent(gameObject.transform, false);
                            AxiumCrisis axiumCrisis = go8.AddComponent<AxiumCrisis>();
                            axiumCrisis.Init(theGameManager,
                                theGameManager.drbfile.CalculateDRBFileTime(scenecontrol["time"].ToObject<float>()));
                            SCs.Add(axiumCrisis);
                            break;
                        case "enwidenlanes":
                            enwidenLane.Add(new EnwidenLaneAtrributes
                            {
                                up = scenecontrol["time"].ToObject<int>(),
                                duration = scenecontrol["duration"].ToObject<int>(),
                                isOn = scenecontrol["isOn"].ToObject<int>() == 1
                            });
                            break;
                        case "testfy_postprocess":
                            theGameManager.enableTestifyAnomaly = true;
                            break;
                        default:
                            throw new ArgumentException("Invalid scenecontrol type: " + type);
                    }
                }

                if (enwidenLane.Count > 0)
                {
                    // enwidenLane.Sort((a, b) => a.up - b.up);
                    // for (int i = 0; i < enwidenLane.Count - 1; i++)
                    // {
                    //     if (Mathf.Abs(enwidenLane[i].up - enwidenLane[i + 1].up) < 1)
                    //     {
                    //         enwidenLane[i + 1] = new EnwidenLaneAtrributes{up = -1};
                    //     }
                    // }
                    GameObject go = new GameObject("EnwidenLaneManager");
                    go.transform.position = Vector3.zero;
                    EnwidenLane enwidenLaneSc = go.AddComponent<EnwidenLane>();
                    enwidenLaneSc.Init(theGameManager, enwidenLane.ToArray());
                }

                // if (CheckSameValue(SCSongNames) ||
                //     CheckSameValue(SCSongArtists) ||
                //     CheckSameValue(SCSongHards) ||
                //     CheckSameValue(SCSongCovers) ||
                //     CheckSameValue(SCHPChanges) ||
                //     CheckSameValue(SCHPMaxes) ||
                //     CheckSameValue(SCHPRefills))
                //     throw new ArgumentException("Check out at least two scenecontrol with the same time and the same type");

                this.SCs = SCs.ToArray();

                if (this.SCs.Length == 0) DestroySelf();

                //Gc();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Destroy(gameObject);
                yield break;
            }

            inited = true;
        }
        //
        // public void Gc()
        // {
        //     GC.Collect();
        //     GC.Collect();
        //     GC.Collect();
        //     GC.Collect();
        //     GC.Collect();
        //     GC.Collect();
        //     GC.Collect();
        //     GC.Collect();
        // }

        public void Update()
        {
            if (!inited) return;
            for (var i = 0; i < SCs.Length; i++)
            {
                if (SCs[i] == null) continue;

                if (!SCs[i].IsNear()) continue;
                SCs[i].gameObject.SetActive(true);
                SCs[i].StartListen();
                SCs[i] = null;
            }
        }

        // public bool CheckSameValue(List<float> list)
        // {
        //     if (list.Count == 0) return false;
        //     list.Sort((a, b) => a <= b ? -1 : 1);
        //     for (int i = 0; i < list.Count - 1; i++)
        //     {
        //         if (list[i] == list[i + 1]) return true;
        //     }
        //
        //     return false;
        // }

        private IEnumerator GetImage(string[] keywords)
        {
            if (keywords.Length == 0)
            {
                yield break;
            }

            int i = 0;
            while (i < keywords.Length)
            {
                yield return GetImageCoroutine(keywords[i]);
                i++;
            }
        }

        private IEnumerator GetImageCoroutine(string keyword)
        {
            if (theGameManager.sprSongImage)
            {
                string dataPath = StaticResources.Instance.dataPath;
                Sprite sprite = null;
                string temp = dataPath + "songs/" + theGameManager.SongKeyword + "/" + keyword;
                if (File.Exists(temp + ".png"))
                {
                    using var uwr = UnityWebRequestTexture.GetTexture("file://" + temp + ".png");
                    yield return uwr.SendWebRequest();
                    if (uwr.isDone)
                    {
                        Texture2D texture2d = DownloadHandlerTexture.GetContent(uwr);
                        sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height),
                            new Vector2(0.5f, 0.5f));
                    }
                }
                else if (File.Exists(temp + ".jpg"))
                {
                    using var uwr = UnityWebRequestTexture.GetTexture("file://" + temp + ".jpg");
                    yield return uwr.SendWebRequest();
                    if (uwr.isDone)
                    {
                        Texture2D texture2d = DownloadHandlerTexture.GetContent(uwr);
                        sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height),
                            new Vector2(0.5f, 0.5f));
                    }
                }

                images.Add(keyword, sprite);
            }
        }
    }
}