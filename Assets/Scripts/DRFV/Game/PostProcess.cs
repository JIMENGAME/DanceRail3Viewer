using DRFV.Data;
using DRFV.Global;
using DRFV.inokana;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class PostProcess : MonoBehaviour
{
    [SerializeField] private Material Material;
    public bool enablePostProcess;
    private ProgressManager _progressManager;
    private TestifyAnomalyArguments[] _arguments;
    private int index;

    public void Init(ProgressManager progressManager, TestifyAnomaly testifyAnomaly)
    {
        _progressManager = progressManager;
        Material.SetFloat(Uniform.minEffect, testifyAnomaly.minEffect);
        Material.SetFloat(Uniform.maxEffect, testifyAnomaly.maxEffect);
        Material.SetFloat(Uniform.strength, testifyAnomaly.strength);
        Material.SetInt(Uniform.sampleCount, testifyAnomaly.sampleCount);
        _arguments = testifyAnomaly.args.ToArray();
        enablePostProcess = true;
    }

    private void UpdateMaterial()
    {
        if (_progressManager.NowTime < _arguments[0].startTime) return;
        for (var i = 0; i < _arguments.Length; i++)
        {
            var testifyAnomalyArguments = _arguments[i];
            if (testifyAnomalyArguments.startTime <= _progressManager.NowTime && _progressManager.NowTime <
                testifyAnomalyArguments.endTime)
            {
                float k = (_progressManager.NowTime - testifyAnomalyArguments.startTime) /
                          testifyAnomalyArguments.duration;
                Material.SetFloat(Uniform.strength,
                    testifyAnomalyArguments.startStrength +
                    (testifyAnomalyArguments.strengthType == StrengthType.SineOut ? Util.SineOutEase(k) : k) *
                    testifyAnomalyArguments.deltaStrength);
                return;
            }

            if (i == _arguments.Length - 1)
            {
                Material.SetFloat(Uniform.strength, _arguments[^1].endStrength);
                return;
            }

            var nextTestifyAnomalyArguments = _arguments[i + 1];
            if (testifyAnomalyArguments.endTime <= _progressManager.NowTime &&
                _progressManager.NowTime < nextTestifyAnomalyArguments.startTime)
            {
                Material.SetFloat(Uniform.strength,
                    testifyAnomalyArguments.endStrength);
                return;
            }
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (enablePostProcess && Material != null)
        {
            UpdateMaterial();
            Graphics.Blit(src, dest, Material);
            return;
        }
        Graphics.Blit(src, dest);
    }

    static class Uniform
    {
        public static readonly int minEffect = Shader.PropertyToID("_MinEffectIntensity");
        public static readonly int maxEffect = Shader.PropertyToID("_MaxEffectIntensity");
        public static readonly int strength = Shader.PropertyToID("_OverallEffectStrength");
        public static readonly int sampleCount = Shader.PropertyToID("_SampleCount");
    }
}