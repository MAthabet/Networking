using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace NGOTanks
{
    [System.Serializable]
    public class SpawnPoint
    {
        public Team playerTeam;
        public Class playerClass;
        public Transform spawnTransform;
        public GameObject TankPrefab;
    }
    public class GamePlayManager : MonoBehaviour
    {
        static NetworkingManager singleton;
        public static NetworkingManager Singleton => singleton;


        [SerializeField] List<SpawnPoint> spawnPoints;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            NetworkingManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;

            if(NetworkingManager.Singleton.IsHost)
            {
                spawnPlayer(NetworkingManager.Singleton.LocalClientId);
                
            }
        }

        private void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (sceneName == NetworkingManager.GameSceneName)
            {
                spawnPlayer(clientId);
            }
        }
        void spawnPlayer(ulong clientId)
        {
            if (NetworkingManager.Singleton.IsServer)
            {
                NetworkPlayer player = NetworkingManager.Singleton.GetPlayer(clientId);
                GameObject tankPrefab;
                Transform pos = getSpawnPointAndTankprefab(player.GetTeam(), player.GetClass(),out tankPrefab);

                GameObject instance = Instantiate(tankPrefab, pos.position, Quaternion.identity);
                NetworkObject obj = instance.GetComponent<NetworkObject>();
                obj.SpawnAsPlayerObject(clientId);
                player.ChangeTankID(obj.NetworkObjectId);
            }

        }
        Transform getSpawnPointAndTankprefab(Team pTeam, Class pClass, out GameObject prefab)
        {
            foreach (SpawnPoint spawnPoint in spawnPoints)
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
