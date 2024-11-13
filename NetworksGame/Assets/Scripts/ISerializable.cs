using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISerializable
{
    //JsonUtility jsonUtility;
    void Serialize();
    void Deserialize();
}
