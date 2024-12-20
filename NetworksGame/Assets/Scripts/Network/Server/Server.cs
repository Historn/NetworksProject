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
        Dictionary<EndPoint, int> server_ConnectedUsers = new Dictionary<EndPoint, int>();

        #region CONNECTION
        public void StartHost(string username)
        {
            NetworkManager.Instance.nm_StatusText += "Creating Host Server...";

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            NetworkManager.Instance.nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            NetworkManager.Instance.nm_Socket.Bind(ipep);
            
            int userID = IDGenerator.GenerateID();

            server_ConnectedUsers.Add(NetworkManager.Instance.nm_Socket.LocalEndPoint, userID);

            NetworkManager.Instance.SetNetPlayer(username, userID);

            NetworkManager.Instance.nm_StatusText += $"\nHost User created with name {username} and ID {userID}";

            NetworkManager.Instance.nm_Match = GameObject.Find("MatchManager").GetComponent<Match>();

            Thread mainThread = new Thread(ReceiveHost);
            mainThread.Start();
        }
        #endregion

        void SendHost(EndPoint Remote, byte[] packetToSend)
        {
            try
            {
                NetworkManager.Instance.nm_Socket.SendTo(packetToSend, Remote);
                Debug.Log("PACKET SENT");
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
                NetworkManager.Instance.nm_StatusText = $"\nServer received a packet of {recv} bytes";

                int newUser = -1;

                if (server_ConnectedUsers.ContainsKey(Remote))
                    newUser = server_ConnectedUsers[Remote];

                if (newUser == -1 && server_ConnectedUsers.Count < GameManager.Instance.gm_MaxPlayers && recv > 0)
                {
                    Debug.Log("CREATING NEW USER");

                    // READ DATA FROM NEW CLIENT
                    PlayerDataPacket playerDataPacket = HandlePacket(data);

                    if (playerDataPacket == null) continue;

                    Debug.Log($"DATA PACKET RECEIVED: {playerDataPacket.PlayerName} + {playerDataPacket.PlayerId} + {playerDataPacket.Position[0]} + {playerDataPacket.Position[1]} + {playerDataPacket.Position[2]}");
                    server_ConnectedUsers.Add(Remote, playerDataPacket.PlayerId);

                    // Send all data to NEW CLIENT
                    byte[] packetToSend = new byte[1024];
                    packetToSend = CreatePacketToSend();
                    Thread serverAnswer = new Thread(() => SendHost(Remote, packetToSend));
                    serverAnswer.Start();

                    MainThreadInvoker.Invoke(() =>
                    {
                        NetworkManager.Instance.InstatiateGO(playerDataPacket);

                        // ALL THIS NEEDS TO BE IN THE SAME THREAD AS InstantiateGO
                        NetworkManager.Instance.nm_StatusText += $"\n{playerDataPacket.PlayerName} joined the server called UDP Server";

                        packetToSend = new byte[1024];
                        packetToSend = CreatePacketToSend();
                        
                        if (server_ConnectedUsers.Count > 1)
                        {
                            foreach (KeyValuePair<EndPoint, int> user in server_ConnectedUsers)
                            {
                                if (NetworkManager.Instance.nm_Socket.LocalEndPoint.ToString() == user.Key.ToString() && newUser == user.Value)
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
                        HandlePacket(data);

                    SendPacket(Remote);
                }
            }
        }

        #region REPLICATION
        // HOST HAS TO SEND
        // GAME STATE + CLIENTS STATE + GENERAL NET OBJECTS WITH ITS ACTIONS?
        byte[] CreatePacketToSend()
        {
            byte[] hostPacket = new byte[1024];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Add a header for metadata 
                int headerSize = 12; // 4 bytes for match state size, 4 bytes for player states size, 4 for projectiles
                memoryStream.Position = headerSize;

                Debug.Log("Creating Match State Packet");

                byte[] matchStateData = NetworkManager.Instance.nm_Match.Packet.Serialize(NetworkManager.Instance.nm_LastMatchState);
                memoryStream.Write(matchStateData, 0, matchStateData.Length);
                
                Debug.Log("Creating Players State Packet");
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
                NetworkManager.Instance.nm_ProjectilesToSend.Clear();
                // Write projectile data to the main packet
                byte[] projectilesData = projectilesStream.ToArray();
                if (memoryStream.Length + projectilesData.Length > 1024)
                {
                    Debug.LogWarning("Projectiles data exceeds packet size.");
                    return null;
                }
                memoryStream.Write(projectilesData, 0, projectilesData.Length);
             
                // Add Header
                memoryStream.Position = 0;
                byte[] matchStateSize = BitConverter.GetBytes(matchStateData.Length);
                byte[] playerStatesSize = BitConverter.GetBytes(playersData.Length);
                byte[] projectilesStatesSize = BitConverter.GetBytes(projectilesData.Length);
                memoryStream.Write(matchStateSize, 0, matchStateSize.Length);
                memoryStream.Write(playerStatesSize, 0, playerStatesSize.Length);
                memoryStream.Write(projectilesStatesSize, 0, projectilesStatesSize.Length);

                // Finalize the packet
                hostPacket = memoryStream.ToArray();
                Debug.Log($"Finished Packet. Total Size: {hostPacket.Length} bytes.");
                return hostPacket;
            }
        }

        void SendPacket(EndPoint Remote)
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
                    Thread.Sleep(10); // Delay of 10ms between packets
                });

                answer.Start();
            }
        }

        private PlayerDataPacket HandlePacket(byte[] receivedData)
        {
            // Separate the data into sections
            (byte[] playerData, byte[] projectileData) = SeparateDataSections(receivedData);

            // Handle player data
            PlayerDataPacket playerDataPacket = HandlePlayerData(playerData);

            // Handle projectile data
            HandleProjectileData(projectileData);

            return playerDataPacket;
        }

        private (byte[], byte[]) SeparateDataSections(byte[] data)
        {
            int headerSize = 8;

            int playerSectionSize = BitConverter.ToInt32(data, 0); // Next 4 bytes define the player data length
            int playerSectionStart = headerSize;

            int projectileSectionSize = BitConverter.ToInt32(data, 4); // Next 4 bytes define the projectile data length
            int projectileSectionStart = playerSectionStart + playerSectionSize;

            byte[] playerData = data.Skip(playerSectionStart).Take(playerSectionSize).ToArray();
            byte[] projectileData = data.Skip(projectileSectionStart).ToArray();

            return (playerData, projectileData);
        }

        PlayerDataPacket HandlePlayerData(byte[] playerData)
        {
            if (playerData.Length > 0)
            {
                int playerId = BitConverter.ToInt32(playerData, 1); // 1 Byte Offset for the Type

                // Process game state data here
                var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(playerId) ? NetworkManager.Instance.nm_LastPlayerStates[playerId] : new PlayerDataPacket();

                //int playerSectionLength = BitConverter.ToInt32(receivedData, 0); // First 4 bytes define the length
                //byte[] playerData = receivedData.Skip(4).Take(playerSectionLength).ToArray();

                // Extract player-specific data
                PlayerDataPacket playerPacket = new PlayerDataPacket();
                playerPacket.Deserialize(playerData, lastState);

                var player = NetworkManager.Instance.nm_ActivePlayers.ContainsKey(playerId) ? NetworkManager.Instance.nm_ActivePlayers[playerId] : null;
                Debug.Log($"ESTE JUGADOR HA ENVIADO PACK {playerPacket.PlayerName}");
                if (player != null)
                {
                    player.Packet = playerPacket;
                    player.updateGO = true;
                    lastState = playerPacket;
                }
                else
                {
                    Debug.Log("Player Not Found");
                }
                return playerPacket;
            }
            return null;
        }
        
        void HandleProjectileData(byte[] projectileData)
        {
            while (projectileData.Length > 0)
            {
                int projectileId = BitConverter.ToInt32(projectileData, 1);
                var lastState = new ProjectilePacket();

                ProjectilePacket projectile = new ProjectilePacket();
                projectile.Deserialize(projectileData, lastState);
                if (projectileId != 0 && projectileId != -1)
                {
                    MainThreadInvoker.Invoke(() =>
                    {
                        Debug.Log($"\nInstantiating NEW PROJECTILE: {projectile.ProjectileId}.");
                        Projectile existingProjectile = NetworkManager.Instance.InstatiateProjectile(projectile);
                        NetworkManager.Instance.nm_ProjectilesToSend.Add(projectileId, existingProjectile);
                        NetworkManager.Instance.nm_ActiveProjectiles.Add(projectileId);
                    });
                }
                projectileData = NetworkManager.Instance.TrimProcessedData(projectileData, projectileId);
            }
            Debug.Log("Projectile data processed.");
        }

        public int GetUserByEndPoint(EndPoint endPoint)
        {
            return server_ConnectedUsers.ContainsKey(endPoint) ? server_ConnectedUsers[endPoint] : -1;
        }
        
        #endregion
    }
}


