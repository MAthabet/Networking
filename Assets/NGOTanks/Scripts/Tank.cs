using TMPro;
using Unity.Collections.LowLevel.Unsafe;
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
        [SerializeField] Transform HUDRoot;
        [SerializeField] bullet bulletPrefab;

        Transform cam;
        Rigidbody rb;
        public bool isDead { get; private set; }

        float MaxHealth;
        float speed;
        float rotationSpeed;
        float damage;

        private InputActionMap IAM;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (isDead)
            {
                return;
            }
            if(cam)
                HUDRoot.LookAt(cam);
        }
        public void TankInit()
        {
            rb = GetComponent<Rigidbody>();
            cam = Camera.main.transform;
            GetComponent<PlayerInput>().Enable();
            isDead = false;
            
            IA_Tank inputActions = new IA_Tank();
            inputActions.Control.MoveTank.performed += OnMove;
            inputActions.Control.RotateTorret.performed += OnRotateTorret;
            
        }

        public void UpdateHealth(float newHP)
        {
            PlayerHealth.localScale = new Vector3(newHP / MaxHealth, 1, 1);
        }

        public bullet Fire()
        {
            return Instantiate(bulletPrefab, bulletHole.position, bulletHole.rotation);
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
            Vector3 movement = new Vector3(move.x, 0.0f, move.y);
            rb.MovePosition(rb.position + movement * Time.deltaTime);
        }
        void OnRotateTorret(InputAction.CallbackContext context)
        {
            float rot = context.ReadValue<float>();
            cannonPivot.Rotate(Vector3.up * rot * rotationSpeed * Time.deltaTime);
        }
        #endregion
    }
}
