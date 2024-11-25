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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // Capture player data on the main thread
        if (player != null && connected)
        {
            // Use the captured data in a background thread
            Thread sendThread = new Thread(() => SendClient());
            sendThread.Start();
        }
    }

    public void StartClient(string username)
    {
        player.playerData.playerId = -1;
        player.playerData.playerName = username;

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

                SendPlayerData();

                //We'll wait for a server response,
                //so you can already start the receive thread
                Thread receive = new Thread(ReceiveClient);
                receive.Start();

                connected = true;
                creatingPlayer = true;
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
        string jsonData = JsonUtility.ToJson(player.playerData);

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

            PlayerData pData = new PlayerData();
            JsonUtility.FromJsonOverwrite(receivedJson, pData);
            nm_StatusText = $"\nReceived data from {pData.playerName}: {pData.position[0]}, {pData.position[1]}, {pData.position[2]}";

            MainThreadInvoker.Invoke(() =>
            {
                GameObject go = GameObject.Find(pData.playerName);
                Debug.Log("GO FOUND: " + pData.playerName);

                if (go != null)
                {
                    Player p = go.GetComponent<Player>();
                    p.updateGO = true;
                    p.playerData = pData;
                    Debug.Log("GO FOUND: " + p.playerData.playerName);
                }
                else
                {
                    InstatiateGO(pData);
                }
            });
        }
    }

    void InstatiateGO(PlayerData data)
    {
        GameObject goInstance = Instantiate(clientInstancePrefab, new Vector3(0, 0, 3), new Quaternion(0, 0, 0, 1));
        goInstance.name = data.playerName;
        goInstance.GetComponent<Player>().playerData = data;
        Debug.Log("GO CREATED: " + data.playerName);
    }
}

