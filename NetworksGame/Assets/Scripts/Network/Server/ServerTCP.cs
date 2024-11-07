using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class ServerTCP : MonoBehaviour
{
    Socket socket;
    Thread mainThread = null;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;
    string serverText;

    public struct User
    {
        public string name;
        public Socket socket;
        public bool firstConnection;
    }

    List<User> users = new List<User>();

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
    }


    void Update()
    {
        UItext.text = serverText;

    }


    public void startServer()
    {
        serverText = "Starting TCP Server...";

        //TO DO 1
        //Create and bind the socket
        //Any IP that wants to connect to the port 9050 with TCP, will communicate with this socket
        //Don't forget to set the socket in listening mode
        try
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            socket.Bind(ipep);
            socket.Listen(10);
        }
        catch
        {
            Debug.Log("Error starting server");
        }

        //TO DO 3
        //TIme to check for connections, start a thread using CheckNewConnections
        mainThread = new Thread(CheckNewConnections);
        mainThread.Start();
        
    }

    void CheckNewConnections()
    {
        while (true)
        {
            User newUser = new User();
            newUser.firstConnection = true;
            //TO DO 3
            //TCP makes it so easy to manage conections, so we are going
            //to put it to use
            //Accept any incoming clients and store them in this user.
            //When accepting, we can now store a copy of our server socket
            //who has established a communication between a
            //local endpoint (server) and the remote endpoint(client)
            //If you want to check their ports and adresses, you can acces
            //the socket's RemoteEndpoint and LocalEndPoint
            //try printing them on the console

            newUser.socket = socket.Accept();//accept the socket

            IPEndPoint clientep = (IPEndPoint)newUser.socket.RemoteEndPoint;
            serverText = serverText + "\n" + "Connected with " + clientep.Address.ToString() + " at port " + clientep.Port.ToString();

            //TO DO 5
            //For every client, we call a new thread to receive their messages. 
            //Here we have to send our user as a parameter so we can use it's socket.
            Thread newConnection = new Thread(() => Receive(newUser));
            newConnection.Start();
        }
        //This users could be stored in the future on a list
        //in case you want to manage your connections

    }

    void Receive(User user)
    {
        byte[] data = new byte[1024];
        int recv = 0;
        //TO DO 5
        //Create an infinite loop to start receiving messages for this user
        //You'll have to use the socket function receive to be able to get them.

        while (true)
        {
            data = new byte[1024];
            try
            {
                recv = user.socket.Receive(data);
                if (recv == 0)
                {
                    RemoveUser(user);
                    break;
                }

                foreach (User userItem in users)
                {
                    if (userItem.socket.RemoteEndPoint.ToString() == user.socket.RemoteEndPoint.ToString())
                    {
                        user = userItem;
                        user.firstConnection = false;
                        break;
                    }
                }

                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                if (user.firstConnection)
                {
                    user.name = receivedMessage;
                    
                    serverText += $"\n{user.name} joined the server called TCP Server!";

                    // Send a response
                    Thread serverAnswer = new Thread(() => Send(user, ""));
                    serverAnswer.Start();

                    foreach (User userItem in users)
                    {
                        Thread answer = new Thread(() => Send(user, "New User connected: " + user.name));
                        answer.Start();
                    }
                    user.firstConnection = false;
                    users.Add(user);
                }
                {
                    serverText += $"\n{user.name}: {receivedMessage}";
                    foreach (User userItem in users)
                    {
                        if (userItem.socket.RemoteEndPoint.ToString() == user.socket.RemoteEndPoint.ToString())
                        {
                            Thread answer = new Thread(() => Send(userItem, "You" + ": " + receivedMessage));
                            answer.Start();
                        }
                        else
                        {
                            Thread answer = new Thread(() => Send(userItem, user.name + ": " + receivedMessage));
                            answer.Start();
                        }

                    }
                }
            }
            catch (SocketException ex)
            {
                Debug.Log($"Receive error: {ex.Message}");
                RemoveUser(user);
                break;
            }
        }
    }

    //TO DO 6
    //Now, we'll use this user socket to send a "ping".
    //Just call the socket's send function and encode the string.
    void Send(User user, string message)
    {
        byte[] data = new byte[1024];

        data = Encoding.ASCII.GetBytes(message);

        try
        {
            user.socket.Send(data);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Send error: {ex.Message}");
        }
    }

    void RemoveUser(User user)
    {
        // Remove the user from the list and close their
        users.Remove(user);
        try
        {
            user.socket.Shutdown(SocketShutdown.Both);
            user.socket.Close();
        }
        catch (SocketException ex)
        {
            Debug.Log($"Error while closing socket: {ex.Message}");
        }
        serverText += $"\n{user.name} has left the server.";
    }
}
