using System.Text;
using DRFV.inokana;
using DRFV.Login;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Global.Components
{
    public class InputWindow : MonoBehaviour
    {
        [SerializeField] private Image _bg;
        [SerializeField] private Text _label;
        [SerializeField] private InputField _inputField;
        private string _onError;
        private Callback _callback = null;
        private byte[] _password;

        public delegate void Callback(bool value);

        private void Awake()
        {
            _bg.color = new Color(0, 0, 0, 0);
        }

        public void Show([CanBeNull] string title, [CanBeNull] string onError, byte[] password,
            Callback callback)
        {
            title ??= "请输入密码...";
            _onError = onError ?? "密码错误";
            _password = password;
            _label.text = title;
            _callback = callback;
            _bg.color = new Color(0, 0, 0, 200f / 255f);
        }

        public void Confirm()
        {
            if (_password.Length < 1 || Encoding.UTF8.GetString(_password) == _inputField.text || AccountInfo.Instance.acountStatus == AccountInfo.AcountStatus.DEMO)
            {
                _callback?.Invoke(true);
                Destroy(gameObject);
            }
            else
            {
                if (NotificationBarManager.Instance) NotificationBarManager.Instance.Show(_onError);
            }
        }

        public void Cancel()
        {
            _callback?.Invoke(false);
            Destroy(gameObject);
        }
    }
}