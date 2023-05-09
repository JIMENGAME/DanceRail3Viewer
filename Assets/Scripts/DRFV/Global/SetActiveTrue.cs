using UnityEngine;

namespace DRFV.Global
{
    public class SetActiveTrue : MonoBehaviour
    {
        public GameObject[] gos;
        // Start is called before the first frame update
        void Start()
        {
            foreach (GameObject go in gos)
            {
                go.SetActive(true);
            }
        } 
    }
}
