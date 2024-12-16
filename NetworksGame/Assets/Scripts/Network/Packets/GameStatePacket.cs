using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace HyperStrike
{
    public class GameStatePacket : Packet
    {
        // ACTION: ALWAYS UPDATE 

        public int GameState = 0;

        public GameStatePacket()
        {
            Type = PacketType.GAME_STATE; // Unique packet type for projectiles
        }
        
        public override byte[] Serialize(ISerializable lastState)
        {
            if (lastState is not GameStatePacket lastGameState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not GameState");
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);
                WriteDelta(writer, lastGameState?.GameState, GameState);
                
                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data, ISerializable lastState)
        {
            if (lastState is not GameStatePacket lastGameState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not GameState");
            }

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                GameState = ReadDelta(reader, lastGameState?.GameState);
            }
        }
    }
}

