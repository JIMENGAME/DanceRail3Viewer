using System;
using System.Collections;
using DRFV.Enums;
using DRFV.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DRFV.Game
{
    public class TheNote : MonoBehaviour
    {
        private bool isWaitForGD;
        private bool isWaitForPF;
        private float WaitForSec;

        private TheGameManager gameManager;
        private InputManager inputManager;
        private SpriteRenderer spriteRenderer;

        [SerializeField] private Material _material1Light,
            _material2Light,
            _material3Light,
            _material4Light,
            _material5Light,
            _material6Light,
            _material7Light;

        [SerializeField] private Material _material1Dark,
            _material2Dark,
            _material3Dark,
            _material4Dark,
            _material5Dark,
            _material6Dark,
            _material7Dark;

        private Material _material1, _material2, _material3, _material4, _material5, _material6, _material7;

        private Mesh _mesh;
        public MeshDrawer mDrawer;

        public AnimationCurve acNSC;
        bool judge_flag = false;

        private Vector3[] _positions = new Vector3[]
        {
            new Vector3(1000, 1000, 0),
            new Vector3(1000, 1000, 0),
            new Vector3(1000, 1000, 0),
            new Vector3(1000, 1000, 0),
            new Vector3(1000, 1000, 0),
            new Vector3(1000, 1000, 0),
            new Vector3(1000, 1000, 0),
            new Vector3(1000, 1000, 0),
        };

        private int[] _triangle = new int[]
        {
            0, 1, 2,
            2, 1, 3,
            2, 3, 4,
            4, 3, 5,
            4, 5, 6,
            6, 5, 7,
        };

        private Vector3[] _normals = new Vector3[]
        {
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
            new Vector3(0, 0, -1),
        };

        private Vector2[] _uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(0.25f, 0),
            new Vector2(0.25f, 1),
            new Vector2(0.75f, 0),
            new Vector2(0.75f, 1),
            new Vector2(1, 0),
            new Vector2(1, 1),
        };

        NoteData _noteData = new();

        public bool flag = false;


        float effect_center_start, effect_center_end;
        float angle_center_start, angle_center_end;
        float stereo_center_start, stereo_center_end;

        static Color PJColor = new Color(1.0f, 0.9f, 0.1f, 1.0f),
            PFColor = new Color(0.9f, 0.4f, 0.1f, 1.0f),
            GDColor = new Color(0.1f, 0.8f, 0.1f, 1.0f);

        // Use this for initialization
        void Start()
        {
            _mesh = new Mesh();

            _mesh.vertices = _positions;
            _mesh.triangles = _triangle;
            _mesh.normals = _normals;
            _mesh.uv = _uvs;

            //_mesh.RecalculateBounds();
        }

        public void Ready(NoteData noteData, int tapSize, int flickSize, int freeFlickSize, int tapAlpha,
            int flickAlpha, int freeFlickAlpha)
        {
            _noteData = noteData;
            _noteData.center = _noteData.pos + _noteData.width * 0.5f;
            if (!_noteData.IsTail())
            {
                _noteData.parent = 0;
                _noteData.parent_ms = 0;
                _noteData.parent_dms = 0;
                _noteData.parent_pos = 0;
                _noteData.parent_width = 0;
            }

            isWaitForGD = false;
            isWaitForPF = false;
            WaitForSec = 0.0f;

            spriteRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer arrorRenderer = transform.Find("Arror").GetComponent<SpriteRenderer>();
            TextMesh IdText = transform.Find("Text").GetComponent<TextMesh>();

            spriteRenderer.size = new Vector2(_noteData.width, 1.0f);
            if (_noteData.IsCenter())
            {
                if (_noteData.kind != NoteKind.BOOM_CENTER) spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.01f);
            }

            if (gameManager.DebugMode && gameManager.ShowNoteId)
            {
                IdText.transform.localPosition = new Vector3(_noteData.width / -2, tapSize * 0.6f, -0.3f);
                IdText.text = _noteData.id + "";
                IdText.gameObject.SetActive(true);
            }

            switch (gameManager.gameSide)
            {
                case GameSide.LIGHT:
                case GameSide.COLORLESS:
                    _material1 = _material1Light;
                    _material2 = _material2Light;
                    _material3 = _material3Light;
                    _material4 = _material4Light;
                    _material5 = _material5Light;
                    _material6 = _material6Light;
                    _material7 = _material7Light;
                    break;
                case GameSide.DARK:
                    _material1 = _material1Dark;
                    _material2 = _material2Dark;
                    _material3 = _material3Dark;
                    _material4 = _material4Dark;
                    _material5 = _material5Dark;
                    _material6 = _material6Dark;
                    _material7 = _material7Dark;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            arrorRenderer.sprite = gameManager.GetSpriteArror(_noteData.kind switch
            {
                NoteKind.TAP => 1,
                NoteKind.ExTAP => 2,
                NoteKind.FLICK_LEFT => 3,
                NoteKind.FLICK_RIGHT => 4,
                NoteKind.FLICK_UP => 5,
                NoteKind.FLICK_DOWN => 6,
                NoteKind.FLICK => 7,
                _ => 0
            });

            if (_noteData.IsTap())
            {
                arrorRenderer.transform.position += new Vector3(0, tapSize * 0.6f, 0f);
                arrorRenderer.transform.localScale += new Vector3(tapSize * 2, tapSize * 2, 0);
                arrorRenderer.color = new Color(1.0f, 1.0f, 1.0f, tapAlpha / 3f);
            }
            else if (_noteData.IsFlick())
            {
                arrorRenderer.transform.position += new Vector3(0, flickSize * 0.6f, 0);
                arrorRenderer.transform.localScale += new Vector3(flickSize * 2, flickSize * 2, 0);
                arrorRenderer.color = new Color(1.0f, 1.0f, 1.0f, flickAlpha / 3f);
            }
            else if (_noteData.kind == NoteKind.FLICK)
            {
                arrorRenderer.transform.position += new Vector3(0, freeFlickSize * 0.6f, 0);
                arrorRenderer.transform.localScale += new Vector3(freeFlickSize * 2, freeFlickSize * 2, 0);
                arrorRenderer.color = new Color(1.0f, 1.0f, 1.0f, freeFlickAlpha / 3f);
            }
            else
            {
                arrorRenderer.enabled = false;
            }

            effect_center_start = (_noteData.parent_pos + _noteData.parent_width * 0.5f) / 16.0f;
            effect_center_end = (_noteData.pos + _noteData.width * 0.5f) / 16.0f;
            angle_center_start = (_noteData.parent_pos + _noteData.parent_width * 0.5f - 8.0f) / 4.0f;
            angle_center_end = (_noteData.pos + _noteData.width * 0.5f - 8.0f) / 4.0f;
            stereo_center_start = (_noteData.parent_pos + _noteData.parent_width * 0.5f - 8.0f) / 6.0f;
            stereo_center_end = (_noteData.pos + _noteData.width * 0.5f - 8.0f) / 6.0f;

            if (_noteData.IsTail())
            {
                _noteData.nsc = NoteData.NoteSC.GetCommonNSC();
            }

            if (_noteData.mode == NoteAppearMode.Jump)
            {
                _noteData.nsc =
                    NoteData.NoteSC.Parse(
                        "1:0;0.9:0.31;0.8:0.59;0.7:0.81;0.6:0.95;0.5:1;0.4:0.95;0.3:0.81;0.2:0.59;0.1:0.31;0:0");
            }

            //nsc処理
            if (_noteData.nsc.type == NoteSCType.MULTI)
            {
                Keyframe[] nsckey = new Keyframe[_noteData.nsc.data.Count];
                for (int ii = 0; ii < _noteData.nsc.data.Count; ii++)
                {
                    nsckey[ii] =
                        new Keyframe(
                            gameManager.BPMCurve.Evaluate(_noteData.time - _noteData.nsc.data[ii].realValue),
                            gameManager.BPMCurve.Evaluate(_noteData.time - _noteData.nsc.data[ii].value));
                }

                _noteData.maxtime = nsckey[0].time;

                Util.LinearKeyframe(nsckey);
                acNSC = new AnimationCurve(nsckey);
            }
        }

        private IEnumerator _judgeEnumerator;

        public void StartC()
        {
            //判定処理
            judge_flag = false;
            flag = false;
            // _noteData.isNear = false;
            isWaitForGD = false;
            isWaitForPF = false;
            _judgeEnumerator = gameManager.GameAuto ? AutoJudge() : Judge();
            StartCoroutine(_judgeEnumerator);
        }

        public void StopC()
        {
            if (_judgeEnumerator == null) return;
            StopCoroutine(_judgeEnumerator);
            _judgeEnumerator = null;
        }

        void Update()
        {
            //Effect処理
            if (gameManager.GameEffectParamEQLevel >= 1 &&
                gameManager.progressManager.NowTime + 100 >= _noteData.parent_ms &&
                gameManager.progressManager.NowTime + 100 < _noteData.ms)
            {
                float qwq = effect_center_start + (effect_center_end - effect_center_start) *
                    (gameManager.progressManager.NowTime + 100 - _noteData.parent_ms) /
                    (_noteData.ms - _noteData.parent_ms);
                float stereo = stereo_center_start + (stereo_center_end - stereo_center_start) *
                    (gameManager.progressManager.NowTime + 100 - _noteData.parent_ms) /
                    (_noteData.ms - _noteData.parent_ms);
                switch (_noteData.kind)
                {
                    case NoteKind.SLIDE_CENTER:
                    case NoteKind.SLIDE_END:
                    {
                        gameManager.AddEQ(qwq);
                        break;
                    }
                    case NoteKind.HPass_CENTER:
                    case NoteKind.HPass_END:
                    {
                        gameManager.AddHPass(qwq);
                        break;
                    }
                    case NoteKind.LPass_CENTER:
                    case NoteKind.LPass_END:
                    {
                        gameManager.AddLPass(qwq);
                        break;
                    }
                    case NoteKind.STEREO_CENTER:
                    case NoteKind.STEREO_END:
                    {
                        gameManager.AddStereo(stereo);
                        break;
                    }
                }
            }

            //斜め処理
            if (_noteData.IsTail())
            {
                if (gameManager.progressManager.NowTime >= _noteData.parent_ms &&
                    gameManager.progressManager.NowTime < _noteData.ms)
                {
                    float p = (gameManager.progressManager.NowTime - _noteData.parent_ms) /
                              (_noteData.ms - _noteData.parent_ms);
                    gameManager.AddAngel(angle_center_start + (angle_center_end - angle_center_start) * p);
                }
            }
        }

        void DistroyThis()
        {
            //DESTROY
            Destroy(gameObject);
        }

        float distance, progress;
        float z, pz, x, y;

        // Update is called once per frame
        void LateUpdate()
        {
            distance = (float)gameManager.Distance;
            progress = gameManager.progressManager.NowTime;
            if (_noteData.nsc.type == NoteSCType.MULTI)
            {
                z = _noteData.maxtime > progress
                    ? 300.0f
                    : 0.01f * (_noteData.ms - acNSC.Evaluate(progress)) * gameManager.NoteSpeed;
                pz = 0.01f * (_noteData.parent_dms - distance);
            }
            else
            {
                z = 0.01f * (_noteData.dms - distance) * _noteData.nsc.value * gameManager.NoteSpeed;
                pz = 0.01f * (_noteData.parent_dms - distance) * gameManager.NoteSpeed;
            }

            x = _noteData.mode switch
            {
                NoteAppearMode.Left => _noteData.center - 8.0f - z * 0.2f,
                NoteAppearMode.Right => _noteData.center - 8.0f + z * 0.2f,
                _ => _noteData.center - 8.0f
            };
            if (_noteData.mode == NoteAppearMode.High)
            {
                y = _noteData.IsTail() ? 0.0f : 0.1f + z * 0.1f;
            }
            else
            {
                y = _noteData.IsTail() ? 0.0f : 0.1f;
            }

            transform.position = new Vector3(x, y, z);
            spriteRenderer.size = new Vector2(_noteData.width, 1.0f + (z / 80.0f));

            if (_noteData.IsTail())
            {
                _positions = new Vector3[]
                {
                    new Vector3(_noteData.parent_pos - 8.0f, -0.1f, pz),
                    new Vector3(_noteData.pos - 8.0f, -0.1f, z),
                    new Vector3(_noteData.parent_pos + 0.4f - 8.0f, -0.1f, pz),
                    new Vector3(_noteData.pos + 0.4f - 8.0f, -0.1f, z),
                    new Vector3(_noteData.parent_pos + _noteData.parent_width - 0.4f - 8.0f, -0.1f, pz),
                    new Vector3(_noteData.pos + _noteData.width - 0.4f - 8.0f, -0.1f, z),
                    new Vector3(_noteData.parent_pos + _noteData.parent_width - 8.0f, -0.1f, pz),
                    new Vector3(_noteData.pos + _noteData.width - 8.0f, -0.1f, z),
                };

                _mesh.vertices = _positions;
                _mesh.RecalculateBounds();
                switch (_noteData.kind)
                {
                    //Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);
                    case NoteKind.HOLD_END:
                    case NoteKind.HOLD_CENTER:
                        mDrawer.AddQue(_mesh, _material1);
                        break;
                    case NoteKind.SLIDE_CENTER:
                    case NoteKind.SLIDE_END:
                        mDrawer.AddQue(_mesh, _material2);
                        break;
                    case NoteKind.BOOM_CENTER:
                    case NoteKind.BOOM_END:
                        mDrawer.AddQue(_mesh, _material3);
                        break;
                    case NoteKind.HPass_CENTER:
                    case NoteKind.HPass_END:
                        mDrawer.AddQue(_mesh, _material4);
                        break;
                    case NoteKind.LPass_CENTER:
                    case NoteKind.LPass_END:
                        mDrawer.AddQue(_mesh, _material5);
                        break;
                    case NoteKind.MOVER_CENTER:
                    case NoteKind.MOVER_END:
                        mDrawer.AddQue(_mesh, _material6);
                        break;
                    case NoteKind.STEREO_CENTER:
                    case NoteKind.STEREO_END:
                        mDrawer.AddQue(_mesh, _material7);
                        break;
                    case NoteKind.MOVERBOMB_CENTER:
                    case NoteKind.MOVERBOMB_END:
                        // TODO: Mover Bomb的材质
                        break;
                }
            }
        }

        IEnumerator Judge()
        {
            if (judge_flag) yield break;

            while (gameManager.progressManager.NowTime < _noteData.ms - gameManager.GDms)
            {
                yield return null;
            }

            while (gameManager.progressManager.NowTime <= _noteData.ms + gameManager.GDms)
            {
                //TOUCH操作
                if (!gameManager.isPause)
                {
                    switch (_noteData.kind)
                    {
                        //TAP必要
                        case NoteKind.TAP:
                            if (_noteData.isJudgeTimeRangeConflicted)
                            {
                                if (isWaitForGD &&
                                    gameManager.progressManager.NowTime > _noteData.ms + gameManager.PFms)
                                {
                                    gameManager.AccMSList.Add((float)(100.0 - (Mathf.Abs(WaitForSec) > 10.0
                                        ? Mathf.Abs(WaitForSec) - 10.0
                                        : 0.0)));
                                    gameManager.Judge(WaitForSec, _noteData.kind,
                                        new Vector3(transform.position.x, 0.0f, 0.0f), _noteData.width);

                                    inputManager.SetBeamColor(_noteData.pos, _noteData.pos + _noteData.width, GDColor);

                                    judge_flag = true;
                                    DistroyThis();
                                    break;
                                }

                                if (isWaitForPF &&
                                    gameManager.progressManager.NowTime > _noteData.ms + gameManager.PJms)
                                {
                                    gameManager.AccMSList.Add((float)(100.0 - (Mathf.Abs(WaitForSec) > 10.0
                                        ? Mathf.Abs(WaitForSec) - 10.0
                                        : 0.0)));
                                    gameManager.Judge(WaitForSec, _noteData.kind,
                                        new Vector3(transform.position.x, 0.0f, 0.0f), _noteData.width);
                                    judge_flag = true;
                                    DistroyThis();
                                    break;
                                }

                                if (inputManager.GetTrigger(_noteData.pos, _noteData.pos + _noteData.width))
                                {
                                    if (gameManager.progressManager.NowTime < _noteData.ms - gameManager.PFms)
                                    {
                                        isWaitForGD = true;
                                        WaitForSec = (gameManager.progressManager.NowTime - _noteData.ms);

                                        //inputManager.SetBeamColor(pos, pos + width, GDColor);
                                    }
                                    else if (gameManager.progressManager.NowTime < _noteData.ms - gameManager.PJms)
                                    {
                                        isWaitForPF = true;
                                        WaitForSec = (gameManager.progressManager.NowTime - _noteData.ms);
                                        inputManager.SetBeamColor(_noteData.pos, _noteData.pos + _noteData.width,
                                            PFColor);
                                    }
                                    else //(gameManager.Progress >= ms - gameManager.PJms)
                                    {
                                        gameManager.AccMSList.Add((float)(100.0 -
                                                                          (Mathf.Abs((float)gameManager.progressManager
                                                                              .NowTime - _noteData.ms) > 10.0
                                                                              ? Mathf.Abs(
                                                                                  (float)gameManager.progressManager
                                                                                      .NowTime - _noteData.ms) - 10.0
                                                                              : 0.0)));
                                        gameManager.Judge((gameManager.progressManager.NowTime - _noteData.ms),
                                            _noteData.kind,
                                            new Vector3(transform.position.x, 0.0f, 0.0f), _noteData.width);
                                        inputManager.SetBeamColor(_noteData.pos, _noteData.pos + _noteData.width,
                                            Mathf.Abs((gameManager.progressManager.NowTime - _noteData.ms)) <=
                                            gameManager.PJms
                                                ? PJColor
                                                : Mathf.Abs((gameManager.progressManager.NowTime - _noteData.ms)) <=
                                                  gameManager.PFms
                                                    ? PFColor
                                                    : GDColor);
                                        judge_flag = true;
                                        DistroyThis();
                                    }
                                }
                            }
                            else
                            {
                                if (inputManager.GetTrigger(_noteData.pos, _noteData.pos + _noteData.width))
                                {
                                    gameManager.AccMSList.Add(100f -
                                                              Mathf.Abs(gameManager.progressManager.NowTime -
                                                                        _noteData.ms));
                                    gameManager.Judge((gameManager.progressManager.NowTime - _noteData.ms),
                                        _noteData.kind,
                                        new Vector3(transform.position.x, 0.0f, 0.0f), _noteData.width);
                                    inputManager.SetBeamColor(_noteData.pos, _noteData.pos + _noteData.width,
                                        Mathf.Abs(gameManager.progressManager.NowTime - _noteData.ms) <=
                                        gameManager.PJms
                                            ? PJColor
                                            : Mathf.Abs(gameManager.progressManager.NowTime - _noteData.ms) <=
                                              gameManager.PFms
                                                ? PFColor
                                                : GDColor);
                                    judge_flag = true;
                                    DistroyThis();
                                }
                            }

                            break;
                        case NoteKind.ExTAP:
                            if (inputManager.GetTrigger(_noteData.pos, _noteData.pos + _noteData.width))
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                inputManager.SetBeamColor(_noteData.pos, _noteData.pos + _noteData.width, PJColor);
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;
                        //PRESS RELEASE OK
                        case NoteKind.HOLD_END:
                        case NoteKind.SLIDE_CENTER:
                        case NoteKind.SLIDE_END:
                        case NoteKind.HOLD_CENTER:
                        case NoteKind.HPass_CENTER:
                        case NoteKind.HPass_END:
                        case NoteKind.LPass_CENTER:
                        case NoteKind.LPass_END:
                        case NoteKind.MOVER_CENTER:
                        case NoteKind.MOVER_END:
                        case NoteKind.STEREO_CENTER:
                        case NoteKind.STEREO_END:
                            if (inputManager.GetRelease(_noteData.pos, _noteData.pos + _noteData.width) && !flag)
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                flag = true;
                            }
                            else if (inputManager.GetPressed(_noteData.pos, _noteData.pos + _noteData.width) && !flag)
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                flag = true;
                            }

                            if (gameManager.progressManager.NowTime >= _noteData.ms && flag)
                            {
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;
                        //PRESS 必要
                        case NoteKind.HOLD_START:
                        case NoteKind.SLIDE_START:
                        case NoteKind.STEREO_START:
                            if (inputManager.GetPressed(_noteData.pos, _noteData.pos + _noteData.width) && !flag)
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                flag = true;
                            }

                            if (gameManager.progressManager.NowTime >= _noteData.ms && flag)
                            {
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;
                        //FLICK必要
                        case NoteKind.FLICK:
                            if (inputManager.GetFlick(_noteData.pos, _noteData.pos + _noteData.width))
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;
                        case NoteKind.FLICK_LEFT:
                            if (inputManager.GetFlickLeft(_noteData.pos, _noteData.pos + _noteData.width))
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;
                        case NoteKind.FLICK_RIGHT:
                            if (inputManager.GetFlickRight(_noteData.pos, _noteData.pos + _noteData.width))
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;
                        case NoteKind.FLICK_UP:
                            if (inputManager.GetFlickUp(_noteData.pos, _noteData.pos + _noteData.width))
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;
                        case NoteKind.FLICK_DOWN:
                            if (inputManager.GetFlickDown(_noteData.pos, _noteData.pos + _noteData.width))
                            {
                                gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                    _noteData.width);
                                judge_flag = true;
                                DistroyThis();
                            }

                            break;

                        //避ける必要
                        case NoteKind.BOOM:
                        case NoteKind.BOOM_CENTER:
                        case NoteKind.BOOM_END:
                        case NoteKind.MOVERBOMB_CENTER:
                        case NoteKind.MOVERBOMB_END:
                            if (gameManager.progressManager.NowTime >= _noteData.ms)
                            {
                                if (inputManager.GetPressed(_noteData.pos + 1, _noteData.pos + _noteData.width - 1))
                                {
                                    gameManager.Judge(gameManager.GDms * 2f, _noteData.kind,
                                        new Vector3(transform.position.x, 0.0f, 0.0f),
                                        _noteData.width);
                                    //血が出る警告
                                    gameManager.ShowHPMask();
                                    judge_flag = true;
                                    DistroyThis();
                                }
                                else
                                {
                                    gameManager.Judge(0, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                                        _noteData.width);
                                    judge_flag = true;
                                    DistroyThis();
                                }
                            }

                            break;
                    }
                }

                yield return null;
            }

            //BOOM NOTE 以外
            if (!judge_flag)
            {
                gameManager.Judge(2 * gameManager.GDms, _noteData.kind, new Vector3(transform.position.x, 0.0f, 0.0f),
                    _noteData.width);
                if (_noteData.kind == NoteKind.TAP)
                {
                    gameManager.AccMSList.Add(0.0f);
                }

                judge_flag = true;
                DistroyThis();
            }
        }

        IEnumerator AutoJudge()
        {
            while (gameManager.progressManager.NowTime < _noteData.ms)
            {
                yield return null;
            }

#if true
            float randomms = Random.Range(-gameManager.PJms * 0.9f, gameManager.PJms * 0.9f);
            if (_noteData.kind == NoteKind.TAP) gameManager.AccMSList.Add(100.0f - Mathf.Abs(randomms));
            gameManager.Judge(_noteData.kind != NoteKind.TAP ? 0 : randomms, _noteData.kind,
                new Vector3(transform.position.x, 0.0f, 0.0f), _noteData.width);
#else
            if (_noteData.kind == NoteKind.TAP) gameManager.AccMSList.Add(100.0f);
            gameManager.Judge(0, _noteData.kind,
                new Vector3(transform.position.x, 0.0f, 0.0f), _noteData.width);
#endif
            if (_noteData.kind == NoteKind.TAP || _noteData.kind == NoteKind.ExTAP)
            {
                inputManager.SetBeamColor(_noteData.pos, _noteData.pos + _noteData.width, PJColor);
            }

            judge_flag = true;
            DistroyThis();
        }


        public void SetGMIMG(TheGameManager gm, InputManager im)
        {
            gameManager = gm;
            inputManager = im;
        }
    }
}