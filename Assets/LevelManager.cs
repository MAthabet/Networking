using System.Collections.Generic;
using UnityEngine;

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
    public class LevelManager : MonoBehaviour
    {
        static LevelManager CurrentLvlManager;
        public static LevelManager Singleton => CurrentLvlManager;

        [SerializeField] public List<SpawnPoint> spawnPoints;

        private void Awake()
        {
            if (CurrentLvlManager == null)
            {
                CurrentLvlManager = this;
            }
            else
            {
                Destroy(CurrentLvlManager);
                CurrentLvlManager = this;
            }
        }
    }
}
