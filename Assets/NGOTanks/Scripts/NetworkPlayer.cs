using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;
using NUnit.Framework.Internal.Commands;

namespace NGOTanks
{
    public class NetworkPlayer : NetworkBehaviour
    {

        NetworkVariable<ulong> tankID = new NetworkVariable<ulong>();
        NetworkVariable<PlayerData> pData = new NetworkVariable<PlayerData>(new PlayerData("", Team.None, Class.None));
        NetworkVariable<float> pHealth = new NetworkVariable<float>(0);
        NetworkVariable<bool> pIsReady = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

        //just referance to save calls to tank
        Tank tank;
        float maxHealth;

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

            tank.SetOwnerId(OwnerClientId);
            tank.UpdateTankName(GetPlayerName());
            if (IsLocalPlayer)
                tank.TankInit(this);
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
        public void UseAbilityServerRpc()
        {
            if(tank.UseAbility())
                UseAbilityClientRpc();
        }
        [ServerRpc]
        public void InitpHealthServerRpc(float _maxHP)
        {
            pHealth.Value = _maxHP;
            maxHealth = _maxHP;
        }
        
        //TODO: player request spawn to server so late join to be an option
        #endregion

        #region ClientRpc
        [ClientRpc]
        void ShootClientRpc()
        {
            if (!IsHost)
                tank.Fire();
        }
        [ClientRpc]
        void UseAbilityClientRpc()
        {
            if (!IsHost)
                tank.UseAbility();
        }
        [ClientRpc]
        void KillPlayerClientRpc(ulong killerID)
        {
            tank.Kill();
            Debug.Log($"Player:{pData.Value.playerName} killled by {NetworkingManager.Singleton.GetPlayer(killerID).pData.Value.playerName}");
            UIManager.Singleton.LogKill(NetworkingManager.Singleton.GetPlayer(killerID).GetPlayerName(), GetPlayerName());
        }
        [ClientRpc]
        void EndGameClientRpc(Team winnerTeam)
        {
            if (NetworkingManager.Singleton.LocalPlayerData.playerTeam == winnerTeam)
                UIManager.Singleton.SetUpPopUpColor(Color.green);
            else
                UIManager.Singleton.SetUpPopUpColor(Color.red);

            UIManager.Singleton.SendMsg($"Team {winnerTeam} Won");
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
            pHealth.Value = Mathf.Clamp(pHealth.Value - damage, 0, maxHealth);
            if (pHealth.Value == 0)
            {
                KillPlayer(attackerID);
            }
        }
        public void Heal(float healAmount)
        {
            if (!IsServer)
            {
                Debug.LogWarning("Heal Should not called in client");
                return;
            }
            if (tank.isDead) return;
            pHealth.Value = Mathf.Clamp(pHealth.Value + healAmount, 0, maxHealth);
        }
        private void KillPlayer(ulong attackerID)
        {
            KillPlayerClientRpc(NetworkingManager.Singleton.GetPlayer(attackerID).OwnerClientId);
            if(NetworkingManager.Singleton.IsAllTeamDead(GetTeam()))
            {
                EndGameClientRpc(GetTeam()==Team.Blue? Team.Brown:Team.Blue);
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
        public float GetCurrentHP()
        {
            return pHealth.Value;
        }
        public bool IsReady()
        {
            return pIsReady.Value;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            Destroy(tank.gameObject);
            pData.OnValueChanged -= OnPlayerDataChanged;
            pHealth.OnValueChanged -= OnPlayerHealthChanged;
            NetworkingManager.Singleton.RemovePlayer(OwnerClientId);
        }
    }
}
