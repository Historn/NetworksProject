using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

namespace HyperStrike
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance { get; private set; }
        void Awake() { if (instance == null) instance = this; }

        Socket nm_Socket;

        public IPEndPoint serverEndPoint;

        // Network Threads
        Thread nm_MainNetworkThread;

        User nm_User;
        bool instantiateNewPlayer = false;

        public GameObject UItextObj;
        TextMeshProUGUI UItext;
        public string nm_StatusText;
        bool creatingPlayer = false;
        bool connected = false;

        [SerializeField]GameObject clientInstancePrefab;

        [SerializeField]GameObject playerPrefab;
        public Player player;
        PlayerData receivedPlayerData;

        // Start is called before the first frame update
        void Start()
        {
            UItext = UItextObj.GetComponent<TextMeshProUGUI>();
            player = new Player(playerPrefab.GetComponent<Player>());
            serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
        }

        void Update()
        {
            UItext.text = nm_StatusText;

            if (SceneManager.GetActiveScene().name == "PitchScene" && creatingPlayer)
            {
                string name = player.playerData.playerName;
                player = GameObject.Find("Player").GetComponent<Player>();
                if (player != null)
                {
                    player.playerData.playerName = name;
                    player.name = name;
                    player.updateGO = true;
                    creatingPlayer = false;
                }
            }

            
        }

        
    }
}