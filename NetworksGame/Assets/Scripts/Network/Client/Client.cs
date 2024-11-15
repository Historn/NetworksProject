using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class Client : MonoBehaviour
{
    Socket socket;
    string clientText;

    Thread mainThread;
    bool connected = false;
    bool waiting = false;

    private UdpClient client;
    private IPEndPoint serverEndPoint;
    private IPEndPoint clientEndPoint;

    public Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        mainThread = new Thread(Send);
        client = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050); // Set server IP and port
    }

    public void StartClient()
    {
        mainThread.Start();
    }

    void Update()
    {
        if (!mainThread.IsAlive && connected)
        {
            StartCoroutine(waiter());
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Thread sendThread = new Thread(Send);
            sendThread.Start();
        }
    }

    void Send()
    {
        try
        {
            if (!waiting)
            {
                //Unlike with TCP, we don't "connect" first,
                //we are going to send a message to establish our communication so we need an endpoint
                //We need the server's IP and the port we've binded it to before
                //Again, initialize the socket

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(serverEndPoint);
                clientText += "\nConnected to the server";

                //Send the Handshake to the server's endpoint.
                //This time, our UDP socket doesn't have it, so we have to pass it
                //as a parameter on it's SendTo() method
                socket.SendTo(Encoding.ASCII.GetBytes("UiInputUsername.text"), serverEndPoint);

                //We'll wait for a server response,
                //so you can already start the receive thread
                Thread receive = new Thread(Receive);
                receive.Start();

                connected = true;
            }
            else
            {
                //socket.SendTo(Encoding.ASCII.GetBytes(UiInputMessage.text), serverEndPoint);
                // Send player data info using JSON serialization
                SendPlayerData();
            }
        }
        catch (SocketException ex)
        {
            clientText += $"\nError sending message to the server: {ex.Message}";
        }
    }

    //Same as in the server, in this case the remote is a bit useless
    //since we already know it's the server who's communicating with us
    void Receive()
    {
        byte[] data = new byte[1024];
        while (true)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            int recv = socket.ReceiveFrom(data, ref Remote);

            clientText += "\n" + Encoding.ASCII.GetString(data, 0, recv);
        }
    }

    private void SendPlayerData()
    {
        //PlayerMovement movement = player.GetComponent<PlayerMovement>().gr;
        //bool isJumping = Input.GetKey(KeyCode.Space); // Example jump state
        //Vector3 pos = new Vector3(1,0,1);

        //// Create and serialize player data
        //PlayerData playerData = new PlayerData();
        //string jsonData = JsonUtility.ToJson(playerData);

        //// Send JSON string as bytes
        //byte[] buffer = Encoding.UTF8.GetBytes(jsonData);
        //client.Send(buffer, buffer.Length, serverEndPoint);
    }

    private void BeginReceive()
    {
        client.BeginReceive(OnReceive, null);
    }


    private void OnReceive(IAsyncResult ar)
    {
        byte[] data = client.EndReceive(ar, ref clientEndPoint);
        string jsonData = Encoding.UTF8.GetString(data);

        UpdateOtherPlayerPosition(jsonData);

        // Continue receiving
        BeginReceive();
    }

    private void UpdateOtherPlayerPosition(string jsonData)
    {
        // Deserialize JSON data to PlayerData object
        PlayerData otherPlayerData = JsonUtility.FromJson<PlayerData>(jsonData);

        // Acces to server users list
        //foreach (User u in users)
        //{

        //}

        // Use deserialized data to update opponent's transform
        // opponentTransform.position = new Vector3(otherPlayerData.posX, otherPlayerData.posY, otherPlayerData.posZ);

        // Handle jump state if needed
        // bool isOtherPlayerJumping = otherPlayerData.isJumping;
    }

    void OnDestroy()
    {
        client.Close();
    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(4);
        if (connected)
        {
            SceneManager.LoadScene("Exercise1_WaitingRoom");
            connected = false;
            waiting = true;
        }
    }
}

