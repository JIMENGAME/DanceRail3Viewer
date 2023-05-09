using DRFV.Game;

namespace DRFV.Game.SceneControl
{
    public class SongName : SceneControl
    {
        private string songName = "";

        protected override void Event()
        {
            theGameManager.textSongTitle.text = songName;
            // if (theGameManager.textSongTitle.preferredWidth > 320.0f)
            // {
            //     theGameManager.textSongTitle.rectTransform.localScale = new Vector2(320.0f / theGameManager.textSongTitle.preferredWidth, 1.0f);
            // }
        }
    
        public void Init(TheGameManager theGameManager, float ms, string value)
        {
            songName = value;
            GeneralInit(theGameManager, ms);
        }
    }
}