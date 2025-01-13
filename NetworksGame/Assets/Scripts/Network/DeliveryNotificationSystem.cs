using System.Collections.Generic;

namespace HyperStrike
{
    public class DeliveryNotificationSystem
    {
        private HashSet<int> acknowledgedPackets = new HashSet<int>();
        private Dictionary<int, float> sentPackets = new Dictionary<int, float>(); // PacketId -> Timestamp
        private int currentPacketId = 0;

        public int GeneratePacketId()
        {
            return IDGenerator.GenerateID();
        }

        public void AcknowledgePacket(int packetId)
        {
            if (!acknowledgedPackets.Contains(packetId))
                acknowledgedPackets.Add(packetId);
        }

        public bool IsPacketAcknowledged(int packetId)
        {
            return acknowledgedPackets.Contains(packetId);
        }

        public void RegisterSentPacket(int packetId, float timestamp)
        {
            sentPackets[packetId] = timestamp;
        }

        public void RemoveAcknowledgedPackets()
        {
            foreach (var packetId in acknowledgedPackets)
            {
                sentPackets.Remove(packetId);
            }
        }

        public List<int> GetUnacknowledgedPackets(float timeout)
        {
            List<int> unacknowledged = new List<int>();
            float currentTime = UnityEngine.Time.time;

            foreach (var kvp in sentPackets)
            {
                if (currentTime - kvp.Value > timeout)
                    unacknowledged.Add(kvp.Key);
            }

            return unacknowledged;
        }
    }
}
