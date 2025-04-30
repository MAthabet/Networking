using UnityEngine;

namespace NGOTanks
{
    public class bullet : MonoBehaviour
    {
        [SerializeField] float speed;
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
    }
}
