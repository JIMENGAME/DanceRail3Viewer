using UnityEngine;

namespace DRFV.Global
{
    public static class CheckDataContainers
    {
        private static void Clean(string tag)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject go in gos)
            {
                Object.Destroy(go);
            }
        }
        public static void CleanSongDataContainer()
        {
            Clean("SongData");
        }
    
        public static void CleanResultDataContainer()
        {
            Clean("ResultData");
        }
    
        public static void CleanStoryDataContainer()
        {
            Clean("StoryData");
        }

        public static void CleanDankaiDataContainer()
        {
            Clean("DankaiData");
        }
    }
}