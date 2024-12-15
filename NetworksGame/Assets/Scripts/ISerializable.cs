using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperStrike
{
    public interface ISerializable
    {
        //byte[] Serialize();
        byte[] Serialize(ISerializable lastISerializable);
        void Deserialize(byte[] data, ISerializable lastISerializable);
    }
}