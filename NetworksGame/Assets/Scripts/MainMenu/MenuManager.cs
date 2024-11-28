using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyperStrike;
using UnityEngine.SceneManagement;

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
        GameManager.instance.SetGameState(GameState.WAITING_ROOM);
        NetworkManager.instance.gameObject.GetComponent<Server>().StartHost(username.text);
        NetworkManager.instance.gameObject.GetComponent<Client>().enabled = false;
        Debug.Log("Debug: Creating Game " + username.text);
    }
    
    public void JoinGame(TMP_InputField username)
    {
        GameManager.instance.SetGameState(GameState.WAITING_ROOM);
        NetworkManager.instance.gameObject.GetComponent<Client>().StartClient(username.text);
        NetworkManager.instance.gameObject.GetComponent<Server>().enabled = false;
        Debug.Log("Debug: Joining Game "+username.text);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
