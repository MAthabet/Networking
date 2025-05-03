using UnityEngine;

namespace NGOTanks
{
    public class bullet : MonoBehaviour
    {
        [SerializeField] float speed;
        [SerializeField] float damage;

        ulong ownerID;
        Rigidbody rb;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.AddForce(-transform.up * speed, ForceMode.Impulse);
            Destroy(gameObject, 3f);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void init(ulong ID)
        {
            ownerID = ID;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(NetworkingManager.Singleton.IsServer)
            {
                if(other.CompareTag("Tank"))
                {
                    if(other.TryGetComponent<NetworkPlayer>(out NetworkPlayer netPlayer))
                    {
                        netPlayer.TakeDamage(damage, ownerID);
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
