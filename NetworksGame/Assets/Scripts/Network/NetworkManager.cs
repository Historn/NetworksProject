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
        public static NetworkManager Instance { get; private set; }
        void Awake() { if (Instance == null) Instance = this; }

        [HideInInspector]public Socket nm_Socket;
        [HideInInspector]public IPEndPoint nm_ServerEndPoint;

        public GameObject nm_UItextObj;
        TextMeshProUGUI nm_UItext;
        [HideInInspector] public string nm_StatusText;
        [HideInInspector] public bool nm_Connected = false;

        [SerializeField]GameObject clientInstancePrefab;

        [HideInInspector] public GameObject nm_Player;
        public PlayerDataPacket nm_PlayerData;


        // VARIABLES FOR REPLICATION MANAGEMENT
        [HideInInspector]
        public Dictionary<int, Player> nm_ActivePlayers = new Dictionary<int, Player>();
        public Dictionary<int, Projectile> nm_ActiveProjectiles = new Dictionary<int, Projectile>();

        [HideInInspector]
        public Dictionary<int, PlayerDataPacket> nm_LastPlayerStates = new Dictionary<int, PlayerDataPacket>();
        public Dictionary<int, ProjectilePacket> nm_LastProjectileStates = new Dictionary<int, ProjectilePacket>();

        // Start is called before the first frame update
        void Start()
        {
            nm_UItext = nm_UItextObj.GetComponent<TextMeshProUGUI>();
            nm_PlayerData = new PlayerDataPacket();
            nm_ServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
        }

        void Update()
        {
            nm_UItext.text = nm_StatusText;
        }

        #region CONNECTION
        public void SetNetPlayer(string username, bool isHost = false)
        {
            // Set Up Player before starting server
            GameObject go = GameObject.Find("Player");
            if (go != null)
            {
                go.name = username;
                Player player = go.GetComponent<Player>();
                player.Packet.PlayerName = username;

                if (isHost)
                    player.Packet.PlayerId = 0;
                else
                    player.Packet.PlayerId = -1;

                nm_PlayerData = player.Packet;
                nm_Player = go;
                nm_ActivePlayers.Add(player.Packet.PlayerId, player);
            }
        }
        #endregion

        #region REPLICATION

        public void InstatiateGO(PlayerDataPacket data)
        {
            GameObject goInstance = Instantiate(clientInstancePrefab, new Vector3(0, 0, 3), new Quaternion(0, 0, 0, 1));
            goInstance.name = data.PlayerName;
            goInstance.GetComponent<Player>().Packet = data;
            Debug.Log("GO CREATED: " + data.PlayerName);
        }

        public Player GetPlayerById(int id)
        {
            return nm_ActivePlayers.ContainsKey(id) ? nm_ActivePlayers[id] : null;
        }
        
        public Projectile GetProjectileById(int id)
        {
            return nm_ActiveProjectiles.ContainsKey(id) ? nm_ActiveProjectiles[id] : null;
        }

        #endregion
    }
}