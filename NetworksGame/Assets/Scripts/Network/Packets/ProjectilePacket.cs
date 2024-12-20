using System;
using System.IO;

namespace HyperStrike
{

    // BY NOW 22 BYTES?
    public class ProjectilePacket : Packet
    {
        public int ProjectileId = -1; // 4
        public int ShooterId = -1; // 4
        public float[] Position = new float[3]; // 12
        public float[] Rotation = new float[4]; // 16 Quaternion for instances

        public ProjectilePacket()
        {
            Type = PacketType.PROJECTILE; // Unique packet type for projectiles

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
                WriteDelta(writer, lastProjectileState?.Rotation, Rotation);

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
                Rotation = ReadDelta(reader, lastProjectileState?.Rotation, 4);
            }
        }
    }
}
