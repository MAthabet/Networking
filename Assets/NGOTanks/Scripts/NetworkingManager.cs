using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Globalization;

namespace NGOTanks
{
    public class NetworkingManager : NetworkManager
    {
        static NetworkingManager singleton;
        public static new NetworkingManager Singleton => singleton;

        //all game scenes must contain this word
        public const string GameSceneIdentifer = "Game";
        public const string LobbySceneName = "Lobby";
        public const string MainMenuSceneName = "MainMenu";

        //looks idiotic but im too lazy to make scriptable object 
        //TODO: scriptable object for game scenes
        public String[] GameScenesNames;

        private PlayerData localPlayerData;
        public PlayerData LocalPlayerData
        {
            get => localPlayerData;
        }

        [SerializeField] private NetworkObject playerPrefab;
        [SerializeField] private GameObject gameSettingsPrefab;

        private Dictionary<ulong, NetworkPlayer> netPlayers = new Dictionary<ulong, NetworkPlayer>();

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
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            OnServerStarted += HandleServerStarted;
            OnClientConnectedCallback += HandleClientConnected;
        }
        private void HandleClientConnected(ulong clientID)
        {

        }

        private void HandleServerStarted()
        {
            SceneManager.OnLoadComplete += OnLobbySceneLoaded;
            SceneManager.LoadScene(LobbySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);            
        }

        private void OnLobbySceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (IsServer && sceneName == LobbySceneName)
            {
                GameObject gameSettings = Instantiate(gameSettingsPrefab);
                gameSettings.GetComponent<NetworkObject>().Spawn();

                SceneManager.OnLoadComplete -= OnLobbySceneLoaded;
            }
        }

        public void SpawnPlayer(ulong clientId)
        {
            Debug.Log("Spawning player for client: " + clientId);
            NetworkObject netPlayer = Instantiate(playerPrefab);
            netPlayer.SpawnAsPlayerObject(clientId);
        }
        public void UpdatePlayerName(string playerName)
        {
            localPlayerData.playerName = playerName;
        }


        public void AddPlayer(NetworkPlayer player)
        {
            if (netPlayers.ContainsKey(player.OwnerClientId)) return;
            netPlayers.Add(player.OwnerClientId, player);
            if (!player.IsLocalPlayer)
            {
                UIManager.Singleton.AddPlayer(player.OwnerClientId, player.GetPlayerName());
            }

        }

        public NetworkPlayer GetPlayer(ulong ID)
        {
            if(netPlayers.ContainsKey(ID))
                return netPlayers[ID];
            else
            {
                Debug.Log("cannot find player: " + ID);
                return null;
            }
        }
        public void RemovePlayer(ulong ID)
        {
            netPlayers.Remove(ID);
            UIManager.Singleton.RemovePlayer(ID);
        }
        public bool IsAllPlayerReady()
        {
            foreach(var player in netPlayers)
            {
                if (!player.Value.IsReady())
                {
                    return false;
                }
            }
            return true;
        }
        public void UpdatePlayerTeam(ulong ID,Team newTeam)
        {
            localPlayerData.playerTeam = newTeam;
            GetPlayer(ID).ChangeTeam(newTeam);
        }
        public void UpdatePlayerClass(ulong ID, Class newClass)
        {
            localPlayerData.playerClass = newClass;
            GetPlayer(ID).ChangeClass(newClass);
        }
        private void OnDestroy()
        {
            OnServerStarted -= HandleServerStarted;
            OnClientConnectedCallback -= HandleClientConnected;
        }

        internal void UpdatePlayerReadyState(ulong localClientId, bool isReady)
        {
            GetPlayer(localClientId).ChangeReadyState(isReady);
        }
        public bool IsAllTeamDead(Team team)
        {
            foreach (var player in netPlayers)
            {
                if (player.Value.GetTeam() == team && player.Value.GetCurrentHP() > 0)
                {
                    return false;
                }
            }
            return true;
        }
        public void LoadGameScene(int indx)
        {
            SceneManager.LoadScene(GameScenesNames[indx], LoadSceneMode.Single);
        }
    }
}
