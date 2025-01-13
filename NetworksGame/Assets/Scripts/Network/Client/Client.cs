using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;


namespace HyperStrike
{
    public class Client : MonoBehaviour
    {
        private DeliveryNotificationSystem deliverySystem = new DeliveryNotificationSystem();
        private float packetTimeout = 1.0f; // 1 second timeout

        float sendInterval = 0.01f; // 10 ms interval
        Thread receive;

        public bool StartClient(string username, string hostIp = "127.0.0.1")
        {
            try
            {
                // IP EndPoint Default to Local: "127.0.0.1" Port: 9050
                NetworkManager.Instance.nm_ServerEndPoint = new IPEndPoint(IPAddress.Parse(hostIp), 9050);

                // Detect if the user is not waiting and in the Menu (or Finished Match?)
                if (!NetworkManager.Instance.nm_Connected)
                {
                    NetworkManager.Instance.nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    NetworkManager.Instance.nm_Socket.Connect(NetworkManager.Instance.nm_ServerEndPoint);

                    // Send a ping message to the host
                    byte[] pingMessage = System.Text.Encoding.UTF8.GetBytes("PING");
                    NetworkManager.Instance.nm_Socket.Send(pingMessage);

                    // Wait for a response with a timeout
                    byte[] buffer = new byte[1024];
                    NetworkManager.Instance.nm_Socket.ReceiveTimeout = 2000; // Set timeout to 2 seconds
                    EndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
                    int receivedBytes = NetworkManager.Instance.nm_Socket.ReceiveFrom(buffer, ref endpoint);

                    string response = System.Text.Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                    if (response == "PONG")
                    {
                        NetworkManager.Instance.nm_StatusText += $"\nPong received {response}";
                        NetworkManager.Instance.nm_StatusText += "\nConnected to the host";
                        NetworkManager.Instance.nm_Connected = true;
                        return true;
                    }
                }
            }
            catch (SocketException ex)
            {
                NetworkManager.Instance.nm_StatusText += $"\nError connecting to the server: {ex.Message}";
                return false;
            }
            NetworkManager.Instance.nm_StatusText += $"\nError connecting to the server!";
            return false;
        }

        public void SetClient(string username)
        {
            int userID = IDGenerator.GenerateID();

            NetworkManager.Instance.SetNetPlayer(username, userID);

            NetworkManager.Instance.nm_StatusText += $"\nPlayer {username} with ID {userID} created";

            NetworkManager.Instance.nm_Match = GameObject.Find("MatchManager").GetComponent<Match>();
            NetworkManager.Instance.nm_Ball = GameObject.Find("Ball").GetComponent<BallController>();

            StartCoroutine(SendPacketsWithDelay());

            //We'll wait for a server response,
            receive = new Thread(ReceiveClient);
            receive.Start();
        }

        private void SendClientPacket()
        {
            byte[] clientPacket = new byte[1024];
            int packetId = deliverySystem.GeneratePacketId();

            using (MemoryStream memoryStream = new MemoryStream())
            {
                //memoryStream.Write(BitConverter.GetBytes(packetId), 0, sizeof(int)); // Add Packet ID

                int id = NetworkManager.Instance.nm_PlayerData.PlayerId;

                var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(id) 
                    ? NetworkManager.Instance.nm_LastPlayerStates[id] 
                    : new PlayerDataPacket();

                byte[] playerData = NetworkManager.Instance.nm_PlayerScript.Packet.Serialize(lastState);

                memoryStream.Write(playerData, 0, playerData.Length);

                MemoryStream projectilesStream = new MemoryStream();
                if (NetworkManager.Instance.nm_ProjectilesToSend.Count > 0)
                {
                    foreach (KeyValuePair<int, Projectile> pr in NetworkManager.Instance.nm_ProjectilesToSend)
                    {
                        var lastStateProjectile = new ProjectilePacket();

                        byte[] projectilePacket = pr.Value.Packet.Serialize(lastStateProjectile);
                        if (memoryStream.Length + projectilesStream.Length + projectilePacket.Length > 1024)
                        {
                            Debug.LogWarning($"Skipping projectile {pr.Key} due to packet size limit.");
                            break;
                        }
                        projectilesStream.Write(projectilePacket, 0, projectilePacket.Length);
                    }
                }
                
                NetworkManager.Instance.nm_ProjectilesToSend.Clear();
                // Write player data to the main packet
                byte[] projectilesData = projectilesStream.ToArray();
                if (memoryStream.Length + projectilesData.Length > 1024)
                {
                    Debug.LogWarning("Projectiles data exceeds packet size.");
                }
                memoryStream.Write(projectilesData, 0, projectilesData.Length);

                clientPacket = memoryStream.ToArray();
                deliverySystem.RegisterSentPacket(packetId, Time.time);
            }
            NetworkManager.Instance.nm_Socket.SendTo(clientPacket, NetworkManager.Instance.nm_ServerEndPoint);
        }

        private IEnumerator SendPacketsWithDelay()
        {
            while (NetworkManager.Instance.nm_Connected)
            {
                SendClientPacket();
                yield return new WaitForSeconds(sendInterval); // Wait before sending the next packet
            }
        }
        
        void ReceiveClient()
        {
            byte[] data = new byte[1024];
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            while (true)
            {
                int recv = NetworkManager.Instance.nm_Socket.ReceiveFrom(data, ref Remote);

                if (recv == 0) continue;

                int packetId = BitConverter.ToInt32(data, 0); // Extract Packet ID
                deliverySystem.AcknowledgePacket(packetId);

                byte[] packetData = data.Skip(sizeof(int)).ToArray(); // Remove Packet ID
                NetworkManager.Instance.HandlePacket(data, out _); // Discard out parameter
            }
        }

        private void OnApplicationQuit()
        {
            if (receive != null && receive.IsAlive)
            {
                receive.Interrupt();
            }
        }
    }

}

