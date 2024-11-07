using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class ClientTCP : MonoBehaviour
{
    public GameObject ComputerUDP;

    public GameObject UiButtonTextObj;

    public GameObject UiTextObj;
    TextMeshProUGUI UiText;

    public GameObject UiInputIPObj;
    TMP_InputField UiInputIP;

    public GameObject UiInputMessageObj;
    TMP_InputField UiInputMessage;

    public GameObject UiInputUsernameObj;
    TMP_InputField UiInputUsername;

    string clientText;
    Socket server;

    Thread connect;
    bool connected = false;

    // Start is called before the first frame update
    void Start()
    {
        UiText = UiTextObj.GetComponent<TextMeshProUGUI>();
        UiInputIP = UiInputIPObj.GetComponent<TMP_InputField>();
        UiInputMessage = UiInputMessageObj.GetComponent<TMP_InputField>();
        UiInputUsername = UiInputUsernameObj.GetComponent<TMP_InputField>();
        connect = new Thread(Connect);
    }

    // Update is called once per frame
    void Update()
    {
        UiText.text = clientText;

        if (!connect.IsAlive && connected)
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

    public void StartClient()
    {
        connect.Start();
    }
    void Connect()
    {
        //TO DO 2
        //Create the server endpoint so we can try to connect to it.
        //You'll need the server's IP and the port we binded it to before
        //Also, initialize our server socket.
        //When calling connect and succeeding, our server socket will create a
        //connection between this endpoint and the server's endpoint
        try
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(UiInputIP.text), 9050);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(ipep);
            clientText += "\nConnected to the server";
            connected = true;

            //TO DO 4
            //With an established connection, we want to send a message so the server aacknowledges us
            //Start the Send Thread
            Thread sendThread = new Thread(Send);
            sendThread.Start();

            //TO DO 7s
            //If the client wants to receive messages, it will have to start another thread. Call Receive()
            Thread receiveThread = new Thread(Receive);
            receiveThread.Start();
        }
        catch (SocketException ex)
        {
            Debug.Log($"Connection error: {ex.Message}");
        }
    }
    void Send()
    {
        //TO DO 4
        //Using the socket that stores the connection between the 2 endpoints, call the TCP send function with
        //an encoded message
        byte[] data = new byte[1024];

        if (connected) 
        {
            data = Encoding.ASCII.GetBytes(UiInputUsername.text);
        }
        else
            data = Encoding.ASCII.GetBytes(UiInputMessage.text);

        try
        {
            server.Send(data);
        }
        catch (SocketException ex)
        {
            Debug.Log($"Send error: {ex.Message}");
        }
    }

    //TO DO 7
    //Similar to what we already did with the server, we have to call the Receive() method from the socket.
    void Receive()
    {
        byte[] data = new byte[1024];
        int recv = 0;

        while (true)
        {
            try
            {
                recv = server.Receive(data);
                if (recv == 0) break;

                string receivedMessage = Encoding.ASCII.GetString(data, 0, recv);
                clientText += $"\n{receivedMessage}"; // Do receive user name + message
            }
            catch (SocketException ex)
            {
                Debug.Log($"Receive error: {ex.Message}");
                break;
            }
        }
    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(4);
        if (connected)
        {
            SceneManager.LoadScene("Exercise1_WaitingRoom");
            ComputerUDP.SetActive(false);
            UiButtonTextObj.SetActive(false);
            UiInputUsernameObj.SetActive(false);
            UiInputIPObj.SetActive(false);
            UiInputMessageObj.SetActive(true);
            connected = false;
        }
    }

}
