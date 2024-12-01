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

        public GameObject nm_UItextObj;
        TextMeshProUGUI nm_UItext;
        [HideInInspector] public string nm_StatusText;
        [HideInInspector] public bool nm_Connected = false;

        [SerializeField]GameObject clientInstancePrefab;

        public PlayerData nm_PlayerData;

        // Start is called before the first frame update
        void Start()
        {
            nm_UItext = nm_UItextObj.GetComponent<TextMeshProUGUI>();
            nm_PlayerData = new PlayerData();
            nm_ServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
        }

        void Update()
        {
            nm_UItext.text = nm_StatusText;
        }

        public void InstatiateGO(PlayerData data)
        {
            GameObject goInstance = Instantiate(clientInstancePrefab, new Vector3(0, 0, 3), new Quaternion(0, 0, 0, 1));
            goInstance.name = data.playerName;
            goInstance.GetComponent<Player>().playerData = data;
            Debug.Log("GO CREATED: " + data.playerName);
        }

        public void SetNetPlayer(string username)
        {
            // Set Up Player before starting server
            GameObject go = GameObject.Find("Player");
            if (go != null)
            {
                go.name = username;
                Player player = go.GetComponent<Player>();
                player.playerData.playerName = username;
                player.playerData.playerId = 0;
                nm_PlayerData = player.playerData;
            }
        }
    }
}