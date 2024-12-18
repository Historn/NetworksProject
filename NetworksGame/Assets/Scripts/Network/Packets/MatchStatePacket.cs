using System;
using System.IO;

namespace HyperStrike
{
    public class MatchStatePacket : Packet
    {
        // 1 + 28 = 29
        public int LocalGoals = -1; // 4
        public int VisitantGoals = -1; // 4
        public float CurrentTime = 0.0f; // 4
        public float[] BallPosition = new float[3]; // 12
        public float[] BallRotation = new float[3]; // 14

        public MatchStatePacket()
        {
            Type = PacketType.MATCH; // Unique packet type for projectiles

            BallPosition[0] = 0;
            BallPosition[1] = 0;
            BallPosition[2] = 0;
            
            BallRotation[0] = 0;
            BallRotation[1] = 0;
            BallRotation[2] = 0;
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
                WriteDelta(writer, lastMatchState?.BallRotation, BallRotation);

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
                CurrentTime = ReadDelta(reader, lastMatchState?.CurrentTime);
                BallPosition = ReadDelta(reader, lastMatchState?.BallPosition, 3);
                BallRotation = ReadDelta(reader, lastMatchState?.BallRotation, 3);
            }
        }
    }
}


