using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.SceneControl
{
    public class SongCover : SceneControl
    {
        private Sprite songCover;

        protected override void Event()
        {
            if (theGameManager.sprSongImage)
                theGameManager.sprSongImage.sprite = songCover;
        }
    
        public void Init(TheGameManager theGameManager, float ms, Sprite value)
        {
            songCover = value;
            GeneralInit(theGameManager, ms);
        }
    }
}
