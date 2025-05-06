using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

namespace NGOTanks
{
    public class UIManager : MonoBehaviour
    {
        #region variables
        static UIManager singleton;
        public static UIManager Singleton => singleton;

        [SerializeField] Color blueTeamColor = Color.blue;
        [SerializeField] Color brownTeamColor = Color.green;
        Color neutralTeamColor;

        [SerializeField] private TMP_InputField IF_PlayerName;
        [SerializeField] private Canvas MainMenuCanva;
        [SerializeField] private Canvas LobbyCanva;
        [SerializeField] private Canvas InGameUI;
        [SerializeField] private TMP_Text GameLog;
        [SerializeField] private Canvas PopUp;
        [SerializeField] private TMP_Text PopUpMsg;
        [SerializeField] private TMP_Dropdown DD_Team;
        [SerializeField] private TMP_Dropdown DD_Class;
        [SerializeField] private TMP_Text PlayerNameText;
        [SerializeField] private GameObject HostPanel;
        [SerializeField] private GameObject ClientPanel;
        [SerializeField] private Toggle friendlyFire;
        [SerializeField] private Toggle friendlyFireClientIndecator;
        [SerializeField] private Toggle Ready;
        [SerializeField] private List<TMP_Text> PlayersInLobbyText;
        [SerializeField] private List<Button> StartBtns;
        [SerializeField] private RawImage ArenaImg;
        [SerializeField] private List<Texture> ArenaImgs;
        private int currentIndx = 0;

        private Dictionary<ulong, TMP_Text> textMap = new Dictionary<ulong, TMP_Text>();
        Coroutine currentPopupCoroutine;
        #endregion
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
            neutralTeamColor = PlayersInLobbyText[0].color;

            InitializeUI();
            MainMenuCanva.enabled = true;
            HostPanel.SetActive(false);
            LobbyCanva.enabled = false;
            InGameUI.enabled = false;

            foreach (TMP_Text text in PlayersInLobbyText)
            {
                text.text = "";
            }

            ChangeMainMenuButtonsInteraction(false);
        }

        public void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (sceneName.Contains(NetworkingManager.GameSceneIdentifer))
            {
                DisableMenus();
                ClearLog();
                InGameUI.enabled = true;
            }
            else
                InGameUI.enabled = false;
        }

        public void DisableMenus()
        {
            MainMenuCanva.enabled = false;
            HostPanel.SetActive(false);
            LobbyCanva.enabled = false;
        }
        public void InitializeInGameUI()
        {
            GameLog.text = "";
        }

        public void InitializeUI()
        {
            List<string> teamNames = System.Enum.GetNames(typeof(Team)).ToList<string>();
            teamNames[0] = "Select Team";
            DD_Team.ClearOptions();
            DD_Team.AddOptions(teamNames);

            List<string> classNames = System.Enum.GetNames(typeof(Class)).ToList<string>();
            classNames[0] = "Select Class";
            DD_Class.ClearOptions();
            DD_Class.AddOptions(classNames);

            foreach (TMP_Text text in PlayersInLobbyText)
            {
                text.text = "";
            }

            PopUp.enabled = false;
            ArenaImg.texture = ArenaImgs[currentIndx];
        }
        private void ChangeMainMenuButtonsInteraction(bool isInteractable)
        {
            foreach (Button btn in StartBtns)
            {
                btn.interactable = isInteractable;
            }
        }

        #region events
        public void OnStartServerClicked()
        {
            NetworkingManager.Singleton.StartServer();

            NetworkingManager.Singleton.SceneManager.OnLoadComplete += UIManager.Singleton.OnSceneLoadComplete;
        }

        public void OnStartHostClicked()
        {
            GetName();
            NetworkingManager.Singleton.StartHost();

            NetworkingManager.Singleton.SceneManager.OnLoadComplete += UIManager.Singleton.OnSceneLoadComplete;
            LobbyStarted(true);

        }

        public void OnStartClientClicked()
        {
            GetName();
            NetworkingManager.Singleton.StartClient();

            NetworkingManager.Singleton.SceneManager.OnLoadComplete += UIManager.Singleton.OnSceneLoadComplete;
            LobbyStarted(false);
        }

        public void OnStartGameClicked()
        {
            if(NetworkingManager.Singleton.IsAllPlayerReady() == false)
            {
                SendMsg("not all players are ready");
                Debug.Log("not all players are ready");
                return;
            }
            else { Debug.Log("all players are ready"); }
            if (NetworkingManager.Singleton.IsHost)
            {
                NetworkingManager.Singleton.LoadGameScene(currentIndx);
            }
        }
        public void OnIFValueChanged()
        {
            if (IF_PlayerName.text == null || IF_PlayerName.text == "")
                ChangeMainMenuButtonsInteraction(false);
            else
                ChangeMainMenuButtonsInteraction(true);
        }

        public void OnDD_TeamValueChanged()
        {
            NetworkingManager.Singleton.UpdatePlayerTeam(NetworkingManager.Singleton.LocalClientId,(Team) DD_Team.value); 
        }
        public void OnDD_ClassValueChanged()
        {
            NetworkingManager.Singleton.UpdatePlayerClass(NetworkingManager.Singleton.LocalClientId,(Class) DD_Class.value);
        }
        public void OnReadyClicked()
        {
            if (Ready.isOn)
            {
                NetworkingManager.Singleton.UpdatePlayerReadyState(NetworkingManager.Singleton.LocalClientId, true);
            }
            else
            {
                NetworkingManager.Singleton.UpdatePlayerReadyState(NetworkingManager.Singleton.LocalClientId, false);
            }
        }
        public void OnFriendlyFireClicked()
        {
            if (friendlyFire.isOn)
            {
                GameSettings.Singleton.SetFriendlyFireServerRpc(true);
            }
            else
            {
                GameSettings.Singleton.SetFriendlyFireServerRpc(false);
            }
        }
        #endregion

        public void ChangeArenaIndx(int increment)
        {
            currentIndx = (currentIndx + increment + ArenaImgs.Count) % ArenaImgs.Count;
            if(NetworkingManager.Singleton.IsHost)
            {
                ChangeArenaImg(currentIndx);
                GameSettings.Singleton.UpdateIndx(currentIndx);
            }
        }
        public void ChangeArenaImg(int indx)
        {
            Debug.Log("Changed arenaImg");
            ArenaImg.texture = ArenaImgs[indx];
        }
        public void GetName()
        {
            NetworkingManager.Singleton.UpdatePlayerName(IF_PlayerName.text);
        }

        public void UpdatefriendlyFireClientIndecator(bool on)
        {
            if (on)
            {
                friendlyFireClientIndecator.isOn = true;
            }
            else
            {
                friendlyFireClientIndecator.isOn = false;
            }
        }

        public void LobbyStarted(bool isHost)
        {
            PlayerNameText.text = IF_PlayerName.text;
            MainMenuCanva.enabled = false;
            LobbyCanva.enabled = true;

            if (isHost)
            {
                HostPanel.SetActive(true);
                ClientPanel.SetActive(false);
            }
            else
            {
                HostPanel.SetActive(false);
                ClientPanel.SetActive(true);
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
                        UpdatePlayerTeam(id, NetworkingManager.Singleton.GetPlayer(id).GetTeam());
                        UpdatePlayerClass(id, NetworkingManager.Singleton.GetPlayer(id).GetClass());
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
            else if(NetworkingManager.Singleton.GetPlayer(id).IsLocalPlayer)
            {
                PlayerNameText.text = name;
            }
        }
        public void UpdatePlayerTeam(ulong id, Team team)
        {
            if (textMap.ContainsKey(id))
            {
                switch (team)
                {
                    case Team.None:
                        textMap[id].color = neutralTeamColor;
                        break;
                    case Team.Blue:
                        textMap[id].color = blueTeamColor;
                        break;
                    case Team.Brown:
                        textMap[id].color = brownTeamColor;
                        break;
                    default:
                        break;
                }
            }
        }
        public void UpdatePlayerClass(ulong id, Class playerClass)
        {
            if (textMap.ContainsKey(id))
            {
                string[] parts = textMap[id].text.Split('(');
                string name = parts[0];
                if (playerClass != Class.None)
                {
                    string className = playerClass.ToString();
                    textMap[id].text = name + "(" + playerClass.ToString() + ")";
                }
                else
                {
                    textMap[id].text = name;
                }
            }
            

        }
        public void LogKill(string killerName, string deadName)
        {
            GameLog.text += "\n" + killerName + " Killed " + deadName;
        }
        public void ClearLog()
        {
            GameLog.text = "";
        }

        public void SendMsg(string msg, float timer = 1.5f)
        {
            
            PopUp.enabled = true;
            if (currentPopupCoroutine != null)
            {
                StopCoroutine(currentPopupCoroutine);
                PopUpMsg.text += "\n";
                PopUpMsg.text += msg;
            }
            else
                PopUpMsg.text = msg;
            
            StartCoroutine(HidePopUp(timer));
        }
        public void SetUpPopUpColor(Color color)
        {
            PopUpMsg.color = color;
        }
        IEnumerator HidePopUp(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            PopUpMsg.text = "";
            PopUp.enabled = false;
            currentPopupCoroutine = null;
        }
    }
}
