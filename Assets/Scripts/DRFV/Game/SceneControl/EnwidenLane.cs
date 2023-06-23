using DRFV.Global;
using UnityEngine;

namespace DRFV.Game.SceneControl
{
    public class EnwidenLane : MonoBehaviour
    {
        private TheGameManager _theGameManager;
        private SpriteRenderer leftLane, rightLane;
        private Transform leftSpectrum, rightspectrum, HanteiLine1, HanteiLine2;
        public AnimationCurve lanesCurve;

        public void Init(TheGameManager theGameManager, EnwidenLaneAtrributes[] qwq)
        {
            _theGameManager = theGameManager;
            leftLane = GameObject.FindWithTag("EnwidenLaneLeft").GetComponent<SpriteRenderer>();
            rightLane = GameObject.FindWithTag("EnwidenLaneRight").GetComponent<SpriteRenderer>();
            GameObject audioSpectrum = GameObject.FindWithTag("AudioSpectrum");
            leftSpectrum = audioSpectrum.transform.Find("Left").GetComponent<Transform>();
            rightspectrum = audioSpectrum.transform.Find("Right").GetComponent<Transform>();
            HanteiLine1 = GameObject.FindWithTag("HanteiLine1").GetComponent<Transform>();
            HanteiLine2 = GameObject.FindWithTag("HanteiLine2").GetComponent<Transform>();
            lanesCurve = GenerateLanesCurve(qwq);
        }

        private AnimationCurve GenerateLanesCurve(EnwidenLaneAtrributes[] list)
        {
            Keyframe[] keyframes = new Keyframe[list.Length * 2 + 2];
            keyframes[0] = new Keyframe(0f, 0f);
            for (var i = 0; i < list.Length; i++)
            {
                var enwidenLaneAtrribute = list[i];
                keyframes[2 * i + 1] = new Keyframe(enwidenLaneAtrribute.up, enwidenLaneAtrribute.isOn ? 0 : 1);
                keyframes[2 * i + 2] = new Keyframe(enwidenLaneAtrribute.up + enwidenLaneAtrribute.duration, enwidenLaneAtrribute.isOn ? 1 : 0);
            }

            keyframes[^1] = new Keyframe(_theGameManager.drbfile.CalculateDRBFileTime(10000), list[^1].isOn ? 1 : 0);
            Util.LinearKeyframe(keyframes);
            return new AnimationCurve(keyframes);
        }

        private void Update()
        {
            if (!_theGameManager) return;
            if (!_theGameManager.IsPlaying) return;
            float value = lanesCurve.Evaluate(_theGameManager.progressManager.NowTime);
            leftLane.color = leftLane.color.ModifyAlpha(value);
            rightLane.color = rightLane.color.ModifyAlpha(value);
            float laneSize = value * 2.0625f;
            float f = 4f * value;
            leftLane.transform.localPosition = leftLane.transform.localPosition.ModifyX(-8f - laneSize);
            rightLane.transform.localPosition = rightLane.transform.localPosition.ModifyX(8f + laneSize);
            leftLane.transform.localScale = leftLane.transform.localScale.ModifyX(laneSize);
            rightLane.transform.localScale = rightLane.transform.localScale.ModifyX(laneSize);
            leftSpectrum.transform.localPosition = leftSpectrum.transform.localPosition.ModifyX(-f);
            rightspectrum.transform.localPosition = rightspectrum.transform.localPosition.ModifyX(f);
            HanteiLine1.transform.localScale = HanteiLine1.transform.localScale.ModifyX(8f + f);
            HanteiLine2.transform.localScale = HanteiLine2.transform.localScale.ModifyX(8f + f);
        }
    }

    public struct EnwidenLaneAtrributes
    {
        public int up;
        public int duration;
        public bool isOn;
    }
}