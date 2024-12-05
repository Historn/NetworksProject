using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace HyperStrike
{
    /// <summary>
    /// TYPE OF THE PACKETS: 
    /// Add sequential types, don't add in between other types.
    /// </summary>
    public enum PacketType : byte
    {
        NONE = 0,
        PLAYER_DATA,
        ABILITY,
        PROJECTILE,
    }

    public abstract class Packet : ISerializable
    {
        public ushort SequenceNumber;

        public PacketType Type { get; protected set; }

        public abstract byte[] Serialize();
        public abstract void Deserialize(byte[] data);
    }
}
