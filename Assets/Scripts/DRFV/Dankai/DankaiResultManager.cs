using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Login;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Dankai
{
    public class DankaiResultManager : MonoBehaviour
    {
        [SerializeField] private GameObject dankaiStagePrefab;
        [SerializeField] private RectTransform dankaiStageParent;
        [SerializeField] private Text tTitle, tSkill, tScore, tDetail, tSpeed;

        private void Start()
        {
            AccountInfo.Instance.UpdateAccountPanel();
            GameObject obj = GameObject.FindWithTag("DankaiData");
            if (!obj)
            {
                CheckDataContainers.CleanSongDataContainer();
                CheckDataContainers.CleanResultDataContainer();
                FadeManager.Instance.Back();
            }
            DankaiDataContainer dankaiDataContainer = obj.GetComponent<DankaiDataContainer>();
            int score = 0, pj = 0, p = 0, g = 0, m = 0;
            foreach (DankaiResultData dankaiResultData in dankaiDataContainer.results)
            {
                score += Mathf.RoundToInt(dankaiResultData.score);
                pj += dankaiResultData.pj;
                p += dankaiResultData.p;
                g += dankaiResultData.g;
                m += dankaiResultData.m;
            }
            tTitle.text = $"技能检定 {(m == 0 ? "超级" : "")}成功！";
            tSkill.text = dankaiDataContainer.skill;
            tScore.text = Util.ParseScore(Mathf.RoundToInt(score), SCORE_TYPE.ORIGINAL);
            tDetail.text = $"<color=#FF7>{pj}</color>/<color=#F97>{p}</color>/<color=#7F7>{g}</color>/<color=#F77>{m}</color>";
            tSpeed.text = $"SPEED: {Util.TransformSongSpeed(GlobalSettings.CurrentSettings.SongSpeed):N1}x";
            GridLayoutGroup gridLayoutGroup = dankaiStageParent.GetComponent<GridLayoutGroup>();
            gridLayoutGroup.spacing = new Vector2(0,(dankaiStageParent.sizeDelta.y - dankaiDataContainer.results.Count * gridLayoutGroup.cellSize.y) / dankaiDataContainer.results.Count);
            for (var i = 0; i < dankaiDataContainer.results.Count; i++)
            {
                GameObject obj1 = Instantiate(dankaiStagePrefab, dankaiStageParent);
                var stage = obj1.GetComponent<Stage>();
                stage.Init(i, dankaiDataContainer.songs[i], dankaiDataContainer.results[i]);
            }
        }
        public void Exit()
        {
            CheckDataContainers.CleanDankaiDataContainer();
            FadeManager.Instance.Back();
        }
    }
}