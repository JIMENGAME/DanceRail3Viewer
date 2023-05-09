using DRFV.Game;

namespace DRFV.Game.SceneControl
{
    public class HPRefill : global::DRFV.Game.SceneControl.SceneControl
    {
        protected override void Event()
        {
            theGameManager.hpManager.RefillHp();
        }

        public void Init(TheGameManager theGameManager, float ms)
        {
            GeneralInit(theGameManager, ms);
        }
    }
}
