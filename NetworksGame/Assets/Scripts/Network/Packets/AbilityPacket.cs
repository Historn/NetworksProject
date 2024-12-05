using System.IO;

namespace HyperStrike
{
    public class AbilityPacket : Packet
    {
        public int PlayerId;         // The player using the ability
        public int AbilityId;        // The specific ability (0-3 regular, 4 ultimate)
        public float[] TargetPosition = new float[3]; // Ability target position (if applicable)
        public bool IsAbilityStart;  // True if the ability is starting, false if ending
        public float CurrentCooldown; // Remaining cooldown for the ability
        public float UltimateCharge;  // For ultimate abilities, how much charge is available

        public AbilityPacket()
        {
            Type = PacketType.ABILITY; // Unique packet type for abilities
        }

        public override byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                writer.Write((byte)Type);
                writer.Write(PlayerId);
                writer.Write(AbilityId);
                foreach (var value in TargetPosition) writer.Write(value);
                writer.Write(IsAbilityStart);
                writer.Write(CurrentCooldown);
                writer.Write(UltimateCharge);
                return ms.ToArray();
            }
        }

        public override void Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                Type = (PacketType)reader.ReadInt32();
                PlayerId = reader.ReadInt32();
                AbilityId = reader.ReadInt32();
                for (int i = 0; i < 3; i++) TargetPosition[i] = reader.ReadSingle();
                IsAbilityStart = reader.ReadBoolean();
                CurrentCooldown = reader.ReadSingle();
                UltimateCharge = reader.ReadSingle();
            }
        }
    }
}
