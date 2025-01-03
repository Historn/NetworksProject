using UnityEngine;
using TMPro;
using HyperStrike;
using System;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject initMenu;

    [SerializeField] GameObject hostMenu;

    [SerializeField] GameObject joinMenu;

    public TMP_InputField usernameHost;
    public TMP_InputField usernameClient;
    public TMP_InputField hostIp;

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
        initMenu.SetActive(true);
    }

    public void CreateGame()
    {
        if (usernameHost is null)
        {
            throw new ArgumentNullException(nameof(usernameHost));
        }

        GameManager.Instance.SetGameState(GameState.WAITING_ROOM);

        NetworkManager.Instance.gameObject.GetComponent<Client>().enabled = false;

        StartCoroutine(CustomSceneManager.LoadSceneWithMethodAsync(
            "PitchScene", 
            async (args) =>
            {
            string playerName = args[0] as string;
            NetworkManager.Instance.gameObject.GetComponent<Server>().StartHost(playerName);
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

        GameManager.Instance.SetGameState(GameState.WAITING_ROOM);

        NetworkManager.Instance.gameObject.GetComponent<Server>().enabled = false;

        StartCoroutine(CustomSceneManager.LoadSceneWithMethodAsync(
            "PitchScene",
            async (args) =>
            {
                string playerName = args[0] as string;
                string Ip = args[1] as string;
                NetworkManager.Instance.gameObject.GetComponent<Client>().StartClient(playerName, Ip);
            },
            usernameClient.text,
            hostIp.text));
    }

    public void Quit()
    {
        Application.Quit();
    }
}
