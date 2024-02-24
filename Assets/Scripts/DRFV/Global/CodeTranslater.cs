#if UNITY_EDITOR
using System.Text;
#endif
using UnityEngine;

namespace DRFV.Global
{
    public class CodeTranslater : MonoBehaviour
    {
#if UNITY_EDITOR
        /**
         * 0: Debug Mode
         * 1: HadouTest
         * 2: API
         * 3: UA
        **/
        public string[] codes;

        private void Start()
        {
            foreach (string code in codes)
            {
                Debug.Log(code + ": " + ConvertToByteArray(code));
            }
        }

        private string ConvertToByteArray(string content, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            if (content.Equals("")) return "";
            byte[] arr = encoding.GetBytes(content);
            StringBuilder stringBuilder = new StringBuilder("{");
            for (int i = 0; i < arr.Length - 1; i++)
            {
                stringBuilder.Append(arr[i]).Append(", ");
            }

            stringBuilder.Append(arr[^1]).Append("}");
            return stringBuilder.ToString();
        }
#endif
    }
}
