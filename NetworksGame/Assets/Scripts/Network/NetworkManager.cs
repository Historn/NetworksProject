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

        [HideInInspector]public IPEndPoint nm_ServerEndPoint;

        // Network Threads
        //Thread nm_MainNetworkThread;

        User nm_User;
        [HideInInspector] public bool nm_InstantiateNewPlayer = false;

        public GameObject nm_UItextObj;
        TextMeshProUGUI nm_UItext;
        [HideInInspector] public string nm_StatusText;
        [HideInInspector] public bool nm_Connected = false;

        [SerializeField]GameObject clientInstancePrefab;

        [SerializeField]GameObject playerPrefab;
        [HideInInspector] public Player player;

        // Start is called before the first frame update
        void Start()
        {
            nm_UItext = nm_UItextObj.GetComponent<TextMeshProUGUI>();
            player = new Player(playerPrefab.GetComponent<Player>());
            nm_ServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
        }

        void Update()
        {
            nm_UItext.text = nm_StatusText;

            if (SceneManager.GetActiveScene().name == "PitchScene" && nm_InstantiateNewPlayer)
            {
                Debug.Log(player.playerData.playerName);
                string name = player.playerData.playerName;
                player = GameObject.Find("Player").GetComponent<Player>();
                if (player != null)
                {
                    player.playerData.playerName = name;
                    player.name = name;
                    player.updateGO = true;
                    nm_InstantiateNewPlayer = false;
                }
            }
        }

        public void InstatiateGO(PlayerData data)
        {
            GameObject goInstance = Instantiate(clientInstancePrefab, new Vector3(0, 0, 3), new Quaternion(0, 0, 0, 1));
            goInstance.name = data.playerName;
            goInstance.GetComponent<Player>().playerData = data;
            Debug.Log("GO CREATED: " + data.playerName);
        }
    }
}