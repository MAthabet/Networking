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

        NetworkVariable<FixedString64Bytes> pName = new NetworkVariable<FixedString64Bytes>();
        NetworkVariable<float> pHealth = new NetworkVariable<float>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            pName.OnValueChanged += OnPlayerNameChanged;
            pHealth.OnValueChanged += OnPlayerHealthChanged;

            if (IsLocalPlayer)
            {
                UpdateNameServerRpc(NetworkingManager.Singleton.localPlayerName);
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

        private void OnPlayerNameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            initPlayerNameUI();
            OnPlayerHealthChanged(0, pHealth.Value);
        }

        [ServerRpc]
        void UpdateNameServerRpc(FixedString64Bytes value)
        {
            pName.Value = value;
        }

        [ServerRpc]
        void ShootServerRpc()
        {
            bullet b = Instantiate(bulletPrefab, bulletHole.position, bulletHole.rotation);
            ShootClientRpc(bulletHole.position, bulletHole.rotation);
        }
        [ClientRpc]
        void ShootClientRpc(Vector3 pos, Quaternion rot)
        {
            if (!IsHost)
            {
                bullet b = Instantiate(bulletPrefab, pos, rot);
            }
        }
        [ClientRpc]
        void killPlayerClientRpc()
        {
            isDead = true;
        }
        void initPlayerNameUI()
        {
            text_PlayerName.text = pName.Value.ToString();
        }
        public void TakeDamage(float damage)
        {
            if(!IsServer)
            {
                Debug.LogWarning("TakeDamage Should not called in client");
                return;
            }
            pHealth.Value -= damage;
            if (pHealth.Value <= 0)
            {
                killPlayerClientRpc();
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            pName.OnValueChanged -= OnPlayerNameChanged;
            pHealth.OnValueChanged -= OnPlayerHealthChanged;

        }
    }
}
