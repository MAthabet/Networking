using TMPro;
using UnityEngine;

namespace NGOTanks
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
            GetName();
            NetworkingManager.Singleton.StartHost();
        }

        public void OnStartClientClicked()
        {
            GetName();
            NetworkingManager.Singleton.StartClient();
        }
        public void GetName()
        {
            NetworkingManager.Singleton.UpdatePlayerName(IF_PlayerName.text);
        }
    }
}
