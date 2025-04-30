using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace RPS
{
    public enum playerMove
    {
        none,
        Rock,
        Paper,
        Scissors
    }

    public class UIManagerInGame : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI localName;
        [SerializeField] private TextMeshProUGUI otherName;

        [SerializeField] private TextMeshProUGUI localScore;
        [SerializeField] private TextMeshProUGUI otherScore;

        [SerializeField] private GameObject playerButtonsUI;
        //list of buttons
        List<Button> playerButtons;
        public GameObject resultPopup;
        private TextMeshProUGUI resultText;

        private void Awake()
        {
            resultText = resultPopup.GetComponentInChildren<TextMeshProUGUI>();
            playerButtons = new List<Button>(playerButtonsUI.GetComponentsInChildren<Button>());

        }
        private void Start()
        {
            resultPopup.SetActive(false);
        }
        public void updateName(bool isLocal, string name)
        {
            if(isLocal)
            {
                localName.text = name;
            }
            else
            {
                otherName.text = name;
            }
        }
        public void updateScore(bool isLocal, int score)
        {
            if (isLocal)
            {
                localScore.text = score.ToString();
            }
            else
            {
                otherScore.text = score.ToString();
            }
        }
        public void ShowResult(GameStatus t)
        {
            switch (t)
            {
                case GameStatus.draw:
                    resultText.text = "Draw!";
                    break;
                case GameStatus.won:
                    resultText.text = "You Win!";
                    break;
                case GameStatus.lost:
                    resultText.text = "You Lose!";
                    break;
                default:
                    return;
            }
            resultPopup.SetActive(true);
            Invoke("ResetGame", 3f);
        }

        private void ResetGame()
        {
            resultPopup.SetActive(false);
            EnableUI();
        }

        public void DisableUI()
        {
            foreach(Button btn in playerButtons)
            {
                btn.interactable = false;
            }
        }
        private void EnableUI()
        {
            foreach (Button btn in playerButtons)
            {
                btn.interactable = true;
            }
        }

        public void updateMove(int move)
        {
            NetworkingManager.Singleton.localPlayer.CmdUpdatePlayerMove((playerMove) move);
        }
    }
}
