using DRFV.Game;

namespace DRFV.Game.SceneControl
{
    public class HPMax : global::DRFV.Game.SceneControl.SceneControl
    {
        private float hpMax;

        protected override void Event()
        {
            theGameManager.hpManager.HPMAX = hpMax;
            if (theGameManager.hpManager.HpNow > hpMax) theGameManager.hpManager.HpNow = hpMax;
        }
    
        public void Init(TheGameManager theGameManager, float ms, float value)
        {
            hpMax = value;
            if (hpMax <= 0) hpMax = 100;
            GeneralInit(theGameManager, ms);
        }
    }
}
