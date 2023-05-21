using System;
using DRFV.inokana;
using DRFV.Setting;
using OBSWebsocketDotNet;

namespace DRFV.Global.Managers
{
    public class OBSManager : MonoSingleton<OBSManager>
    {
        public bool isActive;
        private OBSWebsocket obs = new();

        protected override void OnAwake()
        {
            if (GlobalSettings.CurrentSettings.OBSRecord) EnableRecordMode();
        }

        public bool EnableRecordMode()
        {
            if (obs.IsConnected) return true;
            obs.WSTimeout = TimeSpan.FromSeconds(3.0f);
            obs.Connect("ws://localhost:4444", null);
            if (obs.IsConnected)
            {
                isActive = true;
                return true;
            }

            NotificationBarManager.Instance.Show("无法连接至OBS WebSockets，请确认端口为4444并且禁用密码");
            isActive = false;
            return false;
        }

        public void DisableRecordMode()
        {
            isActive = false;
            if (obs.IsConnected) obs.Disconnect();
        }

        public void OnApplicationQuit()
        {
            DisableRecordMode();
        }

        public void StartRecording()
        {
            if (!isActive) return;
            try
            {
                obs.StartRecording();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void StopRecording()
        {
            if (!isActive) return;
            try
            {
                obs.StopRecording();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}