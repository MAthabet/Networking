using Unity.Netcode;
using Unity.Collections;

namespace NGOTanks
{
    public enum Team
    {
        None,
        Brown,
        Blue
    }
    public enum Class
    {
        None,
        Tank,
        DPS
    }
    public struct PlayerData : INetworkSerializable
    {
        public FixedString64Bytes playerName;
        public Team playerTeam;
        public Class playerClass;

        public PlayerData(FixedString64Bytes name, Team team, Class playerClass)
        {
            playerName = name;
            playerTeam = team;
            this.playerClass = playerClass;
        }
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerName);
            serializer.SerializeValue(ref playerTeam);
            serializer.SerializeValue(ref playerClass);
        }
    }
}
