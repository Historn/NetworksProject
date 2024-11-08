using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


//Adapt it to use UDP for connection but TCP for chat or data that needs to be 100% received

public class Client : MonoBehaviour
{
    public GameObject ComputerTCP;
    public GameObject UiButtonTextObj;

    public GameObject UItextObj;
    TextMeshProUGUI UItext;

    public GameObject UiInputUsernameObj;
    TMP_InputField UiInputUsername;
    string userName;

    public GameObject UiInputMessageObj;
    TMP_InputField UiInputMessage;

    Socket socket;
    string clientText;

    Thread mainUDPThread;
    Thread mainTCPThread;
    bool connected = false;
    bool waiting = false;

    void Start()
    {
        UItext = UItextObj.GetComponent<TextMeshProUGUI>();
        UiInputMessage = UiInputMessageObj.GetComponent<TMP_InputField>();
        UiInputUsername = UiInputUsernameObj.GetComponent<TMP_InputField>();
        mainUDPThread = new Thread(Send);
        mainTCPThread = new Thread(Send);
    }
    public void StartClient()
    {
        mainUDPThread.Start();
        mainTCPThread.Start();
    }

    void Update()
    {
        UItext.text = clientText;
        if (!mainUDPThread.IsAlive && connected)
        {
            IEnumerator coroutine = waiter();
            StartCoroutine(coroutine);
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Thread sendThread = new Thread(Send);
            sendThread.Start();
        }
    }

    void Send()
    {
        try
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
            if (!waiting)
            {
                //Unlike with TCP, we don't "connect" first,
                //we are going to send a message to establish our communication so we need an endpoint
                //We need the server's IP and the port we've binded it to before
                //Again, initialize the socket

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Connect(ipep);
                clientText += "\nConnected to the server";

                //Send the Handshake to the server's endpoint.
                //This time, our UDP socket doesn't have it, so we have to pass it
                //as a parameter on it's SendTo() method
                socket.SendTo(Encoding.ASCII.GetBytes(UiInputUsername.text), ipep);

                //We'll wait for a server response,
                //so you can already start the receive thread
                Thread receive = new Thread(Receive);
                receive.Start();

                connected = true;
            }
            else
            {
                socket.SendTo(Encoding.ASCII.GetBytes(UiInputMessage.text), ipep);
            }

        }
        catch (SocketException ex)
        {
            clientText += $"\nError sending message to the server: {ex.Message}";
        }

    }

    void Receive()
    {
        byte[] data = new byte[1024];
        while (true)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            int recv = socket.ReceiveFrom(data, ref Remote);

            clientText += "\n" + Encoding.ASCII.GetString(data, 0, recv);
        }
    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(4);
        if (connected)
        {
            SceneManager.LoadScene("Exercise1_WaitingRoom");
            ComputerTCP.SetActive(false);
            UiButtonTextObj.SetActive(false);
            UiInputUsernameObj.SetActive(false);
            UiInputMessageObj.SetActive(true);
            connected = false;
            waiting = true;
        }
    }
}
