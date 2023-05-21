using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace DRFV
{
  [Preserve]
  public class SkipUnityLogo
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void BeforeSplashScreen() => Task.Run(new Action(AsyncSkip));

    private static void AsyncSkip() => SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
  
  }
}