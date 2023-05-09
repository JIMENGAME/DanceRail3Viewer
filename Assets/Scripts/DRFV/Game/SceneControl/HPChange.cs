namespace DRFV.Game.SceneControl
{
    public class HPChange : SceneControl
    {
        private float hp = 0;
        private bool isSet;

        protected override void Event()
        {
            if (isSet)
            {
                theGameManager.hpManager.SetHp(hp);
            }
            else
            {
                if (hp < 0)
                {
                    theGameManager.hpManager.DecreaseHp(-hp);
                }
                else
                {
                    theGameManager.hpManager.InCreaseHp(hp);
                }
            }
        }

        public void Init(TheGameManager theGameManager, float ms, float value, bool isSet)
        {
            hp = value;
            this.isSet = isSet;
            GeneralInit(theGameManager, ms);
        }
    }
}