using DRFV.inokana;
using UnityEngine;

namespace DRFV.Global
{
    public class DebugModeController : MonoSingleton<DebugModeController>
    {
        public GameObject InGameDebugConsole;

        private GameObject InGameDebugConsoleInstance;
    
        private bool _debugMode;

        public bool DebugMode
        {
            get => _debugMode;
            set
            {
                _debugMode = value;
                if (value)
                {
                    InGameDebugConsoleInstance = Instantiate(InGameDebugConsole);
                }
                else
                {
                    if (InGameDebugConsoleInstance) Destroy(InGameDebugConsoleInstance);
                }
            }
        }
    }
}