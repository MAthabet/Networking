using UnityEngine;

namespace NGOTanks
{
    public enum Abilities
    {
        Lightning,
        Healing
    }
    [CreateAssetMenu(fileName = "TankStats", menuName = "Scriptable Objects/TankStats")]
    public class TankStats : ScriptableObject
    {
        public Class playerClass;
        public float MaxHealth;
        public float speed;
        public float rotationSpeed;
        public float damage;
        public Abilities ability;
        public GameObject abilityPrefab;
        public float abilityCooldown;
    }
}
