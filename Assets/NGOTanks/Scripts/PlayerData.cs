using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace NGOTanks
{
    public enum Team
    {
        Red,
        Blue
    }
    public enum Class
    {
        Tank,
        DPS
    }
    public struct PlayerData : INetworkSerializable
    {
        public FixedString64Bytes playerName;
        public Team playerTeam;
        public Class playerClass;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerTeam);
            serializer.SerializeValue(ref playerClass);
        }
    }
}
