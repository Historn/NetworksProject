using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.IO;
using System;

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
            
            int userID = PlayerIDGenerator.GeneratePlayerID();

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
                Thread.Sleep(100); // Delay of 100ms between packets
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
            Debug.Log(NetworkManager.Instance.nm_StatusText);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            while (true)
            {
                data = new byte[1024];
                recv = NetworkManager.Instance.nm_Socket.ReceiveFrom(data, ref Remote);

                int newUser = -1;

                if (server_ConnectedUsers.ContainsKey(Remote))
                    newUser = server_ConnectedUsers[Remote];


                Debug.Log("esto va bien?: " + newUser.ToString());
                // SET MAX 10 PLAYERS
                if (newUser == -1)
                {
                    Debug.Log("CREATING NEW USER");
                    server_ConnectedUsers.Add(Remote, newUser);

                    // READ DATA FROM NEW CLIENT
                    var lastState = new PlayerDataPacket();
                    PlayerDataPacket playerDataPacket = new PlayerDataPacket();
                    playerDataPacket.Deserialize(data, lastState);
                    Debug.Log($"DATA PACKET RECEIVED: {playerDataPacket.PlayerName} + {playerDataPacket.PlayerId} + {playerDataPacket.Position[0]} + {playerDataPacket.Position[1]} + {playerDataPacket.Position[2]}");

                    // Send all data to NEW CLIENT
                    byte[] packetToSend = new byte[1024];
                    packetToSend = CreatePacketToSend();
                    Thread serverAnswer = new Thread(() => SendHost(Remote, packetToSend));
                    serverAnswer.Start();

                    MainThreadInvoker.Invoke(() =>
                    {
                        // PASS RECEIVED CLIENT DATA
                        NetworkManager.Instance.InstatiateGO(playerDataPacket);
                    });

                    NetworkManager.Instance.nm_StatusText += $"\n{playerDataPacket.PlayerName} joined the server called UDP Server";

                    packetToSend = new byte[1024];
                    packetToSend = CreatePacketToSend();
                    // Change to send everything an detect in client if there's a new CLIENT
                    foreach (KeyValuePair<EndPoint, int> user in server_ConnectedUsers)
                    {
                        if (NetworkManager.Instance.nm_Socket.LocalEndPoint.ToString() == user.Key.ToString())
                            continue;

                        Thread answer = new Thread(() => SendHost(user.Key, packetToSend));
                        answer.Start();
                    }
                }
                else
                {
                    HandlePlayerData(newUser, data, Remote);
                    SendPacket(Remote);
                }
            }
        }

        #region REPLICATION
        // HOST HAS TO SEND
        // GAME STATE + CLIENTS STATE + GENERAL NET OBJECTS WITH ITS ¿ACTIONS?
        byte[] CreatePacketToSend()
        {
            byte[] hostPacket = new byte[1024];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Add a header for metadata 
                int headerSize = 8; // 4 bytes for match state size, 4 bytes for player states size
                memoryStream.Position = headerSize;

                Debug.Log("Creating Match State Packet");

                byte[] matchStateData = NetworkManager.Instance.nm_Match.Packet.Serialize(NetworkManager.Instance.nm_LastMatchState);
                memoryStream.Write(matchStateData, 0, matchStateData.Length);

                Debug.Log("Creating Players State Packet");
                using (MemoryStream playerStream = new MemoryStream())
                {
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
                }

                // Serialize Pool of Projectiles
                //foreach (KeyValuePair<int, Player> p in NetworkManager.instance.nm_ActivePlayers)
                //{
                //    var lastState = lastProjectileStates.ContainsKey(currentProjectState.ProjectileId) ? lastProjectileStates[currentState.ProjectileId] : null;
                //    hostPacket += p.Value.Packet.Serialize(lastState);
                //}

                // Add Header
                memoryStream.Position = 0;
                byte[] matchStateSize = BitConverter.GetBytes(matchStateData.Length);
                byte[] playerStatesSize = BitConverter.GetBytes((int)(memoryStream.Length - headerSize));
                memoryStream.Write(matchStateSize, 0, matchStateSize.Length);
                memoryStream.Write(playerStatesSize, 0, playerStatesSize.Length);

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
                if (NetworkManager.Instance.nm_Socket.LocalEndPoint.ToString() == u.Key.ToString() || Remote.ToString() == u.Key.ToString())
                    continue;

                // SEND PACKETS
                Thread answer = new Thread(() =>
                {
                    SendHost(u.Key, packetToSend);
                    Thread.Sleep(100); // Delay of 10ms between packets
                });

                answer.Start();
            }
        }

        void HandlePlayerData(int user, byte[] receivedData, EndPoint Remote)
        {

            // Process game state data here
            var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(user) ? NetworkManager.Instance.nm_LastPlayerStates[user] : new PlayerDataPacket();
            
            // Extract player-specific data
            PlayerDataPacket playerPacket = new PlayerDataPacket();
            playerPacket.Deserialize(receivedData, lastState);

            var player = NetworkManager.Instance.nm_ActivePlayers.ContainsKey(user) ? NetworkManager.Instance.nm_ActivePlayers[user] : null;

            if (player != null)
            {
                player.Packet = playerPacket;
                player.updateGO = true;
            }   
            else
            {
                Debug.Log("Player Not Found");
            }
        }
        public int GetUserByEndPoint(EndPoint endPoint)
        {
            return server_ConnectedUsers.ContainsKey(endPoint) ? server_ConnectedUsers[endPoint] : -1;
        }
        
        #endregion
    }
}


