using DRFV.Global;
using DRFV.Game;
using DRFV.Global.Utilities;
using UnityEngine.UI;

namespace DRFV.Game.SceneControl
{
    public class SongHard : SceneControl
    {
        private int songHard = -1;

        protected override void Event()
        {
            theGameManager.textDif.text = "Tier " + (songHard == 0 ? "?" : songHard);
            theGameManager.textDif.color = Util.GetTierColor(songHard);
            theGameManager.textDif.gameObject.GetComponent<Outline>().enabled = songHard is > 20 or < 0;
        }
    
        public void Init(TheGameManager theGameManager, float ms, int value)
        {
            songHard = value;
            GeneralInit(theGameManager, ms);
        }
    }
}
