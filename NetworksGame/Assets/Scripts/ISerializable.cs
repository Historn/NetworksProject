using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISerializable
{
    // First convert it to Json then to byte[] + add headers?
    byte[] Serialize();
    void Deserialize();
}
