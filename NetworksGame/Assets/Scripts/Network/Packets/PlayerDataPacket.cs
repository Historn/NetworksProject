using System;
using System.IO;

namespace HyperStrike
{
    public enum PlayerState : byte
    {
        IDLE = 0,
        RUNNING,
        JUMPING,
        SHOOTING,
        DEAD
    }

    public class PlayerDataPacket : Packet
    {
        public string PlayerName = "Unknown";
        public int PlayerId = -1;
        public float[] Position = new float[3];
        public float[] Rotation = new float[4];
        public int Score = 0;
        public int Goals = 0;

        public PlayerDataPacket()
        {
            Type = PacketType.PLAYER_DATA;

            Position[0] = 0;
            Position[1] = 0;
            Position[2] = 0;

            Rotation[0] = 0;
            Rotation[1] = 0;
            Rotation[2] = 0;
            Rotation[3] = 0;
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

                // Placeholder for Size, will update later
                long sizePosition = ms.Position;
                writer.Write(0); // Temporary Size value

                writer.Write(PlayerId);
                writer.Write(PlayerName);

                // Compare with the last state
                WriteDelta(writer, lastPlayerData?.Position, Position);
                WriteDelta(writer, lastPlayerData?.Rotation, Rotation);
                WriteDelta(writer, lastPlayerData?.Score, Score);
                WriteDelta(writer, lastPlayerData?.Goals, Goals);

                // Update Size
                Size = (int)ms.Length;
                ms.Seek(sizePosition, SeekOrigin.Begin);
                writer.Write(Size);

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
                Size = reader.ReadInt32();
                PlayerId = reader.ReadInt32();
                PlayerName = reader.ReadString();

                Position = ReadDelta(reader, lastPlayerData?.Position, 3);
                Rotation = ReadDelta(reader, lastPlayerData?.Rotation, 4);
                Score = ReadDelta(reader, lastPlayerData?.Score);
                Goals = ReadDelta(reader, lastPlayerData?.Goals);
            }
        }
    }
}