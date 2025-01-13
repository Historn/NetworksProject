using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace HyperStrike
{
    public class Server : MonoBehaviour
    {
        TimeoutManager timeoutManager = new TimeoutManager();
        Dictionary<EndPoint, int> server_ConnectedUsers = new Dictionary<EndPoint, int>();

        Thread receive;

        void Update()
        {
            if (NetworkManager.Instance.nm_Connected)
                CheckForTimeouts();
        }

        #region CONNECTION
        public bool StartHost(string username)
        {
            try
            {
                NetworkManager.Instance.nm_StatusText += "Creating Host Server...";
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
                NetworkManager.Instance.nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                NetworkManager.Instance.nm_Socket.Bind(ipep);
                return true;
            }
            catch (SocketException ex)
            {
                NetworkManager.Instance.nm_StatusText += $"\nError creating server: {ex.Message}";
                return false;
            }
        }

        public void SetHost(string username)
        {
            int userID = IDGenerator.GenerateID();

            server_ConnectedUsers.Add(NetworkManager.Instance.nm_Socket.LocalEndPoint, userID);

            NetworkManager.Instance.SetNetPlayer(username, userID);

            NetworkManager.Instance.nm_StatusText += $"\nHost User created with name {username} and ID {userID}";

            NetworkManager.Instance.nm_Match = GameObject.Find("MatchManager").GetComponent<Match>();
            NetworkManager.Instance.nm_Ball = GameObject.Find("Ball").GetComponent<BallController>();

            NetworkManager.Instance.nm_Connected = true;
            NetworkManager.Instance.nm_IsHost = true;

            receive = new Thread(ReceiveHost);
            receive.Start();
        }

        // Periodically check for timeouts
        void CheckForTimeouts()
        {
            List<int> timedOutPlayers = timeoutManager.CheckTimeouts();
            foreach (int playerId in timedOutPlayers)
            {
                // Remove the player from connected users and notify others
                EndPoint playerEndpoint = server_ConnectedUsers.First(kvp => kvp.Value == playerId).Key;
                server_ConnectedUsers.Remove(playerEndpoint);

                GameObject timedOutPlayer = NetworkManager.Instance.nm_ActivePlayers[playerId].gameObject;

                NetworkManager.Instance.nm_StatusText += $"Player {timedOutPlayer.name} timed out and was removed from the game.";

                Destroy(timedOutPlayer);
                NetworkManager.Instance.nm_ActivePlayers.Remove(playerId);
            }
        }

        void HandshakeToClient(byte[] data, int recv, EndPoint Remote)
        {
            string message = System.Text.Encoding.UTF8.GetString(data, 0, recv);
            if (message == "PING")
            {
                byte[] pongMessage = System.Text.Encoding.UTF8.GetBytes("PONG");
                NetworkManager.Instance.nm_Socket.SendTo(pongMessage, Remote);

                NetworkManager.Instance.nm_StatusText += $"\nReceived PING seding PONG";
            }
        }
        #endregion

        void SendHost(EndPoint Remote, byte[] packetToSend)
        {
            try
            {
                NetworkManager.Instance.nm_Socket.SendTo(packetToSend, Remote);
                Thread.Sleep(10); // Delay of 10ms between packets
            }
            catch (SocketException ex)
            {
                Debug.Log($"Send host error: {ex.Message}");
            }
        }

        void ReceiveHost()
        {
            byte[] data = new byte[1024];
            int recv = 0;

            NetworkManager.Instance.nm_StatusText += "\nWaiting for new players...";

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            while (true)
            {
                data = new byte[1024];
                recv = NetworkManager.Instance.nm_Socket.ReceiveFrom(data, ref Remote);

                int playerId = GetUserByEndPoint(Remote);

                if (playerId != -1)
                {
                    timeoutManager.UpdateActivity(playerId);
                }

                if (playerId == -1 && server_ConnectedUsers.Count < GameManager.Instance.gm_MaxPlayers && recv > 0)
                {
                    //Thread handshake = new Thread(() => HandshakeToClient(data, recv, Remote));
                    HandshakeToClient(data, recv, Remote);

                    // READ DATA FROM NEW CLIENT
                    NetworkManager.Instance.HandlePacket(data, out PlayerDataPacket playerDataPacket);

                    if (playerDataPacket == null) continue;

                    server_ConnectedUsers.Add(Remote, playerDataPacket.PlayerId);

                    // Send all data to NEW CLIENT
                    byte[] packetToSend = CreatePacketToSend();
                    Thread serverAnswer = new Thread(() => SendHost(Remote, packetToSend));
                    serverAnswer.Start();

                    MainThreadInvoker.Invoke(() =>
                    {
                        packetToSend = new byte[1024];
                        packetToSend = CreatePacketToSend();

                        if (server_ConnectedUsers.Count > 2)
                        {
                            foreach (KeyValuePair<EndPoint, int> user in server_ConnectedUsers)
                            {
                                if (NetworkManager.Instance.nm_Socket.LocalEndPoint.ToString() == user.Key.ToString() && playerId == user.Value)
                                    continue;

                                Thread answer = new Thread(() => SendHost(user.Key, packetToSend));
                                answer.Start();
                            }
                        }
                    });
                }
                else
                {
                    if(recv > 0)
                        NetworkManager.Instance.HandlePacket(data, out _);

                    SendPacket();
                }
            }
        }

        #region REPLICATION
        byte[] CreatePacketToSend()
        {
            byte[] hostPacket = new byte[1024];

            using (MemoryStream memoryStream = new MemoryStream())
            {                                                                                                                                                                                                                                                                                                                                                                                                           
                byte[] matchStateData = NetworkManager.Instance.nm_Match.Packet.Serialize(NetworkManager.Instance.nm_LastMatchState);
                memoryStream.Write(matchStateData, 0, matchStateData.Length);

                byte[] ballData = NetworkManager.Instance.nm_Ball.Packet.Serialize(NetworkManager.Instance.nm_LastBallState);
                memoryStream.Write(ballData, 0, ballData.Length);
                
                MemoryStream playerStream = new MemoryStream();
             
                foreach (KeyValuePair<int, Player> p in NetworkManager.Instance.nm_ActivePlayers)
                {
                    var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(p.Value.Packet.PlayerId)
                        ? NetworkManager.Instance.nm_LastPlayerStates[p.Value.Packet.PlayerId]
                        : new PlayerDataPacket();

                    byte[] playerPacket = p.Value.Packet.Serialize(lastState);
                    if (memoryStream.Length + playerStream.Length + playerPacket.Length > 1024)
                    {
                        Debug.LogWarning($"Skipping player {p.Key} due to packet size limit.");
                        break;
                    }
                    playerStream.Write(playerPacket, 0, playerPacket.Length);
                }
                // Write player data to the main packet
                byte[] playersData = playerStream.ToArray();
                if (memoryStream.Length + playersData.Length > 1024)
                {
                    Debug.LogWarning("Player data exceeds packet size.");
                    return null;
                }
                memoryStream.Write(playersData, 0, playersData.Length);
               

                // PROJECTILES ENVIAR SOLO LA PRIMERA VEZ QUE SE RECIBEN
                MemoryStream projectilesStream = new MemoryStream();
                if (NetworkManager.Instance.nm_ProjectilesToSend.Count>0)
                {
                    foreach (KeyValuePair<int, Projectile> pr in NetworkManager.Instance.nm_ProjectilesToSend)
                    {
                        var lastProjectileState = new ProjectilePacket();

                        byte[] projectilePacket = pr.Value.Packet.Serialize(lastProjectileState);
                        if (memoryStream.Length + projectilesStream.Length + projectilePacket.Length > 1024)
                        {
                            Debug.LogWarning($"Skipping projectile {pr.Key} due to packet size limit.");
                            break;
                        }
                        projectilesStream.Write(projectilePacket, 0, projectilePacket.Length);
                    }
                }
                
                NetworkManager.Instance.nm_ProjectilesToSend.Clear();
                // Write projectile data to the main packet
                byte[] projectilesData = projectilesStream.ToArray();
                if (memoryStream.Length + projectilesData.Length > 1024)
                {
                    Debug.LogWarning("Projectiles data exceeds packet size.");
                    return null;
                }
                memoryStream.Write(projectilesData, 0, projectilesData.Length);

                // Finalize the packet
                hostPacket = memoryStream.ToArray();
                return hostPacket;
            }
        }

        void SendPacket()
        {
            // Broadcast the updated player position to all clients
            byte[] packetToSend = new byte[1024];
            packetToSend = CreatePacketToSend();

            foreach (KeyValuePair<EndPoint, int> u in server_ConnectedUsers)
            {
                if (NetworkManager.Instance.nm_Socket.LocalEndPoint.ToString() == u.Key.ToString())
                    continue;

                // SEND PACKETS
                Thread answer = new Thread(() =>
                {
                    SendHost(u.Key, packetToSend);
                });

                answer.Start();
            }
        }

        public int GetUserByEndPoint(EndPoint endPoint)
        {
            return server_ConnectedUsers.ContainsKey(endPoint) ? server_ConnectedUsers[endPoint] : -1;
        }
        #endregion

        private void OnApplicationQuit()
        {
            if (receive != null && receive.IsAlive)
            {
                receive.Interrupt();
            }
        }
    }
}


