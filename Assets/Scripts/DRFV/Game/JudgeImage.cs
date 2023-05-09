using TMPro;
using UnityEngine;

namespace DRFV.Game
{
    public class JudgeImage : MonoBehaviour
    {
        [SerializeField] AnimationCurve PosyCurve, AlphaCurve;

        [SerializeField] Sprite sPJ, sPF, sGD, sMS, sFA, sSL;

        float timer = 0.0f;

        public void Init(int main, bool isMs = false, int ms = 0, int fast = 0)
        {
            transform.Find("SpriteBigJudge").GetComponent<SpriteRenderer>().sprite = main switch
            {
                4 => sPJ,
                3 => sPF,
                2 => sGD,
                _ => sMS
            };

            if (isMs)
            {
                transform.Find("SpriteSmallJudge").gameObject.SetActive(false);
                TextMeshPro textMeshPro = transform.Find("TextSmallJudge").GetComponent<TextMeshPro>();
                textMeshPro.text = ms + "ms";
                textMeshPro.color =
                    ms >= 0 ? new Color(254 / 255f, 143 / 255f, 0f, 1f) : new Color(0f, 167 / 255f, 254 / 255f, 1f);
            }
            else
            {
                transform.Find("TextSmallJudge").gameObject.SetActive(false);
                transform.Find("SpriteSmallJudge").GetComponent<SpriteRenderer>().sprite = fast switch
                {
                    2 => sSL,
                    1 => sFA,
                    _ => null
                };
            }
        }

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime;
            //位置
            var pos = transform.position;
            pos.y = PosyCurve.Evaluate(timer);
            transform.position = pos;

            //半透明
            Color c = new Color(1, 1, 1, AlphaCurve.Evaluate(timer));
            transform.Find("SpriteBigJudge").GetComponent<SpriteRenderer>().color = c;
            transform.Find("SpriteSmallJudge").GetComponent<SpriteRenderer>().color = c;

            //タイムアップ
            if (timer > 0.3f)
            {
                Destroy(gameObject);
            }
        }
    }
}