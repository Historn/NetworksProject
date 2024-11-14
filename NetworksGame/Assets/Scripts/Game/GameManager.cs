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
        public static GameManager instance { get; private set; } // Singleton

        void Awake() { if (instance == null) instance = this; }

        /// <summary>
        /// CHANGE THIS CLASS TO BE MODIFIABLE BY PLAYERS TO CREATE CUSTOM MATCHES
        /// </summary>

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

        void GameStateBehavior()
        {
            switch (gm_GameState)
            {
                case GameState.NONE:
                    SceneManager.LoadScene("MainTitle");
                    gm_GameState = GameState.TITLE;
                    break;
                case GameState.TITLE:
                    break;
                case GameState.MENU:
                    break;
                case GameState.WAITING_ROOM:
                    SceneManager.LoadScene("ArnauTestingScene");
                    
                    break;
                case GameState.IN_GAME:
                    break;
                case GameState.PAUSE:
                    break;
                case GameState.WON: // Maybe change to finished match?
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

