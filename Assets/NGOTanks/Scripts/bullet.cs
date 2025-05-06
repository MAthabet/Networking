using UnityEngine;

namespace NGOTanks
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float speed;
        float damage;

        ulong ownerID;
        Rigidbody rb;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * speed, ForceMode.Impulse);
            Destroy(gameObject, 3f);
        }

        public void Init(ulong _ownerID, float _damage)
        {
            ownerID = _ownerID;
            damage = _damage;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(NetworkingManager.Singleton.IsServer)
            {
                if(other.CompareTag("Tank"))
                {
                    Debug.Log("collider :" + other.gameObject.name);
                    if(other.TryGetComponent<Tank>(out Tank tank))
                    {                        
                        NetworkPlayer netPlayer = NetworkingManager.Singleton.GetPlayer(tank.GetOwnerID());
                        Debug.Log("tank name: " + tank.gameObject.name) ;
                        Debug.Log($"Bullet {ownerID} hit {tank.GetOwnerID()}");
                        if (GameSettings.Singleton.IsFriendlyFireOn())
                        {
                            netPlayer.TakeDamage(damage, ownerID);
                        }
                        else 
                        {
                            Team bulletTeam = NetworkingManager.Singleton.GetPlayer(ownerID).GetTeam();
                            Team tankTeam = netPlayer.GetTeam();
                            if (bulletTeam != tankTeam)
                                netPlayer.TakeDamage(damage, ownerID);
                            else
                            {
                                //bullet to go thru teammate
                                return;
                            }
                        }
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
