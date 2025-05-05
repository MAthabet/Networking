using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;


namespace NGOTanks
{
    public class HealingAbility : MonoBehaviour
    {
        ulong ownerID;
        [SerializeField] float healingAmount;
        [SerializeField] float healingCooldown;
        [SerializeField] float range;
        [SerializeField] float timer;

        Dictionary<ulong, Coroutine> activeHeals = new Dictionary<ulong, Coroutine>();
        
        void Start()
        {
            SphereCollider collider;
            collider = GetComponent<SphereCollider>();
            collider.radius = range;

            VisualEffect vfx;
            vfx = GetComponent<VisualEffect>();
            vfx.SetFloat("Diameter", range * 2);
            vfx.SetFloat("Dissapation", timer);
            vfx.Play();

            Destroy(gameObject, timer + 0.1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (NetworkingManager.Singleton.IsServer)
            {
                if (other.CompareTag("Tank"))
                {
                    if (other.TryGetComponent<Tank>(out Tank tank))
                    {
                        NetworkPlayer netPlayer = NetworkingManager.Singleton.GetPlayer(tank.GetOwnerID());
                        Team healZoneTeam = NetworkingManager.Singleton.GetPlayer(ownerID).GetTeam();
                        Team tankTeam = netPlayer.GetTeam();
                        if (healZoneTeam == tankTeam)
                        {
                            Coroutine healRoutine = StartCoroutine(HealPlayerOverTime(netPlayer));
                            activeHeals[netPlayer.OwnerClientId] = healRoutine;
                        }
                    }
                }
            }
        }
        private IEnumerator HealPlayerOverTime(NetworkPlayer player)
        {
            player.Heal(healingAmount);

            WaitForSeconds wait = new WaitForSeconds(healingCooldown);

            while (true)
            {
                yield return wait;

                if (player == null) break;

                player.Heal(healingAmount);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (NetworkingManager.Singleton.IsServer)
            {
                if (other.CompareTag("Tank"))
                {
                    if (other.TryGetComponent<Tank>(out var tank))
                    {
                        var playerID = tank.GetOwnerID();
                        if (activeHeals.TryGetValue(playerID, out var routine))
                        {
                            StopCoroutine(routine);
                            activeHeals.Remove(playerID);
                        }
                    }
                }
            }
        }
        public void InitOwner(ulong _ownerID)
        {
            ownerID = _ownerID;
        }
    }
}
