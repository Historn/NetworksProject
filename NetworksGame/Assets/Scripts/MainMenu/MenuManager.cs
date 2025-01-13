using UnityEngine;
using TMPro;
using HyperStrike;
using System;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] GameObject initMenu;
    [SerializeField] GameObject hostMenu;
    [SerializeField] GameObject joinMenu;

    [Header("Connection Inputs")]
    public TMP_InputField usernameHost;
    public TMP_InputField usernameClient;
    public TMP_InputField hostIp;

    [SerializeField] TextMeshProUGUI errorText;

    public void OpenHostGameMenu()
    {
        initMenu.SetActive(false);
        hostMenu.SetActive(true);
    }
    
    public void OpenJoinGameMenu()
    {
        initMenu.SetActive(false);
        joinMenu.SetActive(true);
    }
    
    public void BackToInitMenu()
    {
        hostMenu.SetActive(false);
        joinMenu.SetActive(false);
        errorText.text = "";
        initMenu.SetActive(true);
    }

    public void CreateGame()
    {
        if (usernameHost is null)
        {
            throw new ArgumentNullException(nameof(usernameHost));
        }

        if (NetworkManager.Instance.gameObject.GetComponent<Server>().StartHost(usernameHost.text) == false)
        {
            errorText.text = "Couldn't create game, it has been already created in this local network.";
            return;
        }

        GameManager.Instance.SetGameState(GameState.WAITING_ROOM);

        NetworkManager.Instance.nm_IsHost = true;
        NetworkManager.Instance.gameObject.GetComponent<Client>().enabled = false;
        
        StartCoroutine(CustomSceneManager.LoadSceneWithMethodAsync(
            "PitchScene",
            async (args) =>
            {
            string playerName = args[0] as string;
            NetworkManager.Instance.gameObject.GetComponent<Server>().SetHost(playerName);
            },
            usernameHost.text));
    }
    
    public void JoinGame()
    {
        if (usernameClient is null)
        {
            throw new ArgumentNullException(nameof(usernameClient));
        }

        if (hostIp is null)
        {
            throw new ArgumentNullException(nameof(hostIp));
        }

        if (NetworkManager.Instance.gameObject.GetComponent<Client>().StartClient(usernameClient.text, hostIp.text) == false) 
        {
            errorText.text = "Failed to connect to the host!";
            return; 
        }

        GameManager.Instance.SetGameState(GameState.WAITING_ROOM);

        NetworkManager.Instance.nm_IsHost = false;
        NetworkManager.Instance.gameObject.GetComponent<Server>().enabled = false;

        StartCoroutine(CustomSceneManager.LoadSceneWithMethodAsync(
            "PitchScene",
            async (args) =>
            {
                string playerName = args[0] as string;
                NetworkManager.Instance.gameObject.GetComponent<Client>().SetClient(playerName);
            },
            usernameClient.text));
    }

    public void Quit()
    {
        Application.Quit();
    }
}
