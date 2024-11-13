using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Unity.VisualScripting;
using UnityEditor.VersionControl;

namespace HyperStrike
{
    public class NetworkManager : MonoBehaviour
    {
        public struct User
        {
            public string username;
            public EndPoint endPoint;
            public bool firstConnection;
        }

        // Network Threads
        Thread nm_MainNetworkThread;
        Thread nm_mainUDPThread; // Used for player data
        Thread nm_mainTCPThread; // Used for chat data?

        Socket nm_Socket;
        User nm_User;
        List<User> nm_ConnectedUsers = new List<User>();

        string nm_StatusText;
        bool connected = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void StartClient()
        {
            nm_MainNetworkThread = new Thread(SendClient);
            nm_MainNetworkThread.Start();
        }

        public void StartHost()
        {
            nm_StatusText = "Creating Host Server...";

            //UDP doesn't keep track of our connections like TCP
            //This means that we "can only" reply to other endpoints,
            //since we don't know where or who they are
            //We want any UDP connection that wants to communicate with 9050 port to send it to our socket.
            //So as with TCP, we create a socket and bind it to the 9050 port. 

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            nm_Socket.Bind(ipep);

            //Our client is sending a handshake, the server has to be able to recieve it
            //It's time to call the Receive thread
            Thread newConnection = new Thread(ReceiveHost);
            newConnection.Start();
        }

        // First add just UDP for gameplay
        void SendHost(EndPoint Remote)
        {
            //Use socket.SendTo to send a ping using the remote we stored earlier.
            byte[] data = Encoding.ASCII.GetBytes("Send Host Message");

            try
            {
                nm_Socket.SendTo(data, Remote);
            }
            catch (SocketException ex)
            {
                Debug.Log($"Send host error: {ex.Message}");
            }
        }

        void SendClient()
        {
            try
            {
                // IP EndPoint Default to Local: "127.0.0.1" Port: 9050
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

                // Detect if the user is not waiting and in the Menu (or Finished Match?)
                if (!(GameManager.gm_GameState == GameState.WAITING_ROOM) && GameManager.gm_GameState == GameState.MENU)
                {
                    //Unlike with TCP, we don't "connect" first,
                    //we are going to send a message to establish our communication so we need an endpoint
                    //We need the server's IP and the port we've binded it to before
                    //Again, initialize the socket

                    nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    nm_Socket.Connect(ipep);
                    nm_StatusText += "\nConnected to the host";

                    //Send the Handshake to the server's endpoint.
                    //This time, our UDP socket doesn't have it, so we have to pass it
                    //as a parameter on it's SendTo() method
                    nm_Socket.SendTo(Encoding.ASCII.GetBytes(nm_User.username + ": Connected"), ipep);

                    //We'll wait for a server response,
                    //so you can already start the receive thread
                    Thread receive = new Thread(ReceiveHost);
                    receive.Start();

                    connected = true;
                }
                else
                {
                    //nm_Socket.SendTo(Encoding.ASCII.GetBytes(UiInputMessage.text), ipep); // Send packages with player data, events, etc.
                }

            }
            catch (SocketException ex)
            {
                nm_StatusText += $"\nError sending message to the server: {ex.Message}";
            }
        }

        void ReceiveHost()
        {
            //byte[] data = new byte[1024];
            //int recv = 0;

            //nm_StatusText += "\n" + "Waiting for new Client...";

            ////TO DO 3
            ////We don't know who may be comunicating with this server, so we have to create an
            ////endpoint with any address and an IpEndpoint from it to reply to it later.
            //IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //EndPoint Remote = (EndPoint)(sender);

            ////Loop the whole process, and start receiveing messages directed to our socket
            ////(the one we binded to a port before)
            ////When using socket.ReceiveFrom, be sure send our remote as a reference so we can keep
            ////this adress (the client) and reply to it on TO DO 4
            //while (true)
            //{
            //    User newUser = new User();
            //    newUser.firstConnection = true;

            //    data = new byte[1024];
            //    recv = socket.ReceiveFrom(data, ref Remote);
            //    string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

            //    foreach (User user in users)
            //    {
            //        if (user.endPoint.ToString() == Remote.ToString())
            //        {
            //            newUser = user;
            //            newUser.firstConnection = false;
            //            break;
            //        }
            //    }

            //    if (newUser.firstConnection)
            //    {
            //        newUser.name = receivedMessage;
            //        newUser.endPoint = Remote;
            //        serverText += $"\n{newUser.name} joined the server called UDP Server";
            //        //TO DO 4
            //        //When our UDP server receives a message from a random remote, it has to send a ping,
            //        //Call a send thread
            //        Thread serverAnswer = new Thread(() => Send(Remote, "Welcome to the UDP Server: " + newUser.name));
            //        serverAnswer.Start();

            //        foreach (User user in users)
            //        {
            //            Thread answer = new Thread(() => Send(user.endPoint, "New User connected: " + newUser.name));
            //            answer.Start();
            //        }
            //        newUser.firstConnection = false;
            //        users.Add(newUser);
            //    }
            //    else
            //    {
            //        serverText += $"\n{newUser.name}: {receivedMessage}";
            //        foreach (User user in users)
            //        {
            //            if (user.endPoint.ToString() == Remote.ToString())
            //            {
            //                Thread answer = new Thread(() => Send(user.endPoint, "You" + ": " + receivedMessage));
            //                answer.Start();
            //            }
            //            else
            //            {
            //                Thread answer = new Thread(() => Send(user.endPoint, newUser.name + ": " + receivedMessage));
            //                answer.Start();
            //            }

            //        }
            //    }
            //}
        }

        void ReceiveClient()
        {
            byte[] data = new byte[1024];
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = nm_Socket.ReceiveFrom(data, ref Remote);

                // Add deserialization process
                //nm_StatusText += "\n" + Encoding.ASCII.GetString(data, 0, recv);
            }
        }
    }
}