using System;
using System.IO;

namespace HyperStrike
{

    // BY NOW 22 BYTES?
    public class ProjectilePacket : Packet
    {
        public bool IsActive;
        public bool IsExploding;
        public int ProjectileId;
        public int ShooterId;
        public float[] Position = new float[3];

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

                WriteBoolDelta(writer, lastProjectileState?.IsActive, IsActive);
                WriteBoolDelta(writer, lastProjectileState?.IsExploding, IsExploding);

                WriteDelta(writer, lastProjectileState?.Position, Position);

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

                IsActive = ReadBoolDelta(reader, lastProjectileState?.IsActive);
                IsExploding = ReadBoolDelta(reader, lastProjectileState?.IsExploding);

                Position = ReadDelta(reader, lastProjectileState?.Position, 3);
                
            }
        }
    }
}
