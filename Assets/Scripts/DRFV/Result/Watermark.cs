using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class Watermark : MonoBehaviour
{
    [SerializeField] private string normal, aprilFool;
    void Start()
    {
        gameObject.GetComponent<TextMeshProUGUI>().text = RuntimeSettingsManager.Instance.isAprilFool ? aprilFool : normal;
    }
}
