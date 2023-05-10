using System.Collections;
using DRFV.Enums;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Offset
{
    public class TheNoteOffset : MonoBehaviour
    {
        private TheOffsetManager _offsetManager;
        private InputManager _inputManager;
        SpriteRenderer spriteRenderer;

        NoteData _noteData = new();
        
        static Color PJColor = new Color(1.0f, 0.9f, 0.1f, 1.0f);

        public void Ready(NoteData noteData, int tapSize, int tapAlpha)
        {
            _noteData.id = noteData.id;
            _noteData.time = noteData.time;
            _noteData.pos = noteData.pos;
            _noteData.center = noteData.pos + noteData.width * 0.5f;
            _noteData.mode = noteData.mode;
            _noteData.nsc = noteData.nsc;
            _noteData.ms = noteData.ms;
            _noteData.dms = noteData.dms;
            _noteData.width = noteData.width;
            _noteData.kind = NoteKind.TAP;
            _noteData.isJudgeTimeRangeConflicted = noteData.isJudgeTimeRangeConflicted;
            spriteRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer arrorRenderer = transform.Find("Arror").GetComponent<SpriteRenderer>();
            spriteRenderer.size = new Vector2(_noteData.width, 1.0f);
            arrorRenderer.sprite = _offsetManager.GetSpriteArror();
            arrorRenderer.transform.position += new Vector3(0, tapSize * 0.6f, 0f);
            arrorRenderer.transform.localScale += new Vector3(tapSize * 2, tapSize * 2, 0);
            arrorRenderer.color = new Color(1.0f, 1.0f, 1.0f, tapAlpha / 3f);
        }

        private IEnumerator _judgeEnumerator;

        public void StartC()
        {
            //判定処理
            StartCoroutine(AutoJudge());
        }

        void DistroyThis()
        {
            //DESTROY
            Destroy(gameObject);
        }
        
        float z;

        // Update is called once per frame
        void LateUpdate()
        {
            z = 0.01f * (_noteData.dms - _offsetManager.NowTime) * _noteData.nsc.value * _offsetManager.NoteSpeed;

            transform.position = new Vector3(_noteData.center - 8.0f, 0.1f, z);
            spriteRenderer.size = new Vector2(_noteData.width, 1.0f + (z / 80.0f));
        }
        
        IEnumerator AutoJudge()
        {
            while (_offsetManager.NowTime < _noteData.ms)
            {
                yield return null;
            }
            
            _offsetManager.Judge(new Vector3(transform.position.x, 0.0f, 0.0f), _noteData.width);
            if (_noteData.kind == NoteKind.TAP || _noteData.kind == NoteKind.ExTAP)
            {
                _inputManager.SetBeamColor(_noteData.pos, _noteData.pos + _noteData.width, PJColor);
            }

            DistroyThis();
        }


        public void SetGMIMG(TheOffsetManager om, InputManager im)
        {
            _offsetManager = om;
            _inputManager = im;
        }
    }
}