using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace NGOTanks
{
    
    public class GamePlayManager : MonoBehaviour
    {
        static GamePlayManager singleton;
        public static GamePlayManager Singleton => singleton;

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
            NetworkingManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
        }

        private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (sceneName.Contains(NetworkingManager.GameSceneIdentifer))
            {
                SpawnTank(clientId);
            }
        }
        void SpawnTank(ulong clientId)
        {
            if (NetworkingManager.Singleton.IsServer)
            {
                NetworkPlayer player = NetworkingManager.Singleton.GetPlayer(clientId);
                
                Transform pos = getSpawnPointAndTankprefab(player.GetTeam(), player.GetClass(), out GameObject tankPrefab);
                GameObject instance = Instantiate(tankPrefab, pos.position, Quaternion.identity);
                NetworkObject obj = instance.GetComponent<NetworkObject>();
                obj.SpawnAsPlayerObject(clientId);
                player.ChangeTankID(obj.NetworkObjectId);

            }

        }
        Transform getSpawnPointAndTankprefab(Team pTeam, Class pClass, out GameObject prefab)
        {
            foreach (SpawnPoint spawnPoint in LevelManager.Singleton.spawnPoints)
            {
                if (spawnPoint.playerTeam == pTeam && spawnPoint.playerClass == pClass)
                {
                    prefab = spawnPoint.TankPrefab;
                    Debug.Log(spawnPoint.spawnTransform);
                    return spawnPoint.spawnTransform;
                }
            }
            Debug.LogError("No spawn point found for class: " + pClass + " and team: " + pTeam);
            prefab = null;
            return null;
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
