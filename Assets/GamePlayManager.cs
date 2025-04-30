using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace NGOTanks
{
    public class GamePlayManager : MonoBehaviour
    {
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] NetworkObject playerPrefab;
        int currentIndx = 0;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            NetworkingManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
            currentIndx = 0;
            if(NetworkingManager.Singleton.IsHost)
            {
                spawnNextPlayer(NetworkingManager.Singleton.LocalClientId);
            }
        }

        private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (sceneName == NetworkingManager.GameSceneName)
            {
                spawnNextPlayer(clientId);
            }
        }
        void spawnNextPlayer(ulong clientId)
        {
            if (NetworkingManager.Singleton.IsServer)
            {
                NetworkObject netPlayer = Instantiate(playerPrefab, spawnPoints[currentIndx].position, Quaternion.identity);
                currentIndx++;
                currentIndx %= spawnPoints.Count;

                netPlayer.SpawnAsPlayerObject(clientId);
            }

        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnDestroy()
        {
            if(NetworkingManager.Singleton != null)
            {
                NetworkingManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
            }
        }
    }
}
