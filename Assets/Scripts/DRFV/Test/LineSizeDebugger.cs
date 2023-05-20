using System.Collections;
using System.Collections.Generic;
using DRFV.Result;
using UnityEngine;

public class LineSizeDebugger : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private MSDetailsDrawer _drawer;
    public void Init(MSDetailsDrawer drawer)
    {
        _drawer = drawer;
    }
    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_drawer) return;
        lineRenderer.startWidth = lineRenderer.endWidth = _drawer.width;
    }
}
