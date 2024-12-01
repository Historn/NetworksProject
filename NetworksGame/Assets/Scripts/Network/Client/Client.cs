using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using UnityEngine.SceneManagement;
using System;

namespace HyperStrike
{
    public class Client : MonoBehaviour
    {
        Socket client_Socket;

        void Update()
        {
            // Capture player data on the main thread
            if (NetworkManager.instance.nm_PlayerData != null && NetworkManager.instance.nm_Connected)
            {
                // Use the captured data in a background thread
                Thread sendThread = new Thread(() => SendClient());
                sendThread.Start();
            }
        }

        public void StartClient(string username)
        {
            NetworkManager.instance.SetNetPlayer(username);

            NetworkManager.instance.nm_PlayerData.playerId = -1;
            NetworkManager.instance.nm_PlayerData.playerName = username;

            Thread mainThread = new Thread(SendClient);
            mainThread.Start();
        }

        void SendClient()
        {
            try
            {
                // IP EndPoint Default to Local: "127.0.0.1" Port: 9050
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

                // Detect if the user is not waiting and in the Menu (or Finished Match?)
                if (!NetworkManager.instance.nm_Connected)
                {
                    client_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    client_Socket.Connect(ipep);
                    NetworkManager.instance.nm_StatusText += "\nConnected to the host";

                    SendPlayerData();

                    NetworkManager.instance.nm_Connected = true;

                    //We'll wait for a server response,
                    //Maybe first receive Users List then start the receive loop
                    Thread receive = new Thread(ReceiveClient);
                    receive.Start();
                }
                else
                {
                    Debug.Log("Sending Player Updated Data");
                    // Send player data info using JSON serialization
                    SendPlayerData();
                }
            }
            catch (SocketException ex)
            {
                NetworkManager.instance.nm_StatusText += $"\nError sending message to the server: {ex.Message}";
            }
        }

        private void SendPlayerData()
        {
            string jsonData = JsonUtility.ToJson(NetworkManager.instance.nm_PlayerData);

            // Send JSON string as bytes
            byte[] data = Encoding.UTF8.GetBytes(jsonData);
            client_Socket.SendTo(data, NetworkManager.instance.nm_ServerEndPoint);
        }

        void ReceiveClient()
        {
            Debug.Log("Client receiving");
            byte[] data = new byte[1024];
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = client_Socket.ReceiveFrom(data, ref Remote);
                string receivedJson = Encoding.ASCII.GetString(data, 0, recv);

                Debug.Log("Packet received " + receivedJson);

                PlayerData pData = new PlayerData();
                JsonUtility.FromJsonOverwrite(receivedJson, pData);
                NetworkManager.instance.nm_StatusText = $"\nReceived data from {pData.playerName}: {pData.position[0]}, {pData.position[1]}, {pData.position[2]}";

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
                        NetworkManager.instance.InstatiateGO(pData);
                    }
                });
            }
        }
    }

}

