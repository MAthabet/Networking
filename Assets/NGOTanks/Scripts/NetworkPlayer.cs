using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine.UI;
using System;

namespace NGOTanks
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] Transform cannonPivot;
        [SerializeField] Transform bulletHole;
        [SerializeField] TextMeshProUGUI text_PlayerName;
        [SerializeField] Transform PlayerHealth;
        [SerializeField] Transform HUDRoot;
        [SerializeField] bullet bulletPrefab;
        [SerializeField] float MaxHealth = 100;
          
        Transform camera;
        Rigidbody rb;
        bool isDead;

        //NetworkVariable<FixedString64Bytes> pData = new NetworkVariable<FixedString64Bytes>();
        NetworkVariable<PlayerData> pData = new NetworkVariable<PlayerData>();
        NetworkVariable<float> pHealth = new NetworkVariable<float>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            pData.OnValueChanged += OnPlayerDataChanged;
            pHealth.OnValueChanged += OnPlayerHealthChanged;


            if (IsLocalPlayer)
            {
                UpdatePlayerDataServerRpc(pData.Value);
            }
            else
            {
                initPlayerNameUI();
            }
            //manually force calling the function cuz client go has not subscribed to delegate before server changing pHealth
            OnPlayerHealthChanged(0, pHealth.Value);
            if (IsServer)
            {
                pHealth.Value = MaxHealth;
            }

            NetworkingManager.Singleton.addPlayer(this);
        }

        private void OnPlayerHealthChanged(float previousValue, float newValue)
        {
            PlayerHealth.localScale = new Vector3(newValue / MaxHealth, 1, 1);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            camera = Camera.main.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if(isDead)
            {
                return;
            }
            if(IsLocalPlayer)
            {
                float moveHorizontal = Input.GetAxis("Horizontal");
                float moveVertical = Input.GetAxis("Vertical");
                Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
                rb.MovePosition(rb.position + movement * Time.deltaTime);
                if(Input.GetKeyDown(KeyCode.D))
                {
                    cannonPivot.Rotate(Vector3.up, 45);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    cannonPivot.Rotate(Vector3.up, -45);
                }
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ShootServerRpc();
                    
                }
            }

            HUDRoot.LookAt(camera);
        }


        private void OnPlayerDataChanged(PlayerData previousValue, PlayerData newValue)
        {
            initPlayerNameUI();
        }

        [ServerRpc]
        void UpdatePlayerDataServerRpc(PlayerData value)
        {
            pData.Value = value;
        }

        [ServerRpc]
        void ShootServerRpc()
        {
            bullet b = Instantiate(bulletPrefab, bulletHole.position, bulletHole.rotation);
            b.init(OwnerClientId);
            ShootClientRpc(bulletHole.position, bulletHole.rotation);
        }
        [ClientRpc]
        void ShootClientRpc(Vector3 pos, Quaternion rot)
        {
            if (!IsHost)
            {
                bullet b = Instantiate(bulletPrefab, pos, rot);
                b.init(OwnerClientId);
            }
        }
        [ClientRpc]
        void killPlayerClientRpc(ulong killerID)
        {
            isDead = true;
            Debug.Log($"Player:{pData.Value} killled by {NetworkingManager.Singleton.getPlayer(killerID).pData.Value}");
        }
        void initPlayerNameUI()
        {
            text_PlayerName.text = pData.Value.ToString();
            
        }
        public void TakeDamage(float damage, ulong bulletOwnerID)
        {
            if(!IsServer)
            {
                Debug.LogWarning("TakeDamage Should not called in client");
                return;
            }
            if (isDead) return;
            pHealth.Value -= damage;
            if (pHealth.Value <= 0)
            {
                killPlayerClientRpc(NetworkingManager.Singleton.getPlayer(bulletOwnerID).OwnerClientId);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            pData.OnValueChanged -= OnPlayerDataChanged;
            pHealth.OnValueChanged -= OnPlayerHealthChanged;

            NetworkingManager.Singleton.removePlayer(OwnerClientId);
        }
    }
}
