using TMPro;
using UnityEngine;

namespace RPS
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField IF_PlayerName;

        public void OnStartServerClicked()
        {
            NetworkingManager.Singleton.StartServer();
        }

        public void OnStartHostClicked()
        {
            NetworkingManager.Singleton.StartHost();
        }

        public void OnStartClientClicked()
        {
            NetworkingManager.Singleton.StartClient();
        }
        public void GetName()
        {
            NetworkingManager.Singleton.UpdatePlayerName(IF_PlayerName.text);
        }
    }
}
