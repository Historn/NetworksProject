using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.IO;
using System;

namespace HyperStrike
{
    public class Server : MonoBehaviour
    {
        User server_User;
        Dictionary<EndPoint, User> server_ConnectedUsers = new Dictionary<EndPoint, User>();

        #region CONNECTION
        public void StartHost(string username)
        {
            NetworkManager.Instance.nm_StatusText += "Creating Host Server...";

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            NetworkManager.Instance.nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            NetworkManager.Instance.nm_Socket.Bind(ipep);

            // Create Host User as first user connected
            server_User = new User();
            server_User.userId = 0; // Is the first user connected
            server_User.name = username;
            server_User.firstConnection = false;

            server_ConnectedUsers.Add(NetworkManager.Instance.nm_ServerEndPoint, server_User);

            NetworkManager.Instance.SetNetPlayer(username, true);

            NetworkManager.Instance.nm_StatusText += $"\nHost User created with name {username}";

            NetworkManager.Instance.nm_Match = GameObject.Find("MatchManager").GetComponent<Match>();

            Thread mainThread = new Thread(ReceiveHost);
            mainThread.Start();
        }
        #endregion

        #region REPLICATION
        // HOST HAS TO SEND
        // GAME STATE + CLIENTS STATE + GENERAL NET OBJECTS WITH ITS ¿ACTIONS?

        void SendHost(EndPoint Remote, string message)
        {
            byte[] hostPacket = new byte[1024];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // First we want to send the Game State
                // Call Game State Serialize()
                //byte[] gameStateData = GameState.Serialize();
                //memoryStream.Write(gameStateData, 0, gameStateData.Length);

                // Serialize Host and Clients Player Data
                foreach (KeyValuePair<int, Player> p in NetworkManager.Instance.nm_ActivePlayers)
                {
                    var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(p.Value.Packet.PlayerId) ? NetworkManager.Instance.nm_LastPlayerStates[p.Value.Packet.PlayerId] : null;
                    byte[] playersPacket = p.Value.Packet.Serialize(lastState);

                    if (memoryStream.Length + playersPacket.Length <= 1024)
                    {
                        memoryStream.Write(playersPacket, 0, playersPacket.Length);
                    }
                    else
                    {
                        Console.WriteLine($"Client {p.Key} data exceeds packet size limit.");
                        break;
                    }
                }

                // Serialize Pool of Projectiles
                //foreach (KeyValuePair<int, Player> p in NetworkManager.instance.nm_ActivePlayers)
                //{
                //    var lastState = lastProjectileStates.ContainsKey(currentProjectState.ProjectileId) ? lastProjectileStates[currentState.ProjectileId] : null;
                //    hostPacket += p.Value.Packet.Serialize(lastState);
                //}

                // Finalize the packet
                hostPacket = memoryStream.ToArray();
            }

            try
            {
                NetworkManager.Instance.nm_Socket.SendTo(hostPacket, Remote);
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
                string receivedJson = Encoding.ASCII.GetString(data, 0, recv);
                //Debug.Log("Received: " + receivedJson);

                User newUser = GetUserByEndPoint(Remote);

                if (newUser == null) 
                {
                    newUser = new User();
                    newUser.firstConnection = true;
                    //newUser.playerData = new PlayerDataPacket();
                }
                
                // SET MAX 10 PLAYERS
                if (newUser.firstConnection)
                {
                    //JsonUtility.FromJsonOverwrite(receivedJson, newUser.playerData);
                    //newUser.name = newUser.playerData.PlayerName;
                    //newUser.userId = server_ConnectedUsers.Count;
                    //newUser.playerData.PlayerId = server_ConnectedUsers.Count;

                    //PlayerDataPacket server_ReceivedPlayerData = newUser.playerData;
                    PlayerDataPacket playerDataPacket = new PlayerDataPacket();

                    MainThreadInvoker.Invoke(() =>
                    {
                        NetworkManager.Instance.InstatiateGO(playerDataPacket);
                    });

                    NetworkManager.Instance.nm_StatusText += $"\n{newUser.name} joined the server called UDP Server";

                    // CHANGE TO SEND THE HOST USER INFO FIRST + INSTANCE GENERATED
                    string packet = JsonUtility.ToJson(NetworkManager.Instance.nm_PlayerData); // I need to set playerData before send it
                    Thread serverAnswer = new Thread(() => SendHost(Remote, packet));
                    serverAnswer.Start();

                    string packetPlayerData = JsonUtility.ToJson(playerDataPacket);

                    // Send new User to other Clients
                    foreach (KeyValuePair<EndPoint, User> user in server_ConnectedUsers)
                    {
                        if (NetworkManager.Instance.nm_Socket.LocalEndPoint.ToString() == user.Key.ToString())
                            continue;

                        Thread answer = new Thread(() => SendHost(user.Key, packetPlayerData));
                        answer.Start();
                    }

                    newUser.firstConnection = false;
                    server_ConnectedUsers.Add(Remote, newUser);
                }
                else
                {
                    HandlePlayerData(newUser, receivedJson, Remote);
                }
            }
        }

        void HandlePlayerData(User user, string jsonData, EndPoint Remote)
        {
            //JsonUtility.FromJsonOverwrite(jsonData, user.playerData);
            //NetworkManager.Instance.nm_StatusText = $"\nReceived data from {user.name}_{user.userId} : {user.playerData.Position[0]}, {user.playerData.Position[1]}, {user.playerData.Position[2]}";

            //MainThreadInvoker.Invoke(() =>
            //{
            //    GameObject go = GameObject.Find(user.playerData.PlayerName);

            //    if (go != null)
            //    {
            //        Player p = go.GetComponent<Player>();
            //        p.updateGO = true;
            //        p.Packet = user.playerData;
            //    }
            //});

            // Broadcast the updated player position to all clients
            foreach (KeyValuePair<EndPoint, User> u in server_ConnectedUsers)
            {
                if (NetworkManager.Instance.nm_Socket.LocalEndPoint.ToString() == u.Key.ToString())
                    continue;

                // SEND PACKETS

                string playerJson = JsonUtility.ToJson(NetworkManager.Instance.nm_PlayerData);
                Thread answer = new Thread(() => SendHost(u.Key, playerJson));
                answer.Start();
            }

        }
        public User GetUserByEndPoint(EndPoint endPoint)
        {
            return server_ConnectedUsers.ContainsKey(endPoint) ? server_ConnectedUsers[endPoint] : null;
        }
        
        #endregion
    }
}


