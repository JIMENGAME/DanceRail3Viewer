using DG.Tweening;
using DRFV.Login;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DRFV.Multiplayer
{
    public class MultiplayerManager : MonoBehaviour
    {
        private bool atSelect;

        public RectTransform mainPanel;
        
        void Start()
        {
            AccountInfo.Instance.UpdateAccountPanel();
            MoveToSelect();
        }

        private Vector2 selectPos = new(0, 0), itemPos = new(0, 0);

        private void MoveToSelect()
        {
            atSelect = true;
            mainPanel.DOAnchorPos(selectPos, 0.5f).SetEase(Ease.OutExpo);
        }

        private void MoveToItem()
        {
            atSelect = false;
            mainPanel.DOAnchorPos(itemPos, 0.5f).SetEase(Ease.OutExpo);
        }

        public void Back()
        {
            if (atSelect) SceneManager.LoadScene("main");
            else MoveToSelect();
        }
    }
}