using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

namespace NGOTanks
{

    public class GameSettings : NetworkBehaviour
    {
        public static GameSettings Singelton;

        NetworkVariable<bool> IsFriendlyFire = new NetworkVariable<bool>(false);
        public override void OnNetworkSpawn()
        {
            if(Singelton == null)
            {
                Singelton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            base.OnNetworkSpawn();
            IsFriendlyFire.OnValueChanged += OnFriendlyFireChanged;
        }

        private void OnFriendlyFireChanged(bool previousValue, bool newValue)
        {
            UIManager.Singleton.UpdatefriendlyFireClientIndecator(newValue);
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
