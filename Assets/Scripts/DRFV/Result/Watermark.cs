using DRFV.Global.Managers;
#if !UNITY_EDITOR
using DRFV.Select;
#endif
using TMPro;
using UnityEngine;

namespace DRFV.Result
{
    [RequireComponent(typeof(TextMeshProUGUI), typeof(RectTransform))]
    public class Watermark : MonoBehaviour
    {
        [SerializeField] private string normal, aprilFool;
        [SerializeField] private Vector2 normalPos, aprilFoolPos;

        void Start()
        {
            bool isAprilFool = RuntimeSettingsManager.Instance.isAprilFool
#if !UNITY_EDITOR
                               || GameObject.FindWithTag("SongData")
                                   .GetComponent<SongDataContainer>().songData.songArtist.Contains("ぺぽよ")
#endif
                ;
            gameObject.GetComponent<TextMeshProUGUI>().text =
                isAprilFool
                    ? aprilFool
                    : normal;
            gameObject.GetComponent<RectTransform>().anchoredPosition = isAprilFool ? aprilFoolPos : normalPos;
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) Start();
        }
#endif
    }
}