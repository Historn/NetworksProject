using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

namespace HyperStrike
{
    public class Server : MonoBehaviour
    {
        Socket server_Socket;

        User server_User;
        List<User> server_ConnectedUsers = new List<User>(); // Change to a Dictionary based on the Endpoint, User
        //Dictionary<EndPoint, User> server_ConnectedUsers;


        PlayerData server_ReceivedPlayerData;

        public void StartHost(string username)
        {
            NetworkManager.instance.nm_StatusText += "Creating Host Server...";

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            server_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server_Socket.Bind(ipep);

            // Create Host User as first user connected
            server_User = new User();
            server_User.userId = 0; // Is the first user connected
            server_User.name = username; // Get from input
            server_User.endPoint = NetworkManager.instance.nm_ServerEndPoint;
            server_User.firstConnection = false;
            server_ConnectedUsers.Add(server_User);

            NetworkManager.instance.nm_PlayerData.playerId = 0;
            NetworkManager.instance.nm_PlayerData.playerName = username;

            NetworkManager.instance.nm_StatusText += $"Host User created with name {username}";

            Thread mainThread = new Thread(ReceiveHost);
            mainThread.Start();
        }

        void SendHost(EndPoint Remote, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                server_Socket.SendTo(data, Remote);
            }
            catch (SocketException ex)
            {
                Debug.Log($"Send host error: {ex.Message}");
            }
        }

        void ReceiveHost()
        {

            // TENGO QUE AÑADIR ALGO PARA CAMBIAR EL PLAYER AL CARGAR ESCENA

            byte[] data = new byte[1024];
            int recv = 0;

            NetworkManager.instance.nm_StatusText += "\nWaiting for new Client...";
            Debug.Log(NetworkManager.instance.nm_StatusText);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            while (true)
            {
                User newUser = new User();
                newUser.firstConnection = true;
                newUser.playerData = new PlayerData();

                data = new byte[1024];
                recv = server_Socket.ReceiveFrom(data, ref Remote);
                string receivedJson = Encoding.ASCII.GetString(data, 0, recv);
                Debug.Log("Received: " + receivedJson);

                foreach (User user in server_ConnectedUsers)
                {
                    if (user.endPoint.ToString() == Remote.ToString())
                    {
                        newUser = user;
                        newUser.firstConnection = false;
                        break;
                    }
                }

                if (newUser.firstConnection)
                {
                    JsonUtility.FromJsonOverwrite(receivedJson, newUser.playerData);
                    newUser.name = newUser.playerData.playerName;
                    newUser.userId = server_ConnectedUsers.Count;
                    newUser.endPoint = Remote;
                    newUser.playerData.playerId = server_ConnectedUsers.Count;

                    server_ReceivedPlayerData = newUser.playerData;

                    MainThreadInvoker.Invoke(() =>
                    {
                        NetworkManager.instance.InstatiateGO(newUser.playerData);
                    });

                    NetworkManager.instance.nm_StatusText += $"\n{newUser.name} joined the server called UDP Server";

                    // CHANGE TO SEND THE HOST USER INFO FIRST + INSTANCE GENERATED
                    string packet = JsonUtility.ToJson(NetworkManager.instance.nm_PlayerData); // I need to set playerData before send it
                    Thread serverAnswer = new Thread(() => SendHost(Remote, packet));
                    serverAnswer.Start();

                    string packetPlayerData = JsonUtility.ToJson(newUser.playerData);

                    foreach (User user in server_ConnectedUsers)
                    {
                        Thread answer = new Thread(() => SendHost(user.endPoint, packetPlayerData));
                        answer.Start();
                    }
                    newUser.firstConnection = false;
                    server_ConnectedUsers.Add(newUser);
                }
                else if (newUser.endPoint.ToString() != server_User.endPoint.ToString())
                {
                    HandlePlayerData(newUser, receivedJson, Remote);
                }
            }
        }

        void HandlePlayerData(User user, string jsonData, EndPoint Remote)
        {
            JsonUtility.FromJsonOverwrite(jsonData, user.playerData);
            NetworkManager.instance.nm_StatusText = $"\nReceived data from {user.name}_{user.userId} : {user.playerData.position[0]}, {user.playerData.position[1]}, {user.playerData.position[2]}";

            MainThreadInvoker.Invoke(() =>
            {
                GameObject go = GameObject.Find(user.playerData.playerName);

                if (go != null)
                {
                    Player p = go.GetComponent<Player>();
                    p.updateGO = true;
                    p.playerData = user.playerData;
                    Debug.Log("GO FOUND: " + p.playerData.position[0] + " " + p.playerData.position[1] + " " + p.playerData.position[2]);
                }
            });

            // Broadcast the updated player position to all clients
            foreach (User u in server_ConnectedUsers)
            {
                if (u.endPoint.ToString() != server_User.endPoint.ToString())
                {
                    string playerJson = JsonUtility.ToJson(NetworkManager.instance.nm_PlayerData);
                    Thread sendThread = new Thread(() => SendHost(u.endPoint, playerJson));
                    sendThread.Start();
                }

                // Esto hace lo mismo que el de arriba?
                if (u.endPoint.ToString() != Remote.ToString())
                {
                    string playerJson = JsonUtility.ToJson(user.playerData);
                    Thread sendThread = new Thread(() => SendHost(u.endPoint, playerJson));
                    sendThread.Start();
                }
            }
        }
    }
}


