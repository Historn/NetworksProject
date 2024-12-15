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
        private float currentMatchTime;
        private Coroutine matchTimerCoroutine;

        [Header("Match Settings")]
        [SerializeField] private float matchDuration = 300f; // 5 minutes in seconds
        [SerializeField] private GameObject ball;
        [SerializeField] private Transform ballStartPosition;

        [Header("UI Match Display")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI localScoreText;
        [SerializeField] private TextMeshProUGUI visitantScoreText;

        [Header("VFX References")]
        [SerializeField] private GameObject localGoalVFX;
        [SerializeField] private GameObject visitantGoalVFX;

        public static GameState gm_GameState { get; private set; } = GameState.MENU; // Change for the final

        [Header("Objects")]
        [SerializeField] GameObject managerObj;
        [SerializeField] GameObject canvasObj;
        [SerializeField] GameObject menuPanelObj;

        private void Start()
        {
            DontDestroyOnLoad(managerObj);
            DontDestroyOnLoad(canvasObj);

            currentMatchTime = matchDuration;
            UpdateScoreUI();
            UpdateTimerUI();

            matchTimerCoroutine = StartCoroutine(MatchTimer());
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

        private void UpdateScoreUI()
        {
            localScoreText.text = $"Local: {gm_LocalGoals}";
            visitantScoreText.text = $"Visitant: {gm_VisitantGoals}";

            Debug.Log($"Updated Score UI - Local: {gm_LocalGoals}, Visitant: {gm_VisitantGoals}");
        }

        private void UpdateTimerUI()
        {
            int minutes = Mathf.FloorToInt(currentMatchTime / 60f);
            int seconds = Mathf.FloorToInt(currentMatchTime % 60f);

            timerText.text = $"{minutes:D2}:{seconds:D2}";

            Debug.Log($"Updated Timer UI - Time Left: {timerText.text}");
            timerText.ForceMeshUpdate();
        }

        public void IncrementLocalScore()
        {
            gm_LocalGoals++;
            UpdateScoreUI();

            TriggerGoalVFX(localGoalVFX);

            ResetBallPosition();
        }

        public void IncrementVisitantScore()
        {
            gm_VisitantGoals++;
            UpdateScoreUI();

            TriggerGoalVFX(visitantGoalVFX);

            ResetBallPosition();
        }

        private void TriggerGoalVFX(GameObject vfxPrefab)
        {
            if (vfxPrefab != null)
            {
                // Instantiate the VFX at the ball's current position
                GameObject vfxInstance = Instantiate(vfxPrefab, ball.transform.position, Quaternion.identity);

                Destroy(vfxInstance, 3f);
            }
        }

        private IEnumerator MatchTimer()
        {
            while (currentMatchTime > 0)
            {
                yield return new WaitForSeconds(1f);
                currentMatchTime--;
                UpdateTimerUI();
            }

            EndMatch();
        }

        private void EndMatch()
        {
            // Stop the match timer coroutine
            if (matchTimerCoroutine != null)
            {
                StopCoroutine(matchTimerCoroutine);
            }

            // Set the game state to WON or LOOSE based on the score
            gm_GameState = gm_LocalGoals > gm_VisitantGoals ? GameState.WON : GameState.LOOSE;
            GameStateBehavior();

            Debug.Log("Match Ended!");
            Debug.Log($"Final Score: Local {gm_LocalGoals} - {gm_VisitantGoals} Visitant");

            RestartMatch();
        }

        public void RestartMatch()
        {
            // Reset the scores
            gm_LocalGoals = 0;
            gm_VisitantGoals = 0;
            UpdateScoreUI();

            // Reset the timer
            currentMatchTime = matchDuration;
            UpdateTimerUI();

            // Reset the ball to the center
            ResetBallPosition();

            // Restart the match timer
            matchTimerCoroutine = StartCoroutine(MatchTimer());

            // Reset the game state
            gm_GameState = GameState.IN_GAME;
            GameStateBehavior();
        }

        private void ResetBallPosition()
        {
            // Reset ball's position and velocity
            ball.transform.position = ballStartPosition.position;
            Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
            if (ballRigidbody != null)
            {
                ballRigidbody.velocity = Vector3.zero;
                ballRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }
}