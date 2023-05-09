using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SkyboxManager : MonoBehaviour
{
    private Transform _transform;
    private Vector3 startPos;
    // Start is called before the first frame update
    void Awake()
    {
        _transform = gameObject.transform;
        startPos = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        _transform.rotation = Quaternion.Euler(startPos + new Vector3(
            0.0f, 10.0f * Mathf.Sin(Time.realtimeSinceStartup / 55.0f * Mathf.PI),
            0.0f));
    }
}
