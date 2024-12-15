using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Unity.VisualScripting;

namespace HyperStrike
{
    public class Client : MonoBehaviour
    {
        void Update()
        {
            // CHANGE FIRST CONDITIONAL?
            if (NetworkManager.Instance.nm_PlayerData != null && NetworkManager.Instance.nm_Connected)
            {
                Thread sendThread = new Thread(() => SendClient());
                sendThread.Start();
            }
        }

        public void StartClient(string username)
        {
            NetworkManager.Instance.SetNetPlayer(username);

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
                if (!NetworkManager.Instance.nm_Connected)
                {
                    NetworkManager.Instance.nm_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    NetworkManager.Instance.nm_Socket.Connect(ipep);
                    NetworkManager.Instance.nm_StatusText += "\nConnected to the host";

                    ClientPacket(); // MAYBE CHANGE FOR FIRST CONNECTION?

                    NetworkManager.Instance.nm_Connected = true;

                    //We'll wait for a server response,
                    //Maybe first receive Users List then start the receive loop
                    Thread receive = new Thread(ReceiveClient);
                    receive.Start();
                }
                else
                {
                    Debug.Log("Sending Player Updated Data");
                    
                    ClientPacket();
                }
            }
            catch (SocketException ex)
            {
                NetworkManager.Instance.nm_StatusText += $"\nError sending message to the server: {ex.Message}";
            }
        }

        private void ClientPacket()
        {
            byte[] clientPacket = new byte[1024];

            // WE WANT TO SEND THIS CLIENT
            using (MemoryStream memoryStream = new MemoryStream())
            {


                clientPacket = memoryStream.ToArray();
            }

            NetworkManager.Instance.nm_Socket.SendTo(clientPacket, NetworkManager.Instance.nm_ServerEndPoint);
        }

        void ReceiveClient()
        {
            Debug.Log("Client receiving");
            byte[] data = new byte[1024];
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = NetworkManager.Instance.nm_Socket.ReceiveFrom(data, ref Remote);
                string receivedJson = Encoding.ASCII.GetString(data, 0, recv);

                Debug.Log("Packet received " + receivedJson);

                PlayerDataPacket pData = new PlayerDataPacket();
                JsonUtility.FromJsonOverwrite(receivedJson, pData);
                NetworkManager.Instance.nm_StatusText = $"\nReceived data from {pData.PlayerName}: {pData.Position[0]}, {pData.Position[1]}, {pData.Position[2]}";

                // CHANGE TO HANDLE PACKET RECEIVED
                MainThreadInvoker.Invoke(() =>
                {
                    GameObject go = GameObject.Find(pData.PlayerName);
                    Debug.Log("GO FOUND: " + pData.PlayerName);

                    if (go != null)
                    {
                        Player p = go.GetComponent<Player>();
                        p.updateGO = true;
                        p.Packet = pData;
                        Debug.Log("GO FOUND: " + p.Packet.PlayerName);
                    }
                    else
                    {
                        NetworkManager.Instance.InstatiateGO(pData);
                    }
                });
            }
        }

        #region REPLICATION

        private void HandlePacket(byte[] receivedData)
        {
            if (receivedData == null || receivedData.Length == 0)
            {
                throw new ArgumentException("Received data is empty or null.");
            }

            // Separate the data into sections
            (byte[] gameStateData, byte[] playerData, byte[] projectileData) = SeparateDataSections(receivedData);

            // Handle game state data
            HandleGameStateData(gameStateData);

            // Handle player data
            HandlePlayerData(playerData);

            // Handle projectile data
            HandleProjectileData(projectileData);
        }

        private (byte[], byte[], byte[]) SeparateDataSections(byte[] data)
        {
            // Assuming the data format has a predefined structure
            int gameStateSectionStart = 0; // Starting index for game state data
            int gameStateSectionLength = BitConverter.ToInt32(data, gameStateSectionStart); // First 4 bytes define the length
            int playerSectionStart = gameStateSectionStart + 4 + gameStateSectionLength;
            int playerSectionLength = BitConverter.ToInt32(data, playerSectionStart); // Next 4 bytes define the player data length
            int projectileSectionStart = playerSectionStart + 4 + playerSectionLength;

            if (data.Length < projectileSectionStart)
            {
                throw new InvalidOperationException("Invalid data structure.");
            }

            byte[] gameStateData = data.Skip(4).Take(gameStateSectionLength).ToArray();
            byte[] playerData = data.Skip(playerSectionStart + 4).Take(playerSectionLength).ToArray();
            byte[] projectileData = data.Skip(projectileSectionStart).ToArray();

            return (gameStateData, playerData, projectileData);
        }

        private void HandleGameStateData(byte[] gameStateData)
        {
            // Process game state data here
            Console.WriteLine("Game state data processed.");
        }

        private void HandlePlayerData(byte[] playerData)
        {
            // Process each player packet
            while (playerData.Length > 0)
            {
                int playerId = BitConverter.ToInt32(playerData, 0); // Similar to ExtractProjectileId
                HandlePlayerDataPacket(playerId, playerData); // Custom method for processing a single player
                playerData = TrimProcessedData(playerData, playerId); // Remove processed player packet
            }
        }

        private void HandleProjectileData(byte[] projectileData)
        {
            // Process each projectile packet
            while (projectileData.Length > 0)
            {
                int projectileId = BitConverter.ToInt32(projectileData, 0);
                HandleProjectilePacket(projectileId, projectileData);
                projectileData = TrimProcessedData(projectileData, projectileId); // Remove processed projectile packet
            }
        }

        private void HandlePlayerDataPacket(int playerId, byte[] playerData)
        {
            var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(playerId) ? NetworkManager.Instance.nm_LastPlayerStates[playerId] : null;

            // Extract player-specific data
            PlayerDataPacket player = new PlayerDataPacket();
            player.Deserialize(playerData, lastState);

            // Update player state in the game
            var existingPlayer = NetworkManager.Instance.GetPlayerById(playerId);
            if (existingPlayer != null)
            {
                existingPlayer.Packet = player;
                existingPlayer.updateGO = true;
            }
            else
            {
                Console.WriteLine($"Player with ID {playerId} not found.");
            }

            NetworkManager.Instance.nm_LastPlayerStates[playerId] = player;
        }

        private void HandleProjectilePacket(int projectileId, byte[] projectileData)
        {
            var lastState = NetworkManager.Instance.nm_LastProjectileStates.ContainsKey(projectileId) ? NetworkManager.Instance.nm_LastProjectileStates[projectileId] : null;

            // Extract player-specific data
            ProjectilePacket projectile = new ProjectilePacket();
            projectile.Deserialize(projectileData, lastState);

            // Update player state in the game
            var existingProjectile = NetworkManager.Instance.GetProjectileById(projectileId);
            if (existingProjectile != null)
            {
                existingProjectile.Packet = projectile;
                existingProjectile.updateGO = true;
            }
            else
            {
                Console.WriteLine($"Projectile with ID {projectileId} not found.");
            }

            NetworkManager.Instance.nm_LastProjectileStates[projectileId] = projectile;
        }

        private byte[] TrimProcessedData(byte[] data, int processedId)
        {
            // Assuming fixed packet sizes or ability to determine processed packet length
            int processedPacketLength = CalculatePacketLength(data, processedId); // Implement based on your format
            return data.Skip(processedPacketLength).ToArray();
        }

        private int CalculatePacketLength(byte[] data, int processedId)
        {
            // Example logic for determining packet length based on the data structure
            // Assuming the first 4 bytes after the ID represent the packet length

            int idOffset = 4; // Offset where ID ends
            int lengthOffset = idOffset; // Position where length is stored

            if (data.Length < lengthOffset + 4)
            {
                throw new InvalidOperationException("Data is too short to determine packet length.");
            }

            // Extract the packet length (assuming 4-byte integer)
            int packetLength = BitConverter.ToInt32(data, lengthOffset);
            return packetLength;
        }

        #endregion
    }

}

