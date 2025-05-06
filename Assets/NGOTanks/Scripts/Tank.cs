using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NGOTanks
{
    public class Tank : MonoBehaviour
    {
        [SerializeField] Transform cannonPivot;
        [SerializeField] Transform bulletHole;
        [SerializeField] TextMeshProUGUI text_PlayerName;
        [SerializeField] Transform PlayerHealth;
        //[SerializeField] Transform HUDRoot;
        [SerializeField] Bullet bulletPrefab;


        //Transform cam;
        Rigidbody rb;
        TankStats stats;
        GameObject abilityPrefab;

        Vector3 movement;
        float rot;
        float NextAbilityTime;
        public bool isDead { get; private set; }
        bool isLocalTank;
        ulong ownerID;

        IA_Tank inputActions;

        private void Awake()
        {
            inputActions = new IA_Tank();
            //cam = Camera.main.transform;
            stats = GetComponent<TankStatsData>().tankStats;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            NextAbilityTime = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            //HUDRoot.LookAt(cam);

            if (isDead)
            {
                return;
            }
            
            if (!isLocalTank) return;

            if(movement != Vector3.zero)
                rb.MovePosition(rb.position + movement * stats.speed * Time.deltaTime);
            if(rot != 0)
                cannonPivot.Rotate(Vector3.up * rot * stats.rotationSpeed * Time.deltaTime);
        }
        public void TankInit(NetworkPlayer player)
        {
            if (!player.IsLocalPlayer)
            {
                Debug.LogWarning("Tank init on not local player");
                return;
            }
            isLocalTank = true;
            rb = GetComponent<Rigidbody>();
            //ownerID = player.OwnerClientId;
            isDead = false;
            SubscripeToInput(player);

            player.InitpHealthServerRpc(stats.MaxHealth);
        }
        public void SetOwnerId(ulong id)
        {
            ownerID = id;
        }
        public ulong GetOwnerID()
        {
            return ownerID;
        }

        #region helper functions
        private void SubscripeToInput(NetworkPlayer player)
        {
            inputActions.Enable();
            inputActions.Control.MoveTank.started += OnMove;
            inputActions.Control.MoveTank.canceled += OnMove;
            inputActions.Control.RotateTorret.started += OnRotateTorret;
            inputActions.Control.RotateTorret.canceled += OnRotateTorret;
            inputActions.Control.Fire.performed +=
                ctx => player.ShootServerRpc();
            inputActions.Control.UseAbility.performed +=
                ctx => player.UseAbilityServerRpc();
        }
        private void UnsubscripeToInput()
        {
            inputActions.Disable();
            inputActions.Control.MoveTank.started -= OnMove;
            inputActions.Control.MoveTank.canceled -= OnMove;
            inputActions.Control.RotateTorret.started -= OnRotateTorret;
            inputActions.Control.RotateTorret.canceled -= OnRotateTorret;
        }
        #endregion

        public void UpdateHealth(float newHP)
        {
            PlayerHealth.localScale = new Vector3(newHP / stats.MaxHealth, 1, 1);
            if(newHP <= 0)
            {
                //TODO:Death function
                return;
            }
        }
        public void UpdateTankName(string pName)
        {
            text_PlayerName.text = pName;
        }

        public void Fire()
        {
            if (isDead) return;
            Bullet bullet = Instantiate(bulletPrefab, bulletHole.position, bulletHole.rotation);
            bullet.Init(ownerID, stats.damage);
        }
        public bool UseAbility()
        {
            if (isDead) return false;
            if (Time.time < NextAbilityTime) return false;

            GameObject abilityGO = Instantiate(stats.abilityPrefab, transform.position, Quaternion.identity);
            switch(stats.ability)
            {
                case Abilities.Lightning:
                    abilityGO.GetComponent<LightningAbility>().InitOwner(ownerID);
                    break;
                case Abilities.Healing:
                    abilityGO.GetComponent<HealingAbility>().InitOwner(ownerID);
                    break;
            }
            NextAbilityTime = Time.time + stats.abilityCooldown;
            return true;
        }
        public void Kill()
        {
            isDead = true;
        }

        public void initPlayerName(string name)
        {
            text_PlayerName.text = name;
        }

        #region Inputs
        void OnMove(InputAction.CallbackContext context)
        {
            Vector2 move = context.ReadValue<Vector2>();
            movement = new Vector3(move.x, 0.0f, move.y);
        }
        void OnRotateTorret(InputAction.CallbackContext context)
        {
            rot = context.ReadValue<float>();
        }
        #endregion
        private void OnDisable()
        {
            UnsubscripeToInput();
        }
        private void OnDestroy()
        {
            UnsubscripeToInput();
        }
    }
}
