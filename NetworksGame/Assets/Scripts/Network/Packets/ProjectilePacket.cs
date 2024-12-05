using System.IO;

namespace HyperStrike
{
    public class ProjectilePacket : Packet
    {
        public int ProjectileId;
        public int ShooterId;
        public float[] Position = new float[3];
        public float[] Velocity = new float[3];

        public ProjectilePacket()
        {
            Type = PacketType.PROJECTILE; // Unique packet type for projectiles
        }

        public override byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);
                writer.Write(ProjectileId);
                writer.Write(ShooterId);
                foreach (var value in Position) writer.Write(value);
                foreach (var value in Velocity) writer.Write(value);
                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                ProjectileId = reader.ReadInt32();
                ShooterId = reader.ReadInt32();
                for (int i = 0; i < 3; i++) Position[i] = reader.ReadSingle();
                for (int i = 0; i < 3; i++) Velocity[i] = reader.ReadSingle();
            }
        }
    }
}
