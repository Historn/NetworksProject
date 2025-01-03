using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using TMPro;
using System.Linq;
using System;

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
        [SerializeField]GameObject rocketInstancePrefab;

        [HideInInspector] public GameObject nm_Player;
        public PlayerDataPacket nm_PlayerData = new PlayerDataPacket();


        // VARIABLES FOR REPLICATION MANAGEMENT
        public Match nm_Match;
        public MatchStatePacket nm_LastMatchState = new MatchStatePacket();

        [HideInInspector]
        public Dictionary<int, Player> nm_ActivePlayers = new Dictionary<int, Player>();
        public Dictionary<int, Projectile> nm_ProjectilesToSend = new Dictionary<int, Projectile>();
        public List<int> nm_ActiveProjectiles = new List<int>();

        [HideInInspector]
        public Dictionary<int, PlayerDataPacket> nm_LastPlayerStates = new Dictionary<int, PlayerDataPacket>();

        // Start is called before the first frame update
        void Start()
        {
            nm_UItext = nm_UItextObj.GetComponent<TextMeshProUGUI>();
            nm_ServerEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
        }

        void Update()
        {
            nm_UItext.text = nm_StatusText;
        }

        #region CONNECTION
        public void SetNetPlayer(string username, int id)
        {
            // Set Up Player before starting server
            GameObject go = GameObject.Find("Player");
            if (go != null)
            {
                go.name = username;
                Player player = go.GetComponent<Player>();
                player.Packet.PlayerName = username;

                player.Packet.PlayerId = id;

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
            Player player = goInstance.GetComponent<Player>();
            player.Packet = data;
            nm_ActivePlayers.Add(data.PlayerId, player);
            nm_LastPlayerStates.Add(data.PlayerId, player.Packet);
            //Debug.Log($"GO CREATED: {data.PlayerName}, {data.PlayerId}");
        }
        
        public Projectile InstatiateProjectile(ProjectilePacket data)
        {
            GameObject goInstance = Instantiate(rocketInstancePrefab, new Vector3(data.Position[0], data.Position[1], data.Position[2]), new Quaternion(data.Rotation[0], data.Rotation[1], data.Rotation[2], data.Rotation[3]));
            Projectile projectile = goInstance.GetComponent<Projectile>();
            projectile.Packet = data;
            //Debug.Log($"PROJECTILE CREATED: {data.ProjectileId} from {data.ShooterId}");
            return projectile;
        }

        public Player GetPlayerById(int id)
        {
            return nm_ActivePlayers.ContainsKey(id) ? nm_ActivePlayers[id] : null;
        }
        
        public Projectile GetProjectileById(int id)
        {
            return nm_ProjectilesToSend.ContainsKey(id) ? nm_ProjectilesToSend[id] : null;
        }

        public byte[] TrimProcessedData(byte[] data, int processedId)
        {
            // Assuming fixed packet sizes or ability to determine processed packet length
            int processedPacketLength = CalculatePacketLength(data, processedId); // Implement based on your format
            return data.Skip(processedPacketLength).ToArray();
        }

        private int CalculatePacketLength(byte[] data, int processedId)
        {
            // Example logic for determining packet length based on the data structure
            // Assuming the first 4 bytes after the ID represent the packet length

            int idOffset = 4; // Offset where ID ends
            int lengthOffset = idOffset; // Position where length is stored

            if (data.Length < lengthOffset + 4)
            {
                throw new InvalidOperationException("Data is too short to determine packet length.");
            }

            // Extract the packet length (assuming 4-byte integer)
            int packetLength = BitConverter.ToInt32(data, lengthOffset);
            return packetLength;
        }

        #endregion
    }
}