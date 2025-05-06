using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace NGOTanks
{
    public class LightningAbility : MonoBehaviour
    {
        ulong ownerID;
        [SerializeField] float damage;
        [SerializeField] float range;
        [SerializeField] float timer;

        VisualEffect vfx;
        void Start()
        {
            vfx = GetComponent<VisualEffect>();
            vfx.SetFloat("Diameter", range*2);
            vfx.SetFloat("Anticipation", timer);
            if (NetworkingManager.Singleton.IsServer)
            {
                StartCoroutine(DamageAllInRange());
            }
            vfx.Play();

            Destroy(gameObject, timer + 0.1f);
        }

        IEnumerator DamageAllInRange()
        {
            yield return new WaitForSeconds(timer);
            List<Tank> tanks = GetTanksInRange(transform.position, range);
            foreach (Tank tank in tanks)
            {
                NetworkPlayer netPlayer = NetworkingManager.Singleton.GetPlayer(tank.GetOwnerID());
                Debug.Log($"Bomb {ownerID} hit {tank.GetOwnerID()}");
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
                }
            }
        }
        public List<Tank> GetTanksInRange(Vector3 center, float radius)
        {
            Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            List<Tank> tanksInRange = new List<Tank>();

            foreach (Collider collider in hitColliders)
            {
                if (collider.CompareTag("Tank"))
                {
                    Tank tank = collider.GetComponent<Tank>();
                    if (tank != null)
                    {
                        tanksInRange.Add(tank);
                    }
                }
            }

            return tanksInRange;
        }

        public void InitOwner(ulong _ownerID)
        {
            ownerID = _ownerID;
        }
    }
}
