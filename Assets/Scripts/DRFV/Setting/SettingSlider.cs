using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Setting
{
    public class SettingSlider : MonoBehaviour
    {
        public Text Value;

        public Slider Slider;

        public void ChangeValue(int value)
        {
            int final = (int) Slider.value + value;
            if (Slider.value <= final && final <= Slider.maxValue || Slider.value > final && final >= Slider.minValue)
            {
                Slider.value = final;
                UpdateValue();
            }
        }

        public int GetValue()
        {
            return (int) Slider.value;
        }

        public void UpdateValue()
        {
            Value.text = (int) Slider.value * 10 + "%";
        }

        public void SetValue(int value)
        {
            if ((!(value >= Slider.minValue)) || (!(value <= Slider.maxValue))) return;
            Slider.value = value;
            UpdateValue();
        }
    }
}