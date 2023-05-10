#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace DRFV.inokana
{
    public static class SceneTools
    {
        [MenuItem("Scene Tools/Into Initialization")]
        private static void IntoInitialization()
        {
            IntoScene("initialization");
        }

        [MenuItem("Scene Tools/Into Main")]
        private static void IntoMain()
        {
            IntoScene("main");
        }

        [MenuItem("Scene Tools/Into Select")]
        private static void IntoChapterSelect()
        {
            IntoScene("select");
        }

        [MenuItem("Scene Tools/Into Game")]
        private static void IntoGame()
        {
            IntoScene("game");
        }

        [MenuItem("Scene Tools/Into Result")]
        private static void IntoResult()
        {
            IntoScene("result");
        }

        [MenuItem("Scene Tools/Into Shop")]
        private static void IntoShop()
        {
            IntoScene("shop");
        }

        [MenuItem("Scene Tools/Into Multiplayer")]
        private static void IntoMultiplayer()
        {
            IntoScene("multiplayer");
        }

        [MenuItem("Scene Tools/Into Settings")]
        private static void IntoSettings()
        {
            IntoScene("settings");
        }

        [MenuItem("Scene Tools/Into LyricTest")]
        private static void IntoLyricTest()
        {
            IntoScene("LyricTest");
        }

        [MenuItem("Scene Tools/Into Story")]
        private static void IntoStory()
        {
            IntoScene("story");
        }

        [MenuItem("Scene Tools/Into Download")]
        private static void IntoDownload()
        {
            IntoScene("download");
        }
        
        [MenuItem("Scene Tools/Into Test")]
        private static void IntoTest()
        {
            IntoScene("test");
        }
        
        [MenuItem("Scene Tools/Into Video")]
        private static void IntoVideo()
        {
            IntoScene("video");
        }

        [MenuItem("Scene Tools/Into HadouTest")]
        private static void IntoHadouTest()
        {
            IntoScene("hadoutest");
        }
        
        [MenuItem("Scene Tools/Into Offset")]
        private static void IntoOffset()
        {
            IntoScene("offset");
        }

        private static void IntoScene(string name)
        {
            if (EditorApplication.isPlaying)
            {
                SceneManager.LoadSceneAsync($"Scenes/{name}");
                return;
            }

            EditorSceneManager.OpenScene($"Assets/Scenes/{name}.unity");
        }
    }
}

#endif