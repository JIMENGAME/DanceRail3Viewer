using DRFV.Global.Managers;
using DRFV.Select;
using TMPro;
using UnityEngine;

namespace DRFV.Result
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Watermark : MonoBehaviour
    {
        [SerializeField] private string normal, aprilFool;

        void Start()
        {
            gameObject.GetComponent<TextMeshProUGUI>().text =
                RuntimeSettingsManager.Instance.isAprilFool || GameObject.FindWithTag("SongData")
                    .GetComponent<SongDataContainer>().songData.songArtist.Contains("ぺぽよ")
                    ? aprilFool
                    : normal;
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) Start();
        }
#endif
    }
}