using DRFV.Enums;
using DRFV.Game.HPBars;
using UnityEngine;
using UnityEngine.Serialization;

namespace DRFV.Game
{
    public class HPManager : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] Sprites; //length:20

        [SerializeField] Sprite[] spr;

        public TheGameManager manager;

        [FormerlySerializedAs("HPNOW")] public float HpNow = 100.0f;
        public float HPMAX = 100.0f;

        public bool isCheap = false;

        public HPBar HpBar = new HPBarDefault();

        public float[] barColor;

        private bool inited;

        public void Init(HPBar hpBar)
        {
            inited = false;
            HpBar.OnRefresh();
            HpBar = hpBar;
            HpNow = hpBar.HpInit;
            HPMAX = hpBar.HpMax;
            barColor = new float[3];
            Color color = (manager ? manager.gameSide : GameSide.DARK) switch
            {
                GameSide.LIGHT => hpBar.barColorLight,
                GameSide.DARK => hpBar.barColorDark,
                GameSide.COLORLESS => hpBar.barColorLight,
                _ => hpBar.barColorDark
            };
            barColor[0] = color.r;
            barColor[1] = color.g;
            barColor[2] = color.b;
            HpBar.OnInit();
            inited = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (!inited) return;
            int progress = (int)(HpNow / HPMAX * 100.0f);

            int progresslength = ((int)HpNow).ToString().Length;

            int lightnum = Mathf.Min(Mathf.Min(16, 16 - progresslength + 3), (int)(HpNow / HPMAX * 16.0f));

            for (int i = 0; i < lightnum; i++)
            {
                if (Sprites[i].sprite != spr[11])
                {
                    Sprites[i].sprite = spr[11];
                }
            }

            for (int i = 0; i < progresslength; i++)
            {
                Sprites[lightnum + i].sprite = spr[int.Parse(((int)HpNow).ToString().Substring(i, 1))];
            }

            Sprites[lightnum + progresslength].sprite = spr[10];

            if (lightnum + progresslength < 19)
            {
                for (int i = 19; i > lightnum + progresslength; i--)
                {
                    Sprites[i].sprite = null;
                }
            }

            for (int i = 0; i < 20; i++)
            {
                if (i == lightnum - 1)
                {
                    Sprites[i].color = new Color(barColor[0], barColor[1], barColor[2], Random.Range(0.1f, 0.8f));
                }
                else
                {
                    Sprites[i].color = new Color(barColor[0], barColor[1], barColor[2], Random.Range(0.9f, 1.0f));
                }
            }
        }

        public void DecreaseHp(float down)
        {
            if (down > HpNow) HpNow = 0;
            else HpNow -= down;
            UpdateHp();
        }

        public void InCreaseHp(float up)
        {
            if (!isCheap)
            {
                if (up > HPMAX - HpNow) HpNow = HPMAX;
                else HpNow += up;
                UpdateHp();
            }
        }

        public void SetHp(float value)
        {
            if (value < 0) value = 0;
            else if (value > HPMAX) value = HPMAX;
            else HpNow = value;
            if (value > 0) isCheap = false;
            UpdateHp();
        }

        public void RefillHp()
        {
            HpNow = HPMAX;
            isCheap = false;
            UpdateHp();
        }

        private void UpdateHp()
        {
            if (HpNow <= 0 && !isCheap && HpBar.isCheapable)
            {
                isCheap = true;
            }

            HpNow = isCheap ? 0.0f : Mathf.Clamp(HpNow, 0, HPMAX);
        }
    }
}