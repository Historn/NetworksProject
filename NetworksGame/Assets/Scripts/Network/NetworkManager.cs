using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;

namespace HyperStrike
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance { get; private set; }
        void Awake() { if (instance == null) instance = this; }

        Socket nm_Socket;

        private UdpClient client;
        private IPEndPoint serverEndPoint;
        private IPEndPoint clientEndPoint;

        // Network Threads
        Thread nm_MainNetworkThread;
        Thread nm_mainUDPThread; // Used for player data
        Thread nm_mainTCPThread; // Used for chat data?

        User nm_User;
        List<User> nm_ConnectedUsers = new List<User>();

        string nm_StatusText;
        bool connected = false;

        [SerializeField]GameObject clientInstancePrefab;
        [SerializeField]Player player;

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
        }

        public void StartHost()
        {
            nm_StatusText = "Creating Host Server...";

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            nm_Socket.Bind(ipep);

            Thread newConnection = new Thread(ReceiveHost);
            newConnection.Start();

            Debug.Log(nm_StatusText);

            // Create Host User as first user connected
            User newUser = new User();
            newUser.userId = 0; // Is the first user connected
            newUser.name = "Host"; // Get from input
            newUser.endPoint = serverEndPoint;
            newUser.firstConnection = true;
            newUser.playerData = new PlayerData(player); // Take owner player
            nm_ConnectedUsers.Add(newUser);
            nm_User = newUser;
        }

        void SendHost(EndPoint Remote, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                nm_Socket.SendTo(data, Remote);
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

            nm_StatusText = "Waiting for new Client...";
            Debug.Log(nm_StatusText);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            while (true)
            {
                User newUser = new User();
                newUser.firstConnection = true;

                data = new byte[1024];
                recv = nm_Socket.ReceiveFrom(data, ref Remote);
                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);

                foreach (User user in nm_ConnectedUsers)
                {
                    if (user.endPoint.ToString() == Remote.ToString())
                    {
                        newUser = user;
                        break;
                    }
                }

                if (newUser.firstConnection)
                {
                    Bounds bounds = GameObject.Find("Ground").GetComponent<BoxCollider>().bounds;
                    Instantiate(clientInstancePrefab, Spawner.RandomPointInBounds(bounds), new Quaternion(0,0,0,1));

                    // Take UserData sent by user
                    newUser.userId = nm_ConnectedUsers.Count;
                    newUser.name = receivedMessage;
                    newUser.endPoint = Remote;
                    nm_StatusText += $"\n{newUser.name} joined the server called UDP Server";

                    // CHANGE TO SEND THE HOST USER INFO FIRST + INSTANCE GENERATED
                    Thread serverAnswer = new Thread(() => SendHost(Remote, newUser.ToString()));
                    serverAnswer.Start();

                    foreach (User user in nm_ConnectedUsers)
                    {
                        // Send actual user packet
                        HandlePlayerData(newUser, receivedMessage, Remote);
                    }
                    newUser.firstConnection = false;
                    nm_ConnectedUsers.Add(newUser);
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
            nm_StatusText += $"\nReceived data from {user.name}: {playerData.position[0]}, {playerData.position[1]}, {playerData.position[2]}";

            // Broadcast the updated player position to all clients
            foreach (User u in nm_ConnectedUsers)
            {
                if (u.endPoint.ToString() != Remote.ToString())
                {
                    string playerJson = JsonUtility.ToJson(user.playerData);
                    Thread sendThread = new Thread(() => SendHost(u.endPoint, playerJson));
                    sendThread.Start();
                }
            }
        }

        public void StartClient()
        {
            nm_MainNetworkThread = new Thread(SendClient);
            nm_MainNetworkThread.Start();
        }

        void SendClient()
        {
            ///
            /// INTANCE AND SEND CLIENT
            ///
            try
            {
                // IP EndPoint Default to Local: "127.0.0.1" Port: 9050
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

                // Detect if the user is not waiting and in the Menu (or Finished Match?)
                if (!(GameManager.gm_GameState == GameState.WAITING_ROOM))
                {
                    nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    nm_Socket.Connect(ipep);
                    nm_StatusText += "\nConnected to the host";

                    //Send the Handshake to the server's endpoint.
                    nm_Socket.SendTo(Encoding.ASCII.GetBytes(nm_User.name + ": Connected"), ipep);

                    //We'll wait for a server response,
                    //so you can already start the receive thread
                    Thread receive = new Thread(ReceiveClient);
                    receive.Start();

                    connected = true;
                }
                else
                {
                    // Send player data info using JSON serialization
                    SendPlayerData();
                }
            }
            catch (SocketException ex)
            {
                nm_StatusText += $"\nError sending message to the server: {ex.Message}";
            }
        }

        private void SendPlayerData()
        {
            //PlayerMovement movement = player.GetComponent<PlayerMovement>().gr;
            //bool isJumping = Input.GetKey(KeyCode.Space); // Example jump state
            
            // Create and serialize player data
            PlayerData playerData = new PlayerData(player);
            string jsonData = JsonUtility.ToJson(playerData);

            // Send JSON string as bytes
            byte[] buffer = Encoding.UTF8.GetBytes(jsonData);
            client.Send(buffer, buffer.Length, serverEndPoint);
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
        //void OnDestroy()
        //{
        //    client.Close();
        //}
    }
}