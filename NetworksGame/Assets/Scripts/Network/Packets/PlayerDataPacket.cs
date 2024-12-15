using System;
using System.IO;
using static UnityEditorInternal.VersionControl.ListControl;

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
        public string PlayerName;
        public int PlayerId;
        public int CharacterId; // Change to enum
        public PlayerState State; // Not serialized right now
        public float UltimateCharge;
        public float[] Position = new float[3];
        public float[] Rotation = new float[4]; // Quaternions

        public PlayerDataPacket()
        {
            Type = PacketType.PLAYER_DATA; // Assign a unique packet ID
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
                writer.Write(PlayerName);
                writer.Write(PlayerId);
                writer.Write(CharacterId);

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
                Type = (PacketType)reader.ReadInt32();
                PlayerName = reader.ReadString();
                PlayerId = reader.ReadInt32();
                CharacterId = reader.ReadInt32();

                UltimateCharge = ReadDelta(reader, lastPlayerData?.UltimateCharge);
                Position = ReadDelta(reader, lastPlayerData?.Position, 3);
                Rotation = ReadDelta(reader, lastPlayerData?.Rotation, 4);
            }
        }
    }
}