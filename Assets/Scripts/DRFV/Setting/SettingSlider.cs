using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Setting
{
    public class SettingSlider : MonoBehaviour
    {
        [SerializeField] private Text tValue;

        [SerializeField] private Slider Slider;
        
        public int Value => (int)Slider.value;

        private void Awake()
        {
            Slider.onValueChanged.AddListener(SetValue);
        }

        public void AddOrMinusValue(int delta)
        {
            SetValue(Slider.value + delta);
        }

        private void UpdateValue()
        {
            tValue.text = ParseValue(Slider.value);
        }

        protected virtual string ParseValue(float value)
        {
            return (int) value + "";
        }

        public void SetValue(float value)
        {
            Slider.value = Mathf.Clamp(value, Slider.minValue, Slider.maxValue);
            UpdateValue();
        }
    }
}