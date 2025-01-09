using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace HyperStrike
{
    public enum GameState
    {
        NONE = 0,
        TITLE,
        MENU,
        WAITING_ROOM,
        IN_GAME,
        PAUSE,
        WON,
        LOOSE
    }

    [SerializeField]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameStatePacket Packet;

        void Awake() 
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            Packet = new GameStatePacket();
        }

        // Setting the match
        [HideInInspector] public int gm_MaxPlayers = 6;
        [HideInInspector] public int gm_MaxTeamPlayers = 3;

        

        public static GameState gm_GameState { get; private set; } = GameState.MENU; // Change for the final

        [Header("Objects")]
        [SerializeField] GameObject canvasObj;
        [SerializeField] GameObject menuPanelObj;

        private void Start()
        {
            DontDestroyOnLoad(canvasObj);
        }

        void GameStateBehavior()
        {
            switch (gm_GameState)
            {
                case GameState.NONE:
                    //StartCoroutine(CustomSceneManager.LoadYourAsyncScene("MainTitle"));
                    gm_GameState = GameState.TITLE;
                    break;
                case GameState.TITLE:
                    break;
                case GameState.MENU:
                    SceneManager.LoadScene("MainMenu");
                    break;
                case GameState.WAITING_ROOM:
                    //StartCoroutine(CustomSceneManager.LoadYourAsyncScene("PitchScene"));
                    menuPanelObj.SetActive(false);
                    break;
                case GameState.IN_GAME:
                    break;
                case GameState.PAUSE:
                    break;
                case GameState.WON:
                    break;
                case GameState.LOOSE:
                    break;
                default:
                    break;
            }
        }

        public void SetGameState(GameState state)
        {
            gm_GameState = state;
            GameStateBehavior();
        }
    }
}