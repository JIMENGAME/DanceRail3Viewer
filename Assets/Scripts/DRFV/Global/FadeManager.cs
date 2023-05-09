using System.Collections;
using DRFV.inokana;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DRFV.Global
{
  public class FadeManager : MonoSingleton<FadeManager>
  {
    [SerializeField]
    private Texture img;
    [SerializeField]
    private int width = 16;
    [SerializeField]
    private int height = 9;
    private float[,] Scale;
    private Vector2 blocksize;
    private bool flag;
    public GameObject mask;

    protected override void OnAwake()
    {
      Scale = new float[width, height];
      for (int index1 = 0; index1 < width; ++index1)
      {
        for (int index2 = 0; index2 < height; ++index2)
          Scale[index1, index2] = 0.0f;
      }
    }

    public void LoadScene(string SceneName, GlobalSettings globalSettings = null)
    {
      if (globalSettings != null) GlobalSettings.CurrentSettings = globalSettings;
      if (flag)
        return;
      blocksize = new Vector2(Screen.width / (float) width, Screen.height / (float) height);
      StartCoroutine(ShowFade(SceneName));
    }

    public bool isFading() => flag;

    private IEnumerator ShowFade(string SceneName)
    {
      Resources.UnloadUnusedAssets();
      flag = true;
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
      for (int index1 = 0; index1 < width; ++index1)
      {
        for (int index2 = 0; index2 < height; ++index2)
          GUI.DrawTextureWithTexCoords(new Rect((float) (blocksize.x * (double) index1 + (1.0 - Scale[index1, index2]) * 0.5 * blocksize.x), (float) (blocksize.y * (double) index2 + (1.0 - Scale[index1, index2]) * 0.5 * blocksize.y), Scale[index1, index2] * blocksize.x, Scale[index1, index2] * blocksize.y), img, new Rect((float) (0.5 * (1.0 - Scale[index1, index2])), (float) (0.5 * (1.0 - Scale[index1, index2])), Scale[index1, index2], Scale[index1, index2]));
      }
    }
  }
}
