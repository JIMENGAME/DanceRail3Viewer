using UnityEngine;

namespace DRFV.Global
{

    public class ScriptString : MonoBehaviour
    {
        public static string RemoveSlash(string str) => str;

        public static string RemoveComment(string str)
        {
            string str1 = str;
            if (str1.IndexOf("//") >= 0)
                str1 = str1.Substring(0, str1.IndexOf("//"));
            return str1;
        }

        public static string RemoveSpace(string str) => str.Replace(" ", "");

        public static string RemoveTab(string str) => str.Replace("\t", "");

        public static string RemoveEnter(string str) => str.Replace("\n\n", "\n").Replace("\n\n", "\n").Replace("\n\n", "\n");

        public static string SetShapeToBR(string str) => str.Replace("#", "\n").Replace("#", "\n");

        public static string SetFullWidthCharToHalfWidthChar(string str) => str.Replace("＞", ">").Replace("＜", "<").Replace("＃", "#");
    }
}