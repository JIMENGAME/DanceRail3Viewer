using DRFV.Game;

namespace DRFV.Game.SceneControl
{
    public class SongArtist : SceneControl
    {
        private string songArtist = "";

        protected override void Event()
        {
            theGameManager.textSongArtist.text = songArtist;
            // if (theGameManager.textSongArtist.preferredWidth > 320.0f)
            // {
            //     theGameManager.textSongArtist.rectTransform.localScale = new Vector2(320.0f / theGameManager.textSongArtist.preferredWidth, 1.0f);
            // }
        }
    
        public void Init(TheGameManager theGameManager, float ms, string value)
        {
            songArtist = value;
            GeneralInit(theGameManager, ms);
        }
    }
}
