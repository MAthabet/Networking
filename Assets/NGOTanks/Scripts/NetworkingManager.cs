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
        public const string MainMenuSceneName = "MainMenu";

        public string localPlayerName;

        [SerializeField] private NetworkObject playerPrefab;
        private Dictionary<ulong, NetworkPlayer> netPlayers;


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
            SceneManager.LoadScene(GameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SpawnPlayer(ulong clientId)
        {
            NetworkObject netPlayer = Instantiate(playerPrefab);
            netPlayer.SpawnAsPlayerObject(clientId);
        }
        public void UpdatePlayerName(string playerName)
        {
            localPlayerName = playerName;
        }

        public void addPlayer(NetworkPlayer player)
        {
            if(netPlayers == null)
                netPlayers = new Dictionary<ulong, NetworkPlayer> ();
            if (netPlayers.ContainsKey(player.OwnerClientId)) return;
            netPlayers.Add(player.OwnerClientId, player);
        }

        public NetworkPlayer getPlayer(ulong ID)
        {
            if(netPlayers.ContainsKey(ID))
                return netPlayers[ID];
            else
            {
                Debug.Log("cannot find");
                return null;
            }
        }
        public void removePlayer(ulong ID)
        {
            netPlayers.Remove(ID);
        }
        private void OnDestroy()
        {
            OnServerStarted -= HandleServerStarted;
            OnClientConnectedCallback -= HandleClientConnected;

        }
    }
}
