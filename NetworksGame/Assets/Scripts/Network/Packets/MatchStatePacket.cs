using System;
using System.IO;

namespace HyperStrike
{
    public class MatchStatePacket : Packet
    {
        public int LocalGoals = -1;
        public int VisitantGoals = -1;
        public float CurrentTime = 0.0f;
        public float[] BallPosition = new float[3];

        public MatchStatePacket()
        {
            Type = PacketType.MATCH; // Unique packet type for projectiles
        }

        public override byte[] Serialize(ISerializable lastState)
        {
            if (lastState is not MatchStatePacket lastMatchState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not ProjectileState");
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);

                WriteDelta(writer, lastMatchState?.LocalGoals, LocalGoals);
                WriteDelta(writer, lastMatchState?.VisitantGoals, VisitantGoals);
                WriteDelta(writer, lastMatchState?.CurrentTime, CurrentTime);
                WriteDelta(writer, lastMatchState?.BallPosition, BallPosition);

                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data, ISerializable lastState)
        {
            if (lastState is not MatchStatePacket lastMatchState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not ProjectileState");
            }

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                LocalGoals = ReadDelta(reader, lastMatchState?.LocalGoals);
                VisitantGoals = ReadDelta(reader, lastMatchState?.VisitantGoals);
                BallPosition = ReadDelta(reader, lastMatchState?.BallPosition, 3);
            }
        }
    }
}


