using UnityEngine;

namespace DRFV.Global
{
    public static class CheckDataContainers
    {
        public static void CleanSongDataContainer()
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("SongData");
            foreach (GameObject go in gos)
            {
                Object.Destroy(go);
            }
        }
    
        public static void CleanResultDataContainer()
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("ResultData");
            foreach (GameObject go in gos)
            {
                Object.Destroy(go);
            }
        }
    
        public static void CleanStoryDataContainer()
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag("StoryData");
            foreach (GameObject go in gos)
            {
                Object.Destroy(go);
            }
        }

        public static void CleanDankaiDataContainer()
        {
            
        }
    }
}