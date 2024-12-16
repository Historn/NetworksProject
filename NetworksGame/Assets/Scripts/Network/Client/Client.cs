using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace HyperStrike
{
    public class Client : MonoBehaviour
    {
        float sendInterval = 0.01f;
        private bool isSendingPackets = false; // Track if the coroutine is running
        Thread receive;

        void Update()
        {
            //if (NetworkManager.Instance.nm_PlayerData != null && NetworkManager.Instance.nm_Connected)
            //{
            //    if (!isSendingPackets)
            //    {
            //        StartCoroutine(SendPacketsWithDelay()); // Send packet every 50ms
            //    }
            //}
        }

        public void StartClient(string username)
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

                    NetworkManager.Instance.nm_Connected = true;
                }
            }
            catch (SocketException ex)
            {
                NetworkManager.Instance.nm_StatusText += $"\nError connecting to the server: {ex.Message}";
            }

            int userID = PlayerIDGenerator.GeneratePlayerID();

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
            Debug.Log("Client Creating Packet to Send");

            byte[] clientPacket = new byte[1024];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Add a header for metadata 
                //int headerSize = 4; // 4 bytes for player state size change to 8 for +Projectiles
                //memoryStream.Position = headerSize;

                int id = NetworkManager.Instance.nm_PlayerData.PlayerId;
                var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(id) ? NetworkManager.Instance.nm_LastPlayerStates[id] : new PlayerDataPacket();
                byte[] playerData = NetworkManager.Instance.nm_ActivePlayers[id].Packet.Serialize(lastState);

                memoryStream.Write(playerData, 0, playerData.Length);

                //memoryStream.Position = 0;
                //byte[] playerDataSize = BitConverter.GetBytes(playerData.Length);
                //memoryStream.Write(playerDataSize, 0, playerDataSize.Length);

                clientPacket = memoryStream.ToArray();
            }

            NetworkManager.Instance.nm_Socket.SendTo(clientPacket, NetworkManager.Instance.nm_ServerEndPoint);
            Debug.Log("Client Packet Sent");
        }

        private IEnumerator SendPacketsWithDelay()
        {
            isSendingPackets = true;

            while (NetworkManager.Instance.nm_Connected)
            {
                SendClientPacket();
                yield return new WaitForSeconds(sendInterval); // Wait before sending the next packet
            }

            isSendingPackets = false; // Stop sending when disconnected
        }

        void ReceiveClient()
        {
            Debug.Log("Client receiving");
            while (true)
            {
                byte[] data = new byte[1024];
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);
                int recv = NetworkManager.Instance.nm_Socket.ReceiveFrom(data, ref Remote);

                if (recv == 0) continue;

                Debug.Log($"Client received a packet of {recv} bytes");

                HandlePacket(data);

                NetworkManager.Instance.nm_StatusText += $"\nReceived data from Host {NetworkManager.Instance.nm_LastMatchState.CurrentTime}";
            }
        }

        #region REPLICATION

        private void HandlePacket(byte[] receivedData)
        {
            // Separate the data into sections
            (byte[] matchStateData, byte[] playerData, byte[] projectileData) = SeparateDataSections(receivedData);

            // Handle game state data
            HandleMatchStateData(matchStateData);

            // Handle player data
            HandlePlayerData(playerData);

            // Handle projectile data
            HandleProjectileData(projectileData);
        }

        private (byte[], byte[], byte[]) SeparateDataSections(byte[] data)
        {
            // Assuming the data format has a predefined structure
            int matchStateSectionStart = 0; // Starting index for game state data
            int matchStateSectionLength = BitConverter.ToInt32(data, matchStateSectionStart); // First 4 bytes define the length
            int playerSectionStart = matchStateSectionStart + 4 + matchStateSectionLength;
            int playerSectionLength = BitConverter.ToInt32(data, playerSectionStart); // Next 4 bytes define the player data length
            int projectileSectionStart = playerSectionStart + 4 + playerSectionLength;

            //if (data.Length < projectileSectionStart)
            //{
            //    throw new InvalidOperationException("Invalid data structure.");
            //}

            byte[] matchStateData = data.Skip(4).Take(matchStateSectionLength).ToArray();
            byte[] playerData = data.Skip(playerSectionStart + 4).Take(playerSectionLength).ToArray();
            byte[] projectileData = data.Skip(projectileSectionStart).ToArray();

            return (matchStateData, playerData, projectileData);
        }

        private void HandleMatchStateData(byte[] matchStateData)
        {
            // Extract player-specific data
            MatchStatePacket match = new MatchStatePacket();
            match.Deserialize(matchStateData, NetworkManager.Instance.nm_LastMatchState);

            NetworkManager.Instance.nm_Match.Packet = match;
            NetworkManager.Instance.nm_Match.updateGO = true;
            NetworkManager.Instance.nm_LastMatchState = match;

            Debug.Log("Match state data processed.");
        }

        private void HandlePlayerData(byte[] playerData)
        {
            while (playerData.Length > 0)
            {
                Debug.Log("DATA LENGTH FOR PLAYERS is: " + playerData.Length);

                int playerId = BitConverter.ToInt32(playerData, 1); // 1 Byte Offset for the Type

                var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(playerId)
                    ? NetworkManager.Instance.nm_LastPlayerStates[playerId]
                    : new PlayerDataPacket();

                Debug.Log($"Last Player State: {lastState.PlayerName}, {lastState.PlayerId}, {lastState.Position[0]}, {lastState.Position[1]}, {lastState.Position[2]}");

                PlayerDataPacket player = new PlayerDataPacket();
                player.Deserialize(playerData, lastState);

                MainThreadInvoker.Invoke(() =>
                {
                    Player existingPlayer = NetworkManager.Instance.GetPlayerById(playerId);

                    //Debug.Log("PLayer is: " + existingPlayer?.Packet.PlayerName);
                    if (existingPlayer != null)
                    {
                        existingPlayer.Packet = player;
                        existingPlayer.updateGO = true;
                        lastState = player;
                    }
                    else
                    {
                        NetworkManager.Instance.InstatiateGO(player);
                        NetworkManager.Instance.nm_StatusText += $"\nPlayer {player.PlayerName} with ID {playerId} not found, instantiating NEW PLAYER.";
                        Debug.Log($"\nPlayer {player.PlayerName} with ID {playerId} not found, instantiating NEW PLAYER.");
                    }
                });

                playerData = TrimProcessedData(playerData, playerId);
            }
            Debug.Log("Player data processed.");
        }

        private void HandleProjectileData(byte[] projectileData)
        {
            while (projectileData.Length > 0)
            {
                int projectileId = BitConverter.ToInt32(projectileData, 0);
                var lastState = NetworkManager.Instance.nm_LastProjectileStates.ContainsKey(projectileId)
                    ? NetworkManager.Instance.nm_LastProjectileStates[projectileId]
                    : new ProjectilePacket();

                ProjectilePacket projectile = new ProjectilePacket();
                projectile.Deserialize(projectileData, lastState);

                var existingProjectile = NetworkManager.Instance.GetProjectileById(projectileId);
                if (existingProjectile != null)
                {
                    existingProjectile.Packet = projectile;
                    existingProjectile.updateGO = true;
                }

                NetworkManager.Instance.nm_LastProjectileStates[projectileId] = projectile;

                projectileData = TrimProcessedData(projectileData, projectileId);
            }
            Debug.Log("Projectile data processed.");
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

        private void OnApplicationQuit()
        {
            receive.Abort();
        }
    }

}

