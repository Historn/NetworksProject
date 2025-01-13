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
        public float[] BallRotation = new float[4];

        public MatchStatePacket()
        {
            Type = PacketType.MATCH;

            BallPosition[0] = 0;
            BallPosition[1] = 0;
            BallPosition[2] = 0;
            
            BallRotation[0] = 0;
            BallRotation[1] = 0;
            BallRotation[2] = 0;
            BallRotation[3] = 0;
        }

        public override byte[] Serialize(ISerializable lastState)
        {
            if (lastState is not MatchStatePacket lastMatchState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not MatchState");
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);

                // Placeholder for Size, will update later
                long sizePosition = ms.Position;
                writer.Write(0); // Temporary Size value

                WriteDelta(writer, lastMatchState?.LocalGoals, LocalGoals);
                WriteDelta(writer, lastMatchState?.VisitantGoals, VisitantGoals);
                WriteDelta(writer, lastMatchState?.CurrentTime, CurrentTime);
                WriteDelta(writer, lastMatchState?.BallPosition, BallPosition);
                WriteDelta(writer, lastMatchState?.BallRotation, BallRotation);

                // Update Size
                Size = (int)ms.Length;
                ms.Seek(sizePosition, SeekOrigin.Begin);
                writer.Write(Size);

                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data, ISerializable lastState)
        {
            if (lastState is not MatchStatePacket lastMatchState)
            {
                throw new ArgumentException("Invalid packet type for delta serialization not MatchState");
            }

            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadByte();
                Size = reader.ReadInt32();
                LocalGoals = ReadDelta(reader, lastMatchState?.LocalGoals);
                VisitantGoals = ReadDelta(reader, lastMatchState?.VisitantGoals);
                CurrentTime = ReadDelta(reader, lastMatchState?.CurrentTime);
                BallPosition = ReadDelta(reader, lastMatchState?.BallPosition, 3);
                BallRotation = ReadDelta(reader, lastMatchState?.BallRotation, 4);
            }
        }
    }
}


