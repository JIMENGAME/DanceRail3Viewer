using System.Collections;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.SceneControl
{
    public class  SceneControl : MonoBehaviour
    {
        protected TheGameManager theGameManager;
        public float ms;
        protected bool inited = false;

        protected virtual void Event()
        {
        }

        protected void GeneralInit(TheGameManager theGameManager, float ms)
        {
            this.theGameManager = theGameManager;
            this.ms = ms;
            inited = true;
            if (IsNear()) StartListen();
            else gameObject.SetActive(false);
        }

        public void StartListen()
        {
            if (inited) StartCoroutine(Listen());
        }

        public bool IsNear()
        {
            return theGameManager.progressManager.NowTime >= ms - 1000;
        }

        protected virtual IEnumerator Listen()
        {
            yield return new WaitWhile(() => theGameManager.progressManager.NowTime < ms);
            Event();
            Destroy(gameObject);
        }
    }
}