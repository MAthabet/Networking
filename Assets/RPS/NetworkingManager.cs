using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System.Linq;

namespace RPS
{
    public enum GameStatus
    {
        none,
        draw,
        won,
        lost
    }

    public class NetworkingManager : NetworkManager
    {
        static new NetworkingManager singleton;

        public static NetworkingManager Singleton => singleton;

        bool isServer;
        bool isClient;
        List<NetworkPlayer> netPlayers;
        public bool IsServer => isServer;
        public bool IsClient => isClient;
        public bool IsHost => isServer && isClient;
        public string PlayerName { get; private set; }

        public NetworkPlayer localPlayer => netPlayers.First(x => x.isLocalPlayer);
        public NetworkPlayer otherPlayer => netPlayers.First(x => !x.isLocalPlayer);


        private new void Awake()
        {
            if(!singleton)
                singleton = this;
            else
                Destroy(gameObject);
        }
        void Start()
        {
            netPlayers = new List<NetworkPlayer>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            isServer = true;
            Debug.Log("Server started");
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            isClient = true;
            Debug.Log("Client started");
        }

        public void UpdatePlayerName(string s)
        {
            PlayerName = s;
        }
        public void AddPlayer(NetworkPlayer player)
        {
            if (!netPlayers.Contains(player))
            {
                netPlayers.Add(player);
                Debug.Log($"Player {player.netId} added");
            }
            else
            {
                Debug.LogWarning($"Player {player.netId} is already in the list.");
            }
        }
        public void RemovePlayer(NetworkPlayer player)
        {
            if (netPlayers.Contains(player))
            {
                netPlayers.Remove(player);
                Debug.Log($"Player {player.netId} removed");
            }
            else
            {
                Debug.LogWarning($"Player {player.netId} is not in the list.");
            }
        }

        [Server]
        public void CheckGame()
        {
            if (netPlayers.Count < 2)
            {
                return;
            }
            playerMove move1 = localPlayer.PlayerMove;
            playerMove move2 = otherPlayer.PlayerMove;

            if(move1 == playerMove.none || move2 == playerMove.none) return;

            GameStatus res = evaluateGameStatus(move1, move2);
            switch (res)
            {
                case GameStatus.won:
                    {
                        localPlayer.UpdatePlayerScoreBy(1);
                        otherPlayer.UpdatePlayerScoreBy(-1);
                        localPlayer.TargetShowResult(res);
                        otherPlayer.TargetShowResult(GameStatus.lost);
                        break;
                    }
                case GameStatus.lost:
                    {
                        otherPlayer.UpdatePlayerScoreBy(1);
                        localPlayer.UpdatePlayerScoreBy(-1);
                        localPlayer.TargetShowResult(res);
                        otherPlayer.TargetShowResult(GameStatus.won);
                        break;
                    }
                case GameStatus.draw:
                    {
                        localPlayer.UpdatePlayerScoreBy(0);
                        otherPlayer.UpdatePlayerScoreBy(0); 
                        localPlayer.TargetShowResult(res);
                        otherPlayer.TargetShowResult(res);
                        break;
                    }
            }
            

        }

        [Server]
        private GameStatus evaluateGameStatus(playerMove move1, playerMove move2)
        {
            if (move1 == move2) return GameStatus.draw;
            if (move1 == playerMove.Rock && move2 == playerMove.Scissors) return GameStatus.won;
            if (move1 == playerMove.Paper && move2 == playerMove.Rock) return GameStatus.won;
            if (move1 == playerMove.Scissors && move2 == playerMove.Paper) return GameStatus.won;
            return GameStatus.lost;
        }
        [Server]
        public void ResetMove()
        {
            localPlayer.PlayerMove = playerMove.none;
            otherPlayer.PlayerMove = playerMove.none;
        }
    }
}
