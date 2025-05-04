using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;
using NUnit.Framework.Internal.Commands;

namespace NGOTanks
{
    public class NetworkPlayer : NetworkBehaviour
    {
        Tank tank;

        NetworkVariable<ulong> tankID = new NetworkVariable<ulong>();
        NetworkVariable<PlayerData> pData = new NetworkVariable<PlayerData>(new PlayerData("", Team.None, Class.None));
        NetworkVariable<float> pHealth = new NetworkVariable<float>(0);
        NetworkVariable<bool> pIsReady = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            tankID.OnValueChanged += OnTankIDChanged;
            pData.OnValueChanged += OnPlayerDataChanged;
            pHealth.OnValueChanged += OnPlayerHealthChanged;
            pIsReady.OnValueChanged += OnPlayerReadyChanged;

            if (IsLocalPlayer)
            {
                if (IsHost)
                    pIsReady.Value = true;
                ChangeName(NetworkingManager.Singleton.LocalPlayerData.playerName.ToString());
                DontDestroyOnLoad(gameObject);
            }
            NetworkingManager.Singleton.AddPlayer(this);
        }

        private void OnTankIDChanged(ulong previousValue, ulong newValue)
        {
            NetworkObject tankObject = NetworkingManager.Singleton.SpawnManager.SpawnedObjects[tankID.Value];
            tank = tankObject.GetComponent<Tank>();

            if (IsLocalPlayer)
                tank.TankInit(this);
            else
                tank.SetOwnerId(OwnerClientId);
        }

        private void OnPlayerReadyChanged(bool previousValue, bool newValue)
        {

        }

        private void OnPlayerHealthChanged(float previousValue, float newValue)
        {
            if (tank)
                tank.UpdateHealth(newValue);
            else
                Debug.LogWarning("Tank is null");
        }
        private void OnPlayerDataChanged(PlayerData previousValue, PlayerData newValue)
        {
            if (IsLocalPlayer) return;
            if(previousValue.playerName != newValue.playerName)
                UpdateOtherPlayersNameUI();
            if (previousValue.playerTeam != newValue.playerTeam)
                UpdateOtherPlayersTeamUI();
            if (previousValue.playerClass != newValue.playerClass)
                UpdateOtherPlayersClassUI();
        }

        #region ServerRpc
        
        
        [ServerRpc]
        void UpdatePlayerDataServerRpc(PlayerData value)
        {
            pData.Value = value;
        }

        [ServerRpc]
        public void ShootServerRpc()
        {
            tank.Fire();
            ShootClientRpc();
        }
        [ServerRpc]
        public void InitpHealthServerRpc(float health)
        {
            pHealth.Value = health;
        }
        //TODO: player request spawn to server so late join to be an option
        #endregion

        #region ClientRpc
        [ClientRpc]
        void ShootClientRpc()
        {
            if (!IsHost)
            {
                tank.Fire();
            }
        }
        [ClientRpc]
        void killPlayerClientRpc(ulong killerID)
        {
            tank.Kill();
            Debug.Log($"Player:{pData.Value.playerName} killled by {NetworkingManager.Singleton.GetPlayer(killerID).pData.Value.playerName}");
        }
        #endregion
        void UpdateOtherPlayersNameUI()
        {
            UIManager.Singleton.UpdatePlayerName(OwnerClientId, pData.Value.playerName.ToString());
        }
        void UpdateOtherPlayersTeamUI()
        {
            UIManager.Singleton.UpdatePlayerTeam(OwnerClientId, pData.Value.playerTeam);
        }
        void UpdateOtherPlayersClassUI()
        {
            UIManager.Singleton.UpdatePlayerClass(OwnerClientId, pData.Value.playerClass);
        }

        public void TakeDamage(float damage, ulong attackerID)
        {
            if(!IsServer)
            {
                Debug.LogWarning("TakeDamage Should not called in client");
                return;
            }
            if (tank.isDead) return;
            pHealth.Value -= damage;
            if (pHealth.Value <= 0)
            {
                killPlayerClientRpc(NetworkingManager.Singleton.GetPlayer(attackerID).OwnerClientId);
            }
        }        
        public void ChangeTeam(Team newTeam)
        {
            PlayerData p = pData.Value;
            p.playerTeam = newTeam;
            UpdatePlayerDataServerRpc(p);
        }
        public void ChangeClass(Class newClass)
        {
            PlayerData p = pData.Value;
            p.playerClass = newClass;
            UpdatePlayerDataServerRpc(p);
        }
        public void ChangeName(string name)
        {
            PlayerData p = pData.Value;
            p.playerName = name;
            UpdatePlayerDataServerRpc(p);
        }
        public void ChangeReadyState(bool isReady)
        {
            pIsReady.Value = isReady;
        }
        public void ChangeTankID(ulong id)
        {
            tankID.Value = id;
        }
        public string GetPlayerName()
        {
            return pData.Value.playerName.ToString();
        }
        public Team GetTeam()
        {
            return pData.Value.playerTeam;
        }
        public Class GetClass()
        {
            return pData.Value.playerClass;
        }
        public bool IsReady()
        {
            return pIsReady.Value;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            pData.OnValueChanged -= OnPlayerDataChanged;
            pHealth.OnValueChanged -= OnPlayerHealthChanged;
            Destroy(tank.gameObject);
            NetworkingManager.Singleton.RemovePlayer(OwnerClientId);
        }
    }
}
