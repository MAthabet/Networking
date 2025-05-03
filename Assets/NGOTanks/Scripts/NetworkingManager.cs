using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace NGOTanks
{
    public class NetworkingManager : NetworkManager
    {
        static NetworkingManager singleton;
        public static new NetworkingManager Singleton => singleton;

        public const string GameSceneName = "Gameplay";
        public const string LobbySceneName = "Lobby";
        public const string MainMenuSceneName = "MainMenu";

        private PlayerData localPlayerData;
        public PlayerData LocalPlayerData
        {
            get => localPlayerData;
        }

        [SerializeField] private NetworkObject playerPrefab;
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
            SceneManager.LoadScene(LobbySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
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


        public void addPlayer(NetworkPlayer player)
        {
            if (netPlayers.ContainsKey(player.OwnerClientId)) return;
            netPlayers.Add(player.OwnerClientId, player);
            if (!player.IsLocalPlayer)
            {
                UIManager.Singleton.AddPlayer(player.OwnerClientId, player.GetName());
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

    }
}
