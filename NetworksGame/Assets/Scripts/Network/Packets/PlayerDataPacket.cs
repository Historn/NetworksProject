using System.IO;

namespace HyperStrike
{
    public class PlayerDataPacket : Packet
    {
        public string PlayerName;
        public int PlayerId;
        public float[] Position = new float[3];

        public PlayerDataPacket()
        {
            Type = PacketType.PLAYER_DATA; // Assign a unique packet ID
        }

        public override byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);
                writer.Write(PlayerName);
                writer.Write(PlayerId);
                foreach (float pos in Position)
                    writer.Write(pos);

                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadInt32();
                PlayerName = reader.ReadString();
                PlayerId = reader.ReadInt32();
                for (int i = 0; i < 3; i++)
                    Position[i] = reader.ReadSingle();
            }
        }
    }
}