using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using HyperStrike;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject hostButton;
    [SerializeField] GameObject hostMenu;

    [SerializeField] GameObject hostInputText;
    TMP_InputField UiInputUsername;

    [SerializeField] GameObject gameManagerObj;
    [SerializeField] GameObject networkManagerObj;

    // Start is called before the first frame update
    void Start()
    {
        UiInputUsername = hostInputText.GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenHostMatchMenu()
    {
        hostButton.SetActive(false);
        hostMenu.SetActive(true);
    }

    public void CreateMatch()
    {
        DontDestroyOnLoad(gameManagerObj);
        DontDestroyOnLoad(networkManagerObj);
        networkManagerObj.GetComponent<Client>().enabled = false;
        GameManager.SetGameState(GameState.WAITING_ROOM);
    }
}
