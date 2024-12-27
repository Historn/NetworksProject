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
        float sendInterval = 0.01f;
        private bool isSendingPackets = false; // Track if the coroutine is running
        Thread receive;

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
            //Debug.Log("Client Creating Packet to Send");

            byte[] clientPacket = new byte[1024];

            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Add a header for metadata 
                int headerSize = 8; // 4 bytes for player state size change to 8 for +Projectiles
                memoryStream.Position = headerSize;

                //Debug.Log("Creating Player State Packet");

                int id = NetworkManager.Instance.nm_PlayerData.PlayerId;

                var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(id) 
                    ? NetworkManager.Instance.nm_LastPlayerStates[id] 
                    : new PlayerDataPacket();

                byte[] playerData = NetworkManager.Instance.nm_ActivePlayers[id].Packet.Serialize(lastState);

                memoryStream.Write(playerData, 0, playerData.Length);

                // PROJECTILES ENVIAR SOLO LA PRIMERA VEZ QUE SE RECIBEN
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

                memoryStream.Position = 0;
                byte[] playerDataSize = BitConverter.GetBytes(playerData.Length);
                byte[] projectilesStatesSize = BitConverter.GetBytes(projectilesData.Length);
                memoryStream.Write(playerDataSize, 0, playerDataSize.Length);
                memoryStream.Write(projectilesStatesSize, 0, projectilesStatesSize.Length);

                clientPacket = memoryStream.ToArray();
            }

            NetworkManager.Instance.nm_Socket.SendTo(clientPacket, NetworkManager.Instance.nm_ServerEndPoint);
            //Debug.Log("Client Packet Sent");
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

                Debug.Log($"Client received a packet of {recv} bytes");

                if (recv == 0) continue;

                HandlePacket(data);
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
            int headerSize = 12;

            // Assuming the data format has a predefined structure
            
            int matchStateSectionSize = BitConverter.ToInt32(data, 0); // First 4 bytes define the match data size
            int matchStateSectionStart = headerSize; // Starting index for match state data
            
            int playerSectionSize = BitConverter.ToInt32(data, 4); // Next 4 bytes define the player data length
            int playerSectionStart = matchStateSectionStart + matchStateSectionSize;

            int projectileSectionSize = BitConverter.ToInt32(data, 8); // Next 4 bytes define the player data length
            int projectileSectionStart = playerSectionStart + playerSectionSize;

            byte[] matchStateData = data.Skip(matchStateSectionStart).Take(matchStateSectionSize).ToArray();
            byte[] playerData = data.Skip(playerSectionStart).Take(playerSectionSize).ToArray();
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

            //Debug.Log("Match state data processed.");
        }

        private void HandlePlayerData(byte[] playerData)
        {
            while (playerData.Length > 0)
            {
                //Debug.Log("DATA LENGTH FOR PLAYERS is: " + playerData.Length);

                int playerId = BitConverter.ToInt32(playerData, 1); // 1 Byte Offset for the Type

                var lastState = NetworkManager.Instance.nm_LastPlayerStates.ContainsKey(playerId)
                    ? NetworkManager.Instance.nm_LastPlayerStates[playerId]
                    : new PlayerDataPacket();

                //Debug.Log($"Last Player State: {lastState.PlayerName}, {lastState.PlayerId}, {lastState.Position[0]}, {lastState.Position[1]}, {lastState.Position[2]}");

                PlayerDataPacket player = new PlayerDataPacket();
                player.Deserialize(playerData, lastState);

                //Debug.Log($"NEW Player State: {player.PlayerName}, {player.PlayerId}, {player.Position[0]}, {player.Position[1]}, {player.Position[2]}");

                // DETECT IF THE PLAYERS IS THE SAME AS THIS CLIENT
                MainThreadInvoker.Invoke(() =>
                {
                    Player existingPlayer = NetworkManager.Instance.GetPlayerById(playerId);

                    if (existingPlayer != null && existingPlayer.Packet.PlayerId != NetworkManager.Instance.nm_PlayerData.PlayerId)
                    {
                        existingPlayer.Packet = player;
                        existingPlayer.updateGO = true;
                        lastState = player;
                    }
                    else
                    {
                        NetworkManager.Instance.nm_StatusText += $"\nPlayer {player.PlayerName} with ID {playerId} not found, instantiating NEW PLAYER.";
                        Debug.Log($"\nPlayer {player.PlayerName} with ID {playerId} not found, instantiating NEW PLAYER.");
                        NetworkManager.Instance.InstatiateGO(player);
                    }
                });

                playerData = NetworkManager.Instance.TrimProcessedData(playerData, playerId);
            }
            Debug.Log("Player data processed.");
        }

        private void HandleProjectileData(byte[] projectileData)
        {
            bool iterate = true;
            while (projectileData.Length > 0 && iterate)
            {
                int projectileId = BitConverter.ToInt32(projectileData, 1);
                var lastState = new ProjectilePacket();

                ProjectilePacket projectile = new ProjectilePacket();
                projectile.Deserialize(projectileData, lastState);

                MainThreadInvoker.Invoke(() =>
                {
                    // Check if it wasnt created by this player
                    if (projectile.ProjectileId != 0 && projectile.ProjectileId != -1
                        && !NetworkManager.Instance.nm_ActiveProjectiles.Contains(projectileId)
                        && !NetworkManager.Instance.nm_ProjectilesToSend.ContainsKey(projectileId))
                    {
                        Debug.Log($"\nInstantiating NEW PROJECTILE: {projectile.ProjectileId}.");
                        Projectile existingProjectile = NetworkManager.Instance.InstatiateProjectile(projectile);
                        if (!NetworkManager.Instance.nm_ProjectilesToSend.ContainsKey(projectileId)) NetworkManager.Instance.nm_ProjectilesToSend.Add(projectileId, existingProjectile);
                        if (!NetworkManager.Instance.nm_ActiveProjectiles.Contains(projectileId)) NetworkManager.Instance.nm_ActiveProjectiles.Add(projectileId);
                    }
                    else
                    {
                        iterate = false;
                    }
                });

                projectileData = NetworkManager.Instance.TrimProcessedData(projectileData, projectileId);
            }
            Debug.Log("Projectile data processed.");
        }
        #endregion

        private void OnApplicationQuit()
        {
            receive.Abort();
        }
    }

}

