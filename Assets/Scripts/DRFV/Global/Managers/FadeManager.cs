using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DRFV.inokana;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DRFV.Global.Managers
{
    public class FadeManager : MonoSingleton<FadeManager>
    {
        class SceneInfo
        {
            public string scene = "";
        }
        [SerializeField] private Texture img;
        [SerializeField] private int width = 16;
        [SerializeField] private int height = 9;
        private float[,] Scale;
        private Vector2 blocksize;
        private bool flag;
        public GameObject mask;
        private Stack<SceneInfo> scenesStack = new();
        private Exception stackIsEmptyException => new InvalidOperationException("Stack is empty");


        protected override void OnAwake()
        {
            Scale = new float[width, height];
            for (int index1 = 0; index1 < width; ++index1)
            {
                for (int index2 = 0; index2 < height; ++index2)
                    Scale[index1, index2] = 0.0f;
            }
        }

        /// <summary>
        /// 跳转 不可返回
        /// </summary>
        /// <param name="SceneName">场景名称</param>
        /// <exception cref="InvalidOperationException">场景Stack为空</exception>
        public void JumpScene(string SceneName)
        {
            if (flag) return;
            flag = true;
            
            if (scenesStack.Count == 0)
            {
                scenesStack.Push(new()
                {
                    scene = SceneName,
                });
                return;
            }

            if (!scenesStack.TryPeek(out var item))
                throw stackIsEmptyException;

            item.scene = SceneName;
            DoScene(SceneName);
        }

        /// <summary>
        /// 跳转 可以返回
        /// </summary>
        /// <param name="SceneName">场景名称</param>
        /// <exception cref="InvalidOperationException">场景Stack为空</exception>
        public void LoadScene(string SceneName)
        {
            if (flag) return;
            flag = true;
            if (scenesStack.Count == 0)
                throw stackIsEmptyException;

            scenesStack.Push(new()
            {
                scene = SceneName
            });
            DoScene(SceneName);
        }

        public void Back()
        {
            if (flag) return;
            flag = true;
            if (scenesStack.Count == 1)
                return;

            scenesStack.Pop();
            var lastScene = scenesStack.Peek();
            DoScene(lastScene.scene);
        }

        private void DoScene(string SceneName)
        {
            if (flag) return;
            flag = true;
            blocksize = new Vector2(Screen.width / (float)width, Screen.height / (float)height);
            StartCoroutine(ShowFade(SceneName));
        }

        public bool isFading() => flag;

        private IEnumerator ShowFade(string SceneName)
        {
            Resources.UnloadUnusedAssets();
            if (mask) mask.SetActive(flag);
            int timer = 0;
            while (true)
            {
                for (int index1 = 0; index1 < width; ++index1)
                {
                    for (int index2 = 0; index2 < height; ++index2)
                    {
                        if (index1 + index2 < timer)
                        {
                            Scale[index1, index2] += 0.1f;
                            if (Scale[index1, index2] >= 1.0)
                                Scale[index1, index2] = 1f;
                        }
                    }
                }

                ++timer;
                if (timer < width + height + 11)
                    yield return new WaitForSeconds(0.01f);
                else
                    break;
            }
            DOTween.KillAll();
            yield return null;
            SceneManager.LoadScene(SceneName);
            yield return null;
            timer = 0;
            while (true)
            {
                for (int index3 = 0; index3 < width; ++index3)
                {
                    for (int index4 = 0; index4 < height; ++index4)
                    {
                        if (index3 + index4 < timer)
                        {
                            Scale[index3, index4] -= 0.1f;
                            if (Scale[index3, index4] <= 0.0)
                                Scale[index3, index4] = 0.0f;
                        }
                    }
                }

                ++timer;
                if (timer < width + height + 11)
                    yield return new WaitForSeconds(0.01f);
                else
                    break;
            }

            flag = false;
            if (mask) mask.SetActive(flag);
            yield return null;
        }

        private void OnGUI()
        {
            if (!flag) return;
            for (int index1 = 0; index1 < width; ++index1)
            {
                for (int index2 = 0; index2 < height; ++index2)
                    GUI.DrawTextureWithTexCoords(
                        new Rect(
                            (float)(blocksize.x * (double)index1 + (1.0 - Scale[index1, index2]) * 0.5 * blocksize.x),
                            (float)(blocksize.y * (double)index2 + (1.0 - Scale[index1, index2]) * 0.5 * blocksize.y),
                            Scale[index1, index2] * blocksize.x, Scale[index1, index2] * blocksize.y), img,
                        new Rect((float)(0.5 * (1.0 - Scale[index1, index2])),
                            (float)(0.5 * (1.0 - Scale[index1, index2])), Scale[index1, index2],
                            Scale[index1, index2]));
            }
        }
    }
}