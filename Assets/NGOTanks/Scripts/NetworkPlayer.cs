using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace NGOTanks
{
    public class NetworkPlayer : NetworkBehaviour
    {
        Tank tank;
        NetworkVariable<PlayerData> pData = new NetworkVariable<PlayerData>(new PlayerData("", Team.None, Class.None));
        NetworkVariable<float> pHealth = new NetworkVariable<float>(0);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            pData.OnValueChanged += OnPlayerDataChanged;
            pHealth.OnValueChanged += OnPlayerHealthChanged;
            if(IsLocalPlayer)
                ChangeName(NetworkingManager.Singleton.LocalPlayerData.playerName.ToString());
            NetworkingManager.Singleton.addPlayer(this);
        }
        private void OnPlayerHealthChanged(float previousValue, float newValue)
        {
            if (tank)
                tank.UpdateHealth(newValue);
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
        void ShootServerRpc()
        {
            bullet b = tank.Fire();
            b.init(OwnerClientId);
            ShootClientRpc();
        }
        #endregion

        #region ClientRpc
        [ClientRpc]
        void ShootClientRpc()
        {
            if (!IsHost)
            {
                bullet b = tank.Fire();
                b.init(OwnerClientId);
            }
        }
        [ClientRpc]
        void killPlayerClientRpc(ulong killerID)
        {
            tank.Kill();
            if(IsLocalPlayer)
                tank.gameObject.GetComponent<PlayerInput>().Disable();
            Debug.Log($"Player:{pData.Value} killled by {NetworkingManager.Singleton.GetPlayer(killerID).pData.Value}");
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

        public void TakeDamage(float damage, ulong bulletOwnerID)
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
                killPlayerClientRpc(NetworkingManager.Singleton.GetPlayer(bulletOwnerID).OwnerClientId);
            }
        }
        public void spawnTank(Tank tankPrefab)
        {
            if (tank != null)
            {
                Debug.Log("there is already tank");
                Destroy(tank.gameObject);
            }
            tank = Instantiate(tankPrefab, transform.position, Quaternion.identity);
            if (IsLocalPlayer)
            {
                IA_Tank inputActions = new IA_Tank();
                inputActions.Enable();
                inputActions.Control.Fire.performed += ctx => ShootServerRpc();
                tank.TankInit();
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
        public string GetName()
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
