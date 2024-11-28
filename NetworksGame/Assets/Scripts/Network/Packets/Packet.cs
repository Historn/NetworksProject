using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperStrike
{
    public abstract class Packet : ISerializable
    {
        // Change to enum
        public int PacketType { get; protected set; }

        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] data);
    }
}
