using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Language
{
    public class GameLanguage : MonoBehaviour
    {
        public Text jumptoLabel;
        public Text jumpto;
        public Text charter;
    
        // Start is called before the first frame update
        void Start()
        {
            jumptoLabel.text = "跳转至指定时间（单位与maker相同）";
            jumpto.text = "确定";
            charter.text = "谱师";
        }
    }
}
