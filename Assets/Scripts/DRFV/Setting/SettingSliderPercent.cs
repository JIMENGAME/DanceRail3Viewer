namespace DRFV.Setting
{
    public class SettingSliderPercent : SettingSlider
    {
        protected override string ParseValue(float value)
        {
            return (int) value * 10 + "%";
        }
    }
}