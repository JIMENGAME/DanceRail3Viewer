using System;
using DRFV.Data;
using DRFV.Global;
using DRFV.inokana;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostProcess : MonoBehaviour
{
    public Material Material;
    public bool enablePostProcess;
    private ProgressManager _progressManager;
    private TestifyAnomalyArguments[] _arguments;
    private int index;

    public void Init(ProgressManager progressManager, float minEffect, float maxEffect, float strength, int sampleCount,
        TestifyAnomalyArguments[] arguments)
    {
        _progressManager = progressManager;
        Material.SetFloat(Uniform.minEffect, minEffect);
        Material.SetFloat(Uniform.maxEffect, maxEffect);
        Material.SetFloat(Uniform.strength, strength);
        Material.SetInt(Uniform.sampleCount, sampleCount);
        _arguments = arguments;
        enablePostProcess = true;
    }

    public void Update()
    {
        if (!enablePostProcess) return;
        if (_progressManager.NowTime < _arguments[0].startTime) return;
        if (_progressManager.NowTime >= _arguments[^1].startTime + _arguments[^1].duration)
        {
            Material.SetFloat(Uniform.strength, _arguments[^1].startStrength + _arguments[^1].deltaStrength);
            return;
        }

        for (var index1 = 0; index1 < _arguments.Length - 1; index1++)
        {
            var testifyAnomalyArguments = _arguments[index1];
            var nextTestifyAnomalyArguments = _arguments[index1 + 1];
            if (testifyAnomalyArguments.startTime <= _progressManager.NowTime && _progressManager.NowTime <
                testifyAnomalyArguments.startTime + testifyAnomalyArguments.duration)
            {
                float i = (_progressManager.NowTime - testifyAnomalyArguments.startTime) /
                          testifyAnomalyArguments.duration;
                Material.SetFloat(Uniform.strength,
                    testifyAnomalyArguments.startStrength +
                    (testifyAnomalyArguments.strengthType == StrengthType.SineOut ? Util.SineOutEase(i) : i) *
                    testifyAnomalyArguments.deltaStrength);
                return;
            }

            if (testifyAnomalyArguments.startTime + testifyAnomalyArguments.duration <= _progressManager.NowTime &&
                _progressManager.NowTime < nextTestifyAnomalyArguments.startTime)
            {
                Material.SetFloat(Uniform.strength,
                    testifyAnomalyArguments.startStrength + testifyAnomalyArguments.deltaStrength);
                return;
            }
        }
        // while (_arguments[index].startTime < _progressManager.NowTime) index++;
        // if (_progressManager.NowTime < _arguments[0].startTime) return;
        // if (_progressManager.NowTime >= _arguments[^1].startTime + _arguments[^1].duration)
        // {
        //     Material.SetFloat(Uniform.strength, _arguments[^1].startStrength + _arguments[^1].deltaStrength);
        //     return;
        // }
        //
        // float i = (_progressManager.NowTime - _arguments[index].startTime) /
        //           _arguments[index].duration;
        // Material.SetFloat(Uniform.strength,
        //     _arguments[index].startStrength +
        //     (_arguments[index].strengthType == StrengthType.SineOut ? Util.SineOutEase(i) : i) *
        //     _arguments[index].deltaStrength);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (enablePostProcess && Material != null)
        {
            Graphics.Blit(src, dest, Material);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    static class Uniform
    {
        public static readonly int minEffect = Shader.PropertyToID("_MinEffectIntensity");
        public static readonly int maxEffect = Shader.PropertyToID("_MaxEffectIntensity");
        public static readonly int strength = Shader.PropertyToID("_OverallEffectStrength");
        public static readonly int sampleCount = Shader.PropertyToID("_SampleCount");
    }
}