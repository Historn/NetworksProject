using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperStrike
{
    public interface ISerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }
}