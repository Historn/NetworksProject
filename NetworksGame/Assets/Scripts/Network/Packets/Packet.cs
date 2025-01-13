using System.IO;
using System.Linq;

namespace HyperStrike
{
    /// <summary>
    /// TYPE OF THE PACKETS: 
    /// Add sequential types, don't add in between other types.
    /// </summary>
    public enum PacketType : byte
    {
        NONE,
        GAME_STATE,
        MATCH,
        PLAYER_DATA,
        ABILITY,
        PROJECTILE,
        BALL
    }

    public abstract class Packet : ISerializable
    {
        //public ushort SequenceNumber; // Add for reliability

        public PacketType Type { get; protected set; }
        public int Size { get; protected set; }

        public abstract byte[] Serialize(ISerializable lastStatePacket);
        public abstract void Deserialize(byte[] data, ISerializable lastStatePacket);

        protected void WriteDelta(BinaryWriter writer, int? lastValue, int currentValue)
        {
            if (lastValue != currentValue)
            {
                writer.Write(true);
                writer.Write(currentValue);
            }
            else
            {
                writer.Write(false);
            }
        }

        protected int ReadDelta(BinaryReader reader, int? lastValue)
        {
            if (reader.ReadBoolean())
            {
                int value = reader.ReadInt32();
                return value;
            }
            return lastValue ?? 0;
        }
        
        protected void WriteDelta(BinaryWriter writer, float? lastValue, float currentValue)
        {
            if (lastValue != currentValue)
            {
                writer.Write(true);
                writer.Write(currentValue);
            }
            else
            {
                writer.Write(false);
            }
        }

        protected float ReadDelta(BinaryReader reader, float? lastValue)
        {
            if (reader.ReadBoolean())
            {
                float value = reader.ReadSingle();
                return value;
            }
            return lastValue ?? 0.0f;
        }
        
        protected void WriteDelta(BinaryWriter writer, float[] lastValue, float[] currentValue)
        {
            if (lastValue == null || !lastValue.SequenceEqual(currentValue))
            {
                writer.Write(true);
                foreach (var value in currentValue) writer.Write(value);
            }
            else
            {
                writer.Write(false);
            }
        }

        protected float[] ReadDelta(BinaryReader reader, float[] lastValue, int size)
        {
            if (reader.ReadBoolean())
            {
                float[] values = new float[size];
                for (int i = 0; i < size; i++) values[i] = reader.ReadSingle();
                return values;
            }
            return lastValue ?? new float[size];
        }

        protected void WriteBoolDelta(BinaryWriter writer, bool? lastValue, bool currentValue)
        {
            if (lastValue == null || lastValue != currentValue)
            {
                writer.Write(true);
                writer.Write(currentValue);
            }
            else
            {
                writer.Write(false);
            }
        }

        protected bool ReadBoolDelta(BinaryReader reader, bool? lastValue)
        {
            return reader.ReadBoolean() ? reader.ReadBoolean() : lastValue ?? false;
        }
    }
}
