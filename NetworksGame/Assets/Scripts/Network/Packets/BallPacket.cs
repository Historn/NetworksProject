using System;
using System.IO;

namespace HyperStrike
{
    public class BallPacket : Packet
    {
        public float[] Position = new float[3];
        public float[] Rotation = new float[4];
        public int LastHitPlayerId = -1;
        public BallPacket()
        {
            Type = PacketType.BALL;

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
            if (lastState is not BallPacket lastBallState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not BallPacket");
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);

                // Placeholder for Size, will update later
                long sizePosition = ms.Position;
                writer.Write(0); // Temporary Size value

                WriteDelta(writer, lastBallState?.LastHitPlayerId, LastHitPlayerId);
                WriteDelta(writer, lastBallState?.Position, Position);
                WriteDelta(writer, lastBallState?.Rotation, Rotation);

                // Update Size
                Size = (int)ms.Length;
                ms.Seek(sizePosition, SeekOrigin.Begin);
                writer.Write(Size);

                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data, ISerializable lastState)
        {
            if (lastState is not BallPacket lastBallState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not BallPacket");
            }

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                Size = reader.ReadInt32();

                LastHitPlayerId = ReadDelta(reader, lastBallState?.LastHitPlayerId);
                Position = ReadDelta(reader, lastBallState?.Position, 3);
                Rotation = ReadDelta(reader, lastBallState?.Rotation, 4);
            }
        }
    }
}
