using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace NGOTanks
{
    public class UIManager : MonoBehaviour
    {
        static UIManager singleton;
        public static UIManager Singleton => singleton;

        [SerializeField] Color blueTeamColor = Color.blue;
        [SerializeField] Color brownTeamColor = Color.green;

        [SerializeField] private TMP_InputField IF_PlayerName;
        [SerializeField] private Canvas MainMenuCanva;
        [SerializeField] private Canvas LobbyCanva;
        [SerializeField] private TMP_Dropdown DD_Team;
        [SerializeField] private TMP_Dropdown DD_Class;
        [SerializeField] private TMP_Text PlayerNameText;
        [SerializeField] private GameObject HostPanel;
        [SerializeField] private Toggle friendlyFire;
        [SerializeField] private List<TMP_Text> PlayersInLobbyText = new List<TMP_Text>();

        private Dictionary<ulong, TMP_Text> textMap = new Dictionary<ulong, TMP_Text>();
        private void Awake()
        {
            if (singleton == null)
            {
                singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        private void Start()
        {
            InitializeUI();
            MainMenuCanva.enabled = true;
            HostPanel.SetActive(false);
            LobbyCanva.enabled = false;

            foreach (TMP_Text text in PlayersInLobbyText)
            {
                text.text = "";
            }
        }

        private void DisableUi()
        {
            MainMenuCanva.enabled = false;
            HostPanel.SetActive(false);
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
            LobbyStarted(true);

        }

        public void OnStartClientClicked()
        {
            GetName();
            NetworkingManager.Singleton.StartClient();
            LobbyStarted(false);
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

        public void LobbyStarted(bool isHost)
        {
            PlayerNameText.text = IF_PlayerName.text;
            MainMenuCanva.enabled = false;
            LobbyCanva.enabled = true;

            if (isHost)
            {
                HostPanel.SetActive(true);
            }
            else
            {
                HostPanel.SetActive(false);
            }
        }

        public void AddPlayer(ulong id, string name)
        {
            if (textMap.ContainsKey(id))
            {
                textMap[id].text = name;
            }
            else
            {
                foreach (TMP_Text text in PlayersInLobbyText)
                {
                    if (text.text == "")
                    {
                        textMap.Add(id, text);
                        text.text = name;
                        break;
                    }
                }
            }
        }
        public void RemovePlayer(ulong id)
        {
            if (textMap.ContainsKey(id))
            {
                textMap[id].text = "";
                textMap.Remove(id);
            }
        }

        public void UpdatePlayerName(ulong id, string name)
        {
            if (textMap.ContainsKey(id))
            {
                textMap[id].text = name;
            }
            else
            {
                Debug.LogWarning("Player: "  + name + " Does Not exist");
            }
        }

        public void OnExitGameClicked()
        {
            Application.Quit();
        }
    }
}
