using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

[System.Serializable]
public struct User
{
    public int userId;
    public string name;
    public EndPoint endPoint;
    public bool firstConnection;
    public PlayerData playerData;
    public GameObject userGO;
}
