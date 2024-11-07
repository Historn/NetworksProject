using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ServerUDP : MonoBehaviour
{
    Socket socket;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    public struct User
    {
        public string name;
        public EndPoint endPoint;
        public bool firstConnection;
    }

    List<User> users = new List<User>();

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();

    }
    public void startServer()
    {
        serverText = "Starting UDP Server...";

        //TO DO 1
        //UDP doesn't keep track of our connections like TCP
        //This means that we "can only" reply to other endpoints,
        //since we don't know where or who they are
        //We want any UDP connection that wants to communicate with 9050 port to send it to our socket.
        //So as with TCP, we create a socket and bind it to the 9050 port. 

        IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(ipep);

        //TO DO 3
        //Our client is sending a handshake, the server has to be able to recieve it
        //It's time to call the Receive thread
        Thread newConnection = new Thread(Receive);
        newConnection.Start();
    }

    void Update()
    {
        UItext.text = serverText;

    }

 
    void Receive()
    {
        byte[] data = new byte[1024];
        int recv = 0;

        serverText += "\n" + "Waiting for new Client...";

        //TO DO 3
        //We don't know who may be comunicating with this server, so we have to create an
        //endpoint with any address and an IpEndpoint from it to reply to it later.
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
        EndPoint Remote = (EndPoint)(sender);

        //Loop the whole process, and start receiveing messages directed to our socket
        //(the one we binded to a port before)
        //When using socket.ReceiveFrom, be sure send our remote as a reference so we can keep
        //this adress (the client) and reply to it on TO DO 4
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
                //TO DO 4
                //When our UDP server receives a message from a random remote, it has to send a ping,
                //Call a send thread
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
                serverText += $"\n{newUser.name}: {receivedMessage}";
                foreach (User user in users) 
                {
                    if (user.endPoint.ToString() == Remote.ToString())
                    {
                        Thread answer = new Thread(() => Send(user.endPoint, "You" + ": " + receivedMessage));
                        answer.Start();
                    }
                    else
                    {
                        Thread answer = new Thread(() => Send(user.endPoint, newUser.name + ": " + receivedMessage));
                        answer.Start();
                    }
                    
                }
            }
        }

    }

    void Send(EndPoint Remote, string message)
    {
        //TO DO 4
        //Use socket.SendTo to send a ping using the remote we stored earlier.
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
