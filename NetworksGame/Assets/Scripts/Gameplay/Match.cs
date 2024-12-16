using HyperStrike;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Match : MonoBehaviour
{
    public bool updateGO = false;

    private float currentMatchTime = 300f;
    private Coroutine matchTimerCoroutine;

    [Header("Match Settings")]
    [SerializeField] private GameObject ball;
    [SerializeField] private Transform ballStartPosition;

    [Header("UI Match Display")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI localScoreText;
    [SerializeField] private TextMeshProUGUI visitantScoreText;

    [Header("3D Scoreboard References")]
    [SerializeField] private TextMeshPro localScore3DText;
    [SerializeField] private TextMeshPro visitantScore3DText;
    [SerializeField] private TextMeshPro timer3DText;

    [Header("VFX References")]
    [SerializeField] private GameObject localGoalVFX;
    [SerializeField] private GameObject visitantGoalVFX;

    public MatchStatePacket Packet;

    private void Awake()
    {
        Packet = new MatchStatePacket();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitMatch();
    }

    private void Update()
    {
        if (updateGO)
        {
            UpdateGameObjectData();
            updateGO = false;
        }
        UpdateMatchPacket();
    }

    void InitMatch()
    {
        //currentMatchTime = GameManager.Instance.gm_MaxTime;
        currentMatchTime = 300f;
        UpdateScoreUI();
        UpdateTimerUI();

        matchTimerCoroutine = StartCoroutine(MatchTimer());
    }

    private void UpdateScoreUI()
    {
        localScoreText.text = $"Local: {GameManager.Instance.gm_LocalGoals}";
        visitantScoreText.text = $"Visitant: {GameManager.Instance.gm_VisitantGoals}";

        localScore3DText.text = GameManager.Instance.gm_LocalGoals.ToString();
        visitantScore3DText.text = GameManager.Instance.gm_VisitantGoals.ToString();

        Packet.LocalGoals = GameManager.Instance.gm_LocalGoals;
        Packet.VisitantGoals = GameManager.Instance.gm_VisitantGoals;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentMatchTime / 60f);
        int seconds = Mathf.FloorToInt(currentMatchTime % 60f);

        string timeText = $"{minutes:D2}:{seconds:D2}";

        timerText.text = timeText;
        timer3DText.text = timeText;

        //Debug.Log($"Updated Timer UI & 3D Text - Time Left: {timeText}");

        timerText.ForceMeshUpdate();
    }

    public void IncrementLocalScore()
    {
        GameManager.Instance.gm_LocalGoals++;
        UpdateScoreUI();

        TriggerGoalVFX(localGoalVFX);

        ResetBallPosition();
    }

    public void IncrementVisitantScore()
    {
        GameManager.Instance.gm_VisitantGoals++;
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
        GameState st = GameManager.Instance.gm_LocalGoals > GameManager.Instance.gm_VisitantGoals ? GameState.WON : GameState.LOOSE;
        GameManager.Instance.SetGameState(st);

        Debug.Log("Match Ended!");
        Debug.Log($"Final Score: Local {GameManager.Instance.gm_LocalGoals} - {GameManager.Instance.gm_VisitantGoals} Visitant");

        RestartMatch();
    }

    public void RestartMatch()
    {
        // Reset the scores
        GameManager.Instance.gm_LocalGoals = 0;
        GameManager.Instance.gm_VisitantGoals = 0;
        UpdateScoreUI();

        // Reset the timer
        currentMatchTime = GameManager.Instance.gm_MaxTime;
        UpdateTimerUI();

        // Reset the ball to the center
        ResetBallPosition();

        // Restart the match timer
        matchTimerCoroutine = StartCoroutine(MatchTimer());

        // Reset the game state
        GameManager.Instance.SetGameState(GameState.IN_GAME);
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

    void UpdateGameObjectData()
    {
        currentMatchTime = Packet.CurrentTime;
        ball.transform.position = new Vector3(Packet.BallPosition[0], Packet.BallPosition[1], Packet.BallPosition[2]);
    }

    void UpdateMatchPacket()
    {
        Packet.CurrentTime = currentMatchTime;
        Packet.BallPosition[0] = ball.transform.position.x;
        Packet.BallPosition[1] = ball.transform.position.y;
        Packet.BallPosition[2] = ball.transform.position.z;
    } 
}
