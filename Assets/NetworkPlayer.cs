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
        Rigidbody rb;
        [SerializeField] Transform cannonPivot;
        [SerializeField] Transform bulletHole;
        [SerializeField] TextMeshProUGUI text_PlayerName;
        [SerializeField] Transform PlayerHealth;
        [SerializeField] Transform HUDRoot;
        [SerializeField] bullet bulletPrefab;
        [SerializeField] int MaxHealth = 100;
                

        Transform camera;


        NetworkVariable<FixedString64Bytes> pName = new NetworkVariable<FixedString64Bytes>();
        NetworkVariable<int> pHealth = new NetworkVariable<int>();

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
                OnPlayerHealthChanged(0, pHealth.Value);
            }
            if (IsServer)
            {
                pHealth.Value = MaxHealth;
                Debug.Log($"Server: {pHealth.Value}");
            }
        }

        private void OnPlayerHealthChanged(int previousValue, int newValue)
        {
            PlayerHealth.localScale = new Vector3((float)newValue / MaxHealth, 1, 1);
            Debug.Log($"Player Health: {newValue}");
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
            ShootClientRpc();

        }
        [ClientRpc]
        void ShootClientRpc()
        {
            if (!IsHost)
            {
                bullet b = Instantiate(bulletPrefab, bulletHole.position, bulletHole.rotation);
            }
        }
        void initPlayerNameUI()
        {
            text_PlayerName.text = pName.Value.ToString();
        }
    }
}
