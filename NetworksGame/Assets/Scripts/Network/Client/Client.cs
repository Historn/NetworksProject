using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace HyperStrike
{
    public class Client : MonoBehaviour
    {
        float sendInterval = 0.01f; // 10 ms interval
        Thread receive;

        public void StartClient(string username, string hostIp = "127.0.0.1")
        {
            try
            {
                // IP EndPoint Default to Local: "127.0.0.1" Port: 9050
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(hostIp), 9050);

                // Detect if the user is not waiting and in the Menu (or Finished Match?)
                if (!NetworkManager.Instance.nm_Connected)
                {
                    NetworkManager.Instance.nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    NetworkManager.Instance.nm_Socket.Connect(ipep);
                    NetworkManager.Instance.nm_StatusText += "\nConnected to the host";

                    NetworkManager.Instance.nm_Connected = true;
                }
            }
            catch (SocketException ex)
            {
                NetworkManager.Instance.nm_StatusText += $"\nError connecting to the server: {ex.Message}";
            }

            int userID = IDGenerator.GenerateID();

            NetworkManager.Instance.SetNetPlayer(username, userID);

            NetworkManager.Instance.nm_StatusText += $"\nPlayer {username} with ID {userID} created";

            NetworkManager.Instance.nm_Match = GameObject.Find("MatchManager").GetComponent<Match>();

            StartCoroutine(SendPacketsWithDelay());

            //We'll wait for a server response,
            receive = new Thread(ReceiveClient);
            receive.Start();
        }

        private void SendClientPacket()
        {
            byte[] clientPacket = new byte[1024];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                int id = NetworkManager.Instance.nm_PlayerData.PlayerId;

                var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(id) 
                    ? NetworkManager.Instance.nm_LastPlayerStates[id] 
                    : new PlayerDataPacket();

                byte[] playerData = NetworkManager.Instance.nm_ActivePlayers[id].Packet.Serialize(lastState);

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
            while (true)
            {
                byte[] data = new byte[1024];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = NetworkManager.Instance.nm_Socket.ReceiveFrom(data, ref Remote);

                if (recv == 0) continue;

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

