using DRFV.Dankai;
using UnityEngine;
using UnityEngine.UI;

public class DankaiButton : MonoBehaviour
{
    [SerializeField] private Text _text;
    [SerializeField] private Button _button;
    public void Init(int dankai, int id, DankaiManager dankaiManager)
    {
        _text.text = $"{dankai}-{id + 1}";
        _button.onClick.AddListener(() =>
        {
            dankaiManager.RefreshSelectedDankaiNow(dankai, id);
        });
    }
}
