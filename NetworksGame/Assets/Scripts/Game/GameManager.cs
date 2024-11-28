using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public static GameManager instance { get; private set; }

        void Awake() { if (instance == null) instance = this; }

        // Setting the match
        uint gm_MaxPlayers = 6;
        uint gm_MaxTeamPlayers = 3;

        // Conditions
        float gm_MaxTime = 5.0f; // Make it to Minutes
        //Dictionary<Player, score> gm_LocalTeamScore // Save into a dictionary the score of each player of the team
        //Dictionary<Player, score> gm_VisitantTeamScore
        uint gm_LocalGoals = 0;
        uint gm_VisitantGoals = 0;

        public static GameState gm_GameState { get; private set; } = GameState.MENU; // Change for the final

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
                    StartCoroutine(CustomSceneManager.LoadYourAsyncScene("MainTitle"));
                    gm_GameState = GameState.TITLE;
                    break;
                case GameState.TITLE:
                    break;
                case GameState.MENU:
                    break;
                case GameState.WAITING_ROOM:
                    StartCoroutine(CustomSceneManager.LoadYourAsyncScene("PitchScene"));
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

