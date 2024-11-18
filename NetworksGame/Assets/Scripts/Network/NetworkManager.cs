using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using TMPro;
using System.Linq;

namespace HyperStrike
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance { get; private set; }
        void Awake() { if (instance == null) instance = this; }

        Socket nm_Socket;

        //private UdpClient client;
        private IPEndPoint serverEndPoint;
        private IPEndPoint clientEndPoint;

        // Network Threads
        Thread nm_MainNetworkThread;
        Thread nm_mainUDPThread; // Used for player data
        Thread nm_mainTCPThread; // Used for chat data?

        User nm_User;
        List<User> nm_ConnectedUsers = new List<User>();
        bool instantiateNewPlayer = false;

        public GameObject UItextObj;
        TextMeshProUGUI UItext;
        string nm_StatusText;
        bool connected = false;

        [SerializeField]GameObject clientInstancePrefab;

        [SerializeField]GameObject playerPrefab;
        Player player;
        PlayerData receivedPlayerData;

        // Start is called before the first frame update
        void Start()
        {
            UItext = UItextObj.GetComponent<TextMeshProUGUI>();
            player = playerPrefab.GetComponent<Player>();
            serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
        }

        void Update()
        {
            UItext.text = nm_StatusText;

            // Capture player data on the main thread
            if (player != null && connected)
            {
                nm_User.playerData = player.playerData;
                nm_User.playerData.playerId = player.id;

                // Use the captured data in a background thread
                Thread sendThread = new Thread(() => SendClient());
                sendThread.Start();
            }

            if (instantiateNewPlayer)
            {
                //Bounds bounds = GameObject.Find("Ground").GetComponent<BoxCollider>().bounds;
                //GameObject instance = Instantiate(clientInstancePrefab, Spawner.RandomPointInBounds(bounds), new Quaternion(0, 0, 0, 1));
                GameObject go = Instantiate(clientInstancePrefab, new Vector3(0,0,3), new Quaternion(0, 0, 0, 1));
                go.GetComponent<Player>().playerData = receivedPlayerData;
                instantiateNewPlayer = false;
            }
        }

        public void StartHost(string username)
        {
            nm_StatusText += "Creating Host Server...";

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            nm_Socket.Bind(ipep);

            player.id = 0;
            player.name = username;

            // Create Host User as first user connected
            nm_User = new User();
            nm_User.userId = 0; // Is the first user connected
            nm_User.name = username; // Get from input
            nm_User.endPoint = serverEndPoint;
            nm_User.firstConnection = true;
            nm_User.playerData = player.playerData;
            nm_ConnectedUsers.Add(nm_User);

            nm_StatusText += "Host User created...";

            Thread newConnection = new Thread(ReceiveHost);
            newConnection.Start();
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

            nm_StatusText += "\nWaiting for new Client...";
            Debug.Log(nm_StatusText);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            while (true)
            {
                User newUser = new User();
                newUser.firstConnection = true;
                newUser.playerData = player.playerData;

                data = new byte[1024];
                recv = nm_Socket.ReceiveFrom(data, ref Remote);
                string receivedJson = Encoding.ASCII.GetString(data, 0, recv);
                Debug.Log("Rceived: "+receivedJson);

                foreach (User user in nm_ConnectedUsers)
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
                    newUser.userId = nm_ConnectedUsers.Count;
                    newUser.endPoint = Remote;
                    newUser.playerData.playerId = nm_ConnectedUsers.Count;

                    receivedPlayerData = newUser.playerData;

                    instantiateNewPlayer = true;

                    nm_StatusText += $"\n{newUser.name} joined the server called UDP Server";

                    // CHANGE TO SEND THE HOST USER INFO FIRST + INSTANCE GENERATED
                    string packet = JsonUtility.ToJson(nm_User.playerData);
                    Thread serverAnswer = new Thread(() => SendHost(Remote, packet));
                    serverAnswer.Start();

                    string packetPlayerData = JsonUtility.ToJson(newUser.playerData);

                    foreach (User user in nm_ConnectedUsers)
                    {
                        Thread answer = new Thread(() => SendHost(user.endPoint, packetPlayerData));
                        answer.Start();
                    }
                    newUser.firstConnection = false;
                    nm_ConnectedUsers.Add(newUser);
                }
                else if (newUser.endPoint.ToString() != nm_User.endPoint.ToString())
                {
                    HandlePlayerData(newUser, receivedJson, Remote);
                }
            }
        }

        void HandlePlayerData(User user, string jsonData, EndPoint Remote)
        {
            JsonUtility.FromJsonOverwrite(jsonData, user.playerData);
            nm_StatusText = $"\nReceived data from {user.name}: {user.playerData.position[0]}, {user.playerData.position[1]}, {user.playerData.position[2]}";
            Debug.Log(nm_StatusText);
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

        public void StartClient(string username)
        {
            player.id = -1;
            player.playerData.playerId = -1;
            player.name = username;
            player.playerData.playerName = username;
            //player.playerData.position[0] = 0;
            //player.playerData.position[1] = 0;
            //player.playerData.position[2] = 3;

            Debug.Log(player.name);

            //// Create Host User as first user connected
            //nm_User = new User();
            //nm_User.userId = 0;
            //nm_User.name = username; // Get from input
            //nm_User.firstConnection = true;
            //nm_User.playerData.playerName = username; // Take owner player
            //nm_User.playerData.playerId = nm_User.userId;

            nm_MainNetworkThread = new Thread(SendClient);
            nm_MainNetworkThread.Start();
        }

        void SendClient()
        {
            try
            {
                // IP EndPoint Default to Local: "127.0.0.1" Port: 9050
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

                // Detect if the user is not waiting and in the Menu (or Finished Match?)
                if (!connected)
                {
                    nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    nm_Socket.Connect(ipep);
                    nm_StatusText += "\nConnected to the host";

                    string packet = JsonUtility.ToJson(player.playerData);
                    byte[] data = Encoding.ASCII.GetBytes(packet);
                    //Send the Handshake to the server's endpoint.
                    nm_Socket.SendTo(data, ipep);

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
            string jsonData = JsonUtility.ToJson(nm_User.playerData);

            // Send JSON string as bytes
            byte[] data = Encoding.UTF8.GetBytes(jsonData);
            nm_Socket.SendTo(data, serverEndPoint);
        }

        void ReceiveClient()
        {
            byte[] data = new byte[1024];
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = nm_Socket.ReceiveFrom(data, ref Remote);
                string receivedJson = Encoding.ASCII.GetString(data, 0, recv);

                PlayerData pData = new PlayerData(player);
                JsonUtility.FromJsonOverwrite(receivedJson, pData);

                GameObject go = GameObject.Find(pData.playerName);

                if (go != null) 
                {
                    Player p = go.GetComponent<Player>();
                    p.updateGO = true;
                    p.playerData = pData;
                }
                else
                {
                    instantiateNewPlayer = true;
                    receivedPlayerData = pData;
                }
            }
        }
    }
}