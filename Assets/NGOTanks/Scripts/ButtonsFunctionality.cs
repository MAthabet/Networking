using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace NGOTanks
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField IF_PlayerName;
        [SerializeField] private TMP_Dropdown DD_Team;
        [SerializeField] private TMP_Dropdown DD_Class;

        private void Start()
        {
            InitializeUI();
        }
        public void InitializeUI()
        {
            string[] teamNames = System.Enum.GetNames(typeof(Team));
            DD_Team.ClearOptions();
            DD_Team.AddOptions(new System.Collections.Generic.List<string>(teamNames));

            string[] classNames = System.Enum.GetNames(typeof(Class));
            List<TMP_Dropdown.OptionData> DD_ClassOptions = new List<TMP_Dropdown.OptionData>();
            foreach (string className in classNames)
            {
                DD_ClassOptions.Add(new TMP_Dropdown.OptionData(className));
            }
            DD_Class.options = DD_ClassOptions;


        }
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
