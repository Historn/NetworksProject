using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyperStrike;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject initMenu;

    [SerializeField] GameObject hostMenu;

    [SerializeField] GameObject joinMenu;

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

    public void CreateGame(TMP_InputField username)
    {
        Debug.Log(username.text);
        NetworkManager.instance.StartHost(username.text);
        GameManager.instance.SetGameState(GameState.WAITING_ROOM);
    }
    
    public void JoinGame(TMP_InputField username)
    {
        Debug.Log(username.text);
        NetworkManager.instance.StartClient(username.text);
        GameManager.instance.SetGameState(GameState.WAITING_ROOM);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
