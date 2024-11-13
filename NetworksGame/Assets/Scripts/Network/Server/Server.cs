using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class Server : MonoBehaviour
{
    Socket socket;

    string serverText;

    public struct User
    {
        public string name;
        public EndPoint endPoint;
        public bool firstConnection;
        public PlayerData playerData;
    }

    List<User> users = new List<User>();

    void Start()
    {
    }

    public void startServer()
    {
        serverText = "Starting UDP Server...";

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    void Update()
    {
    }

    void Receive()
    {
        byte[] data = new byte[1024];
        int recv = 0;

        serverText += "\nWaiting for new Client...";

        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        while (true)
        {
            User newUser = new User();
            newUser.firstConnection = true;

            data = new byte[1024];
            recv = socket.ReceiveFrom(data, ref Remote);
            string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            foreach (User user in users)
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
                newUser.name = receivedMessage;
                newUser.endPoint = Remote;
                serverText += $"\n{newUser.name} joined the server called UDP Server";

                Thread serverAnswer = new Thread(() => Send(Remote, "Welcome to the UDP Server: " + newUser.name));
                serverAnswer.Start();

                foreach (User user in users)
                {
                    Thread answer = new Thread(() => Send(user.endPoint, "New User connected: " + newUser.name));
                    answer.Start();
                }
                newUser.firstConnection = false;
                users.Add(newUser);
            }
            else
            {
                HandlePlayerData(newUser, receivedMessage, Remote);
            }
        }

    }

    void HandlePlayerData(User user, string jsonData, EndPoint Remote)
    {
        PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);
        user.playerData = playerData;
        serverText += $"\nReceived position from {user.name}: {playerData.playerTransform.position.x}, {playerData.playerTransform.position.y}, {playerData.playerTransform.position.z}";

        // Broadcast the updated player position to all clients
        foreach (User u in users)
        {
            if (u.endPoint.ToString() != Remote.ToString())
            {
                string playerJson = JsonUtility.ToJson(user.playerData);
                Thread sendThread = new Thread(() => Send(u.endPoint, playerJson));
                sendThread.Start();
            }
        }
    }

    void Send(EndPoint Remote, string message)
    {
        byte[] data = Encoding.ASCII.GetBytes(message);

        try
        {
            socket.SendTo(data, Remote);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Send error: {ex.Message}");
        }
    }
}
