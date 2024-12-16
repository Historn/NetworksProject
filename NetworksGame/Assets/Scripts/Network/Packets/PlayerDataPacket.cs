using System;
using System.IO;

namespace HyperStrike
{
    public enum PlayerState
    {
        IDLE = 0,
        RUNNING,
        JUMPING,
        SHOOTING,
        DEAD
    }

    // BY NOW 74 BYTES?
    public class PlayerDataPacket : Packet
    {
        public string PlayerName = "Unknown";
        public int PlayerId = -1;
        public int CharacterId = -1; // Change to enum
        public PlayerState State = 0; // Not serialized right now
        public float UltimateCharge = 0.0f;
        public float[] Position = new float[3];
        public float[] Rotation = new float[3]; // Quaternions

        public PlayerDataPacket()
        {
            Type = PacketType.PLAYER_DATA; // Assign a unique packet ID

            Position[0] = 0;
            Position[1] = 0;
            Position[2] = 0;

            Rotation[0] = 0;
            Rotation[1] = 0;
            Rotation[2] = 0;
        }

        public override byte[] Serialize(ISerializable lastState)
        {
            if (lastState is not PlayerDataPacket lastPlayerData)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not PlayerData");
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);
                writer.Write(PlayerId);
                writer.Write(CharacterId);
                writer.Write(PlayerName);

                // Compare with the last state
                WriteDelta(writer, lastPlayerData?.UltimateCharge, UltimateCharge);
                WriteDelta(writer, lastPlayerData?.Position, Position);
                WriteDelta(writer, lastPlayerData?.Rotation, Rotation);

                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data, ISerializable lastState)
        {
            if (lastState is not PlayerDataPacket lastPlayerData)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not PlayerData");
            }

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                PlayerId = reader.ReadInt32();
                CharacterId = reader.ReadInt32();
                PlayerName = reader.ReadString();

                UltimateCharge = ReadDelta(reader, lastPlayerData?.UltimateCharge);
                Position = ReadDelta(reader, lastPlayerData?.Position, 3);
                Rotation = ReadDelta(reader, lastPlayerData?.Rotation, 3);
            }
        }
    }
}