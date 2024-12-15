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

        public int GameState;
        public int LocalGoals;
        public int VisitantGoals;
        public float CurrentTime;

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
                //writer.Write(ProjectileId);
                //writer.Write(ShooterId);
                //foreach (var value in Position) writer.Write(value);
                //foreach (var value in Velocity) writer.Write(value);
                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data, ISerializable lastState)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                //ProjectileId = reader.ReadInt32();
                //ShooterId = reader.ReadInt32();
                //for (int i = 0; i < 3; i++) Position[i] = reader.ReadSingle();
                //for (int i = 0; i < 3; i++) Velocity[i] = reader.ReadSingle();
            }
        }
    }
}

