using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace NGOTanks
{
    public class NetworkPlayer : NetworkBehaviour
    {
        Tank tank;
        NetworkVariable<PlayerData> pData = new NetworkVariable<PlayerData>();
        NetworkVariable<float> pHealth = new NetworkVariable<float>(0);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            pData.OnValueChanged += OnPlayerDataChanged;
            pHealth.OnValueChanged += OnPlayerHealthChanged;

            NetworkingManager.Singleton.addPlayer(this);
            Debug.Log("spawned");
        }
        private void OnPlayerHealthChanged(float previousValue, float newValue)
        {
            if (tank)
                tank.UpdateHealth(newValue);
        }
        private void OnPlayerDataChanged(PlayerData previousValue, PlayerData newValue)
        {
            initPlayerNameUI();
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
            Debug.Log($"Player:{pData.Value} killled by {NetworkingManager.Singleton.getPlayer(killerID).pData.Value}");
        }
        #endregion
        void initPlayerNameUI()
        {

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
                killPlayerClientRpc(NetworkingManager.Singleton.getPlayer(bulletOwnerID).OwnerClientId);
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
        
        public void changeTeam(Team newTeam)
        {
            PlayerData p = pData.Value;
            p.playerTeam = newTeam;
            pData.Value = p;
        }
        public void changeClass(Class newClass)
        {
            PlayerData p = pData.Value;
            p.playerClass = newClass;
            pData.Value = p;
        }
        public string getName()
        {
            return pData.Value.playerName.ToString();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            pData.OnValueChanged -= OnPlayerDataChanged;
            pHealth.OnValueChanged -= OnPlayerHealthChanged;
            Destroy(tank.gameObject);
            NetworkingManager.Singleton.removePlayer(OwnerClientId);
        }
    }
}
