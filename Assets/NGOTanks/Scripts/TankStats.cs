using UnityEngine;

namespace NGOTanks
{
    [CreateAssetMenu(fileName = "TankStats", menuName = "Scriptable Objects/TankStats")]
    public class TankStats : ScriptableObject
    {
        public Class playerClass;
        public float MaxHealth;
        public float speed;
        public float rotationSpeed;
        public float damage;
    }
}
