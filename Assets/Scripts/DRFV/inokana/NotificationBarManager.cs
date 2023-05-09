using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.inokana
{
    public class NotificationBarManager : MonoSingleton<NotificationBarManager>
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private Text textComponent;


        public void Show(string text)
        {
            textComponent.text = text;

            rect.DOKill();
            StopAllCoroutines();

            StartCoroutine(ShowBar());
        }

        private IEnumerator ShowBar()
        {
            rect.localPosition = new Vector3(0f, 50f, 0f);

            rect.DOLocalMove(new Vector3(0f, -50f, 0f), 0.5f).SetEase(Ease.OutSine);

            yield return new WaitForSeconds(1.5f);

            rect.DOLocalMove(new Vector3(0f, 50f, 0f), 0.5f).SetEase(Ease.InSine);
        }


    }
}
