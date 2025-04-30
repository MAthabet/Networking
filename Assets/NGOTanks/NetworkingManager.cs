using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

namespace NGOTanks
{
    public class NetworkingManager : NetworkManager
    {
        static NetworkingManager singleton;
        public static NetworkingManager Singleton => singleton;

        public const string GameSceneName = "Gameplay";
        public const string MainMenuSceneName = "MainMenu";

        [SerializeField] private NetworkObject playerPrefab;

        public string localPlayerName;

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
        private void OnDestroy()
        {
            OnServerStarted -= HandleServerStarted;
            OnClientConnectedCallback -= HandleClientConnected;

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
    }
}
