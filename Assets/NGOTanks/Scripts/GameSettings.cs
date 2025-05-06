using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

namespace NGOTanks
{

    public class GameSettings : NetworkBehaviour
    {
        public static GameSettings Singleton;

        NetworkVariable<bool> IsFriendlyFire = new NetworkVariable<bool>(false);
        NetworkVariable<int> ArenaIndex = new NetworkVariable<int>();
        public override void OnNetworkSpawn()
        {
            if (Singleton == null)
            {
                Singleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            base.OnNetworkSpawn();
            IsFriendlyFire.OnValueChanged += OnFriendlyFireChanged;
            ArenaIndex.OnValueChanged += OnArenaIndexChanged;
        }

        private void OnArenaIndexChanged(int previousValue, int newValue)
        {
            if (!NetworkingManager.Singleton.IsHost)
            {
                UIManager.Singleton.ChangeArenaImg(newValue);
                Debug.Log("Arena index changed to: " + newValue);
            }
            else
                Debug.Log("IsHOst");
        }

        private void OnFriendlyFireChanged(bool previousValue, bool newValue)
        {
            UIManager.Singleton.UpdatefriendlyFireClientIndecator(newValue);
        }
        public void UpdateIndx(int newIndx)
        {
            ArenaIndex.Value = newIndx;
        }
        [ServerRpc]
        public void SetFriendlyFireServerRpc(bool value)
        {
            IsFriendlyFire.Value = value;
        }
        public bool IsFriendlyFireOn()
        {
            return IsFriendlyFire.Value;
        }
    }
}
