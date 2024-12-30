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
        GameManager.Instance.SetGameState(GameState.WAITING_ROOM);

        StartCoroutine(CustomSceneManager.LoadSceneWithMethodAsync(
            "PitchScene", 
            async (args) =>
            {
            string playerIp = args[0] as string;
            string playerName = args[1] as string;
            NetworkManager.Instance.gameObject.GetComponent<Server>().StartHost(playerName);
            }, 
            username.text));

        NetworkManager.Instance.gameObject.GetComponent<Client>().enabled = false;
        //Debug.Log("Debug: Creating Game " + username.text);
    }
    
    public void JoinGame(TMP_InputField username)
    {
        GameManager.Instance.SetGameState(GameState.WAITING_ROOM);

        StartCoroutine(CustomSceneManager.LoadSceneWithMethodAsync(
            "PitchScene",
            async (args) =>
            {
                string playerName = args[0] as string;
                NetworkManager.Instance.gameObject.GetComponent<Client>().StartClient(playerName);
            },
            username.text));

        NetworkManager.Instance.gameObject.GetComponent<Server>().enabled = false;
        //Debug.Log("Debug: Joining Game "+username.text);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
