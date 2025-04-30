using System;
using Mirror;
using UnityEngine;

namespace RPS
{
    public class NetworkPlayer : NetworkBehaviour
    {
        UIManagerInGame uiManager;
        [SyncVar(hook = nameof(OnNameChanged) )] string PlayerName;
        [SyncVar(hook = nameof(OnMoveChanged) )] public playerMove PlayerMove = playerMove.none;
        [SyncVar(hook = nameof(OnScoreChanged))] int PlayerScore;

        

        private void Awake()
        {
            uiManager = FindFirstObjectByType<UIManagerInGame>();
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkingManager.Singleton.AddPlayer(this);
        }
        public override void OnStopClient()
        {
            base.OnStopClient();
            NetworkingManager.Singleton.RemovePlayer(this);
        }
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            cmdUpdatePlayerName(NetworkingManager.Singleton.PlayerName);
        }

        [Command]
        void cmdUpdatePlayerName(string name)
        {
            PlayerName = name;
        }
        [Command]
        public void CmdUpdatePlayerMove(playerMove newMove)
        {
            PlayerMove = newMove;
        }
        [Server]
        public void UpdatePlayerScoreBy(int delta)
        {
            PlayerScore += delta;
        }
        void OnNameChanged(string oldVal, string newVal)
        {
            PlayerName = newVal;
            uiManager.updateName(isLocalPlayer, PlayerName);
        }

        void OnMoveChanged(playerMove oldVal, playerMove newVal)
        {
            PlayerMove = newVal;
            if (isLocalPlayer) 
                uiManager.DisableUI();
            if(isServer)
                NetworkingManager.Singleton.CheckGame();
        }
        void OnScoreChanged(int oldVal, int newVal)
        {
            PlayerScore = newVal;
            if(isServer)
            NetworkingManager.Singleton.ResetMove();
            uiManager.updateScore(isLocalPlayer, PlayerScore);
        }
        [TargetRpc]
        public void TargetShowResult(GameStatus result)
        {
            uiManager.ShowResult(result);
        }
    }
}
