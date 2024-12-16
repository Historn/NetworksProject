using System;
using System.IO;

namespace HyperStrike
{

    // BY NOW 22 BYTES?
    public class ProjectilePacket : Packet
    {
        // Type(1 Byte) + 32 Bytes = 33
        public int ProjectileId = -1; // 4
        public int ShooterId = -1; // 4
        public float[] Position = new float[3]; // 12
        public float[] Forward = new float[3]; // 12

        public ProjectilePacket()
        {
            Type = PacketType.PROJECTILE; // Unique packet type for projectiles
        }
        
        public override byte[] Serialize(ISerializable lastState)
        {
            if (lastState is not ProjectilePacket lastProjectileState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not ProjectileState");
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);

                writer.Write(ProjectileId);
                writer.Write(ShooterId);

                WriteDelta(writer, lastProjectileState?.Position, Position);
                WriteDelta(writer, lastProjectileState?.Forward, Forward);

                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data, ISerializable lastState)
        {
            if (lastState is not ProjectilePacket lastProjectileState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not ProjectileState");
            }

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                ProjectileId = reader.ReadInt32();
                ShooterId = reader.ReadInt32();

                Position = ReadDelta(reader, lastProjectileState?.Position, 3);
                
            }
        }
    }
}
