using System.Collections;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.SceneControl
{
    public class AxiumCrisis : global::DRFV.Game.SceneControl.SceneControl
    {
        public void Init(TheGameManager theGameManager, float ms)
        {
            GeneralInit(theGameManager, ms);
        }

        protected override IEnumerator Listen()
        {
            yield return new WaitWhile(() => theGameManager.progressManager.NowTime < ms);
            Event();
        }

        protected override void Event()
        {
            if (!theGameManager.storyMode || theGameManager.miss > 0) return;
            StartCoroutine(Change());
        }

        private IEnumerator Change()
        {
            yield return theGameManager.AxiumCrisis();
            Destroy(gameObject);
        }
    }
}