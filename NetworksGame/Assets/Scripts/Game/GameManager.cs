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
            if (Instance == null) Instance = this;

            Packet = new GameStatePacket();
        }

        // Setting the match
        [HideInInspector] public int gm_MaxPlayers = 6;
        [HideInInspector] public int gm_MaxTeamPlayers = 3;

        // Conditions
        public float gm_MaxTime = 300f; // 300 = 5 minutes in seconds
        [HideInInspector] public int gm_LocalGoals = 0;
        [HideInInspector] public int gm_VisitantGoals = 0;

        public static GameState gm_GameState { get; private set; } = GameState.MENU; // Change for the final

        [Header("Objects")]
        [SerializeField] GameObject managerObj;
        [SerializeField] GameObject canvasObj;
        [SerializeField] GameObject menuPanelObj;

        private void Start()
        {
            DontDestroyOnLoad(managerObj);
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