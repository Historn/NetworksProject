using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using System.Threading;

namespace HyperStrike
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        void Awake() 
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

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

        #region NetObjectControl
        public void InstatiateGO(PlayerDataPacket data)
        {
            GameObject goInstance = Instantiate(clientInstancePrefab, new Vector3(data.Position[0], data.Position[1], data.Position[2]), new Quaternion(0, 0, 0, 1));
            goInstance.name = data.PlayerName;
            Player player = goInstance.GetComponent<Player>();
            player.Packet = data;
            nm_ActivePlayers.Add(data.PlayerId, player);
            nm_LastPlayerStates.Add(data.PlayerId, player.Packet);
        }
        
        public Projectile InstatiateProjectile(ProjectilePacket data)
        {
            GameObject goInstance = Instantiate(rocketInstancePrefab, new Vector3(data.Position[0], data.Position[1], data.Position[2]), new Quaternion(data.Rotation[0], data.Rotation[1], data.Rotation[2], data.Rotation[3]));
            Projectile projectile = goInstance.GetComponent<Projectile>();
            projectile.Packet = data;
            nm_ActiveProjectiles.Add(data.ProjectileId);
            return projectile;
        }

        public Player GetPlayerById(int id)
        {
            return nm_ActivePlayers.ContainsKey(id) ? nm_ActivePlayers[id] : null;
        }
        
        public bool GetProjectileById(int id)
        {
            return nm_ActiveProjectiles.Contains(id) ? true : false;
        }
        #endregion

        #region PacketHandling
        public void HandlePacket(byte[] receivedData, out PlayerDataPacket playerData)
        {
            List<byte[]> packets = SeparateDataPackets(receivedData);

            PacketType type = PacketType.NONE;

            playerData = null;
            //Debug.Log($"There are {packets.Count} different packets");
            if (packets.Count < 1)
                return;

            foreach (byte[] packet in packets)
            {
                type = (PacketType)BitConverter.ToInt32(packet, 0); // Mal
                switch (type)
                {
                    case PacketType.NONE:
                        break;
                    case PacketType.GAME_STATE:
                        break;
                    case PacketType.MATCH:
                        HandleMatchStateData(packet);
                        break;
                    case PacketType.PLAYER_DATA:
                        playerData = HandlePlayerData(packet);
                        break;
                    case PacketType.ABILITY:
                        break;
                    case PacketType.PROJECTILE:
                        HandleProjectileData(packet);
                        break;
                    default:
                        break;
                }
            }
        }

        private List<byte[]> SeparateDataPackets(byte[] data)
        {
            List<byte[]> packets = new List<byte[]>();

            for (int nextPacket = 0; nextPacket < data.Length;)
            {
                PacketType packetType = (PacketType)data[nextPacket];
                if (packetType == PacketType.NONE) break;

                int packetSize = BitConverter.ToInt32(data, nextPacket + 1); // Take Size

                byte[] packet = data.Skip(nextPacket).Take(packetSize).ToArray();
                packets.Add(packet);

                nextPacket += packetSize;
            }

            return packets;
        }

        private void HandleMatchStateData(byte[] matchStateData)
        {
            MatchStatePacket match = new MatchStatePacket();
            match.Deserialize(matchStateData, nm_LastMatchState);

            nm_Match.Packet = match;
            nm_Match.updateGO = true;

            nm_LastMatchState = match;
        }

        private PlayerDataPacket HandlePlayerData(byte[] playerData)
        {
            int playerId = BitConverter.ToInt32(playerData, 5); // 5 Byte Offset for the Type and Size

            // Process game state data here
            var lastState = nm_LastPlayerStates.ContainsKey(playerId) ? nm_LastPlayerStates[playerId] : new PlayerDataPacket();

            // Extract player-specific data
            PlayerDataPacket playerPacket = new PlayerDataPacket();
            playerPacket.Deserialize(playerData, lastState);
            

            if(playerId == nm_PlayerData.PlayerId) return playerPacket;

            MainThreadInvoker.Invoke(() =>
            {
                var player = GetPlayerById(playerId);
                if (player != null)
                {
                    player.Packet = playerPacket;
                    player.updateGO = true;
                    lastState = playerPacket;
                }
                else
                {
                    InstatiateGO(playerPacket);
                    nm_StatusText += $"\n{playerPacket.PlayerName} joined the server called UDP Server";
                }
            });
            return playerPacket;
        }

        private void HandleProjectileData(byte[] projectileData)
        {
            int projectileId = BitConverter.ToInt32(projectileData, 5); // 5 Byte Offset for the Type and Size

            // Process game state data here
            var lastState = new ProjectilePacket();

            // Extract player-specific data
            ProjectilePacket projectilePacket = new ProjectilePacket();
            projectilePacket.Deserialize(projectileData, lastState);

            var projectile = GetProjectileById(projectileId);

            if (!projectile)
            {
                MainThreadInvoker.Invoke(() =>
                {
                    Projectile existingProjectile = InstatiateProjectile(projectilePacket);
                    // NEED TO CHECK IF ITS SERVER AND SEND
                });
            }
        }
        #endregion
    }
}