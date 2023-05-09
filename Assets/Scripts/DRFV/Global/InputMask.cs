using System.Collections;
using DG.Tweening;
using DRFV.inokana;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Global
{
    [RequireComponent(typeof(Image))]
    public class InputMask : MonoSingleton<InputMask>
    {
        private Image _image;
        private Coroutine _coroutine;
        public void Start()
        {
            _image = gameObject.GetComponent<Image>();
            _image.raycastTarget = false;
        }

        public void EnableInputMask(float alpha = 0.5f, float duration = 0.1f, Ease ease = Ease.OutSine)
        {
            if (_image.raycastTarget) return;
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(EnableInputMaskC(alpha, duration, ease));
        }

        private IEnumerator EnableInputMaskC(float alpha = 0.1f, float duration = 0.1f, Ease ease = Ease.OutSine)
        {
            _image.DOKill();
            _image.raycastTarget = true;
            _image.DOFade(alpha, duration).SetEase(ease);
            yield return new WaitForSeconds(duration);
        }
        
        public void DisableInputMask(float duration = 0.1f, Ease ease = Ease.OutSine)
        {
            if (!_image.raycastTarget) return;
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(DisableInputMaskC(duration, ease));
        }

        private IEnumerator DisableInputMaskC(float duration = 0.1f, Ease ease = Ease.OutSine)
        {
            _image.DOKill();
            _image.DOFade(0f, duration).SetEase(ease);
            yield return new WaitForSeconds(duration);
            _image.raycastTarget = false;
        }
    }
}