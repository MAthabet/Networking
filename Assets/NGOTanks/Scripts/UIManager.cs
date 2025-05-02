using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace NGOTanks
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField IF_PlayerName;
        [SerializeField] private Canvas MainMenuCanva;
        [SerializeField] private Canvas LobbyCanva;
        [SerializeField] private TMP_Dropdown DD_Team;
        [SerializeField] private TMP_Dropdown DD_Class;
        [SerializeField] private TMP_Text PlayerNameText;
        [SerializeField] private GameObject HostPanel;
        [SerializeField] private Toggle friendlyFire;
        [SerializeField] private List<TMP_Text> PlayersInLobbyText = new List<TMP_Text>();

        private void Awake()
        {
            if (FindAnyObjectByType<UIManager>())
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        private void Start()
        {
            InitializeUI();
            MainMenuCanva.enabled = true;
            LobbyCanva.enabled = false;
        }
        public void InitializeUI()
        {
            List<string> teamNames = System.Enum.GetNames(typeof(Team)).ToList<string>();
            DD_Team.ClearOptions();
            DD_Team.AddOptions(teamNames);

            List<string> classNames = System.Enum.GetNames(typeof(Class)).ToList<string>();
            DD_Class.ClearOptions();
            DD_Class.AddOptions(classNames);

            foreach (TMP_Text text in PlayersInLobbyText)
            {
                text.text = "";
            }
        }
        public void OnStartServerClicked()
        {
            NetworkingManager.Singleton.StartServer();
        }

        public void OnStartHostClicked()
        {
            GetName();
            NetworkingManager.Singleton.StartHost();
        }

        public void OnStartClientClicked()
        {
            GetName();
            NetworkingManager.Singleton.StartClient();
        }

        public void OnStartGameClicked()
        {
            if (NetworkingManager.Singleton.IsHost)
            {
                NetworkingManager.Singleton.SceneManager.LoadScene(NetworkingManager.GameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        public void OnDD_TeamValueChanged()
        {

        }
        public void OnDD_ClassValueChanged()
        {

        }
        public void GetName()
        {
            NetworkingManager.Singleton.UpdatePlayerName(IF_PlayerName.text);
        }
    }
}
