using HyperStrike;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Match : MonoBehaviour
{
    Interpolation interpolation = new Interpolation();

    private Coroutine matchTimerCoroutine;

    [Header("Match Conditions")]
    public float maxTime = 300f; // 300 = 5 minutes in seconds
    private float currentMatchTime = 300f;
    [HideInInspector] public int localGoals = 0;
    [HideInInspector] public int visitantGoals = 0;

    [Header("Match Settings")]
    public List<Player> localPlayers = new List<Player>(); // Use Players ID
    public List<Player> visitantPlayers = new List<Player>(); // Use Players ID
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
    public bool updateGO = false;

    Rigidbody ballRigidbody;
    BallController ballController;

    private void Awake()
    {
        Packet = new MatchStatePacket();
    }

    // Start is called before the first frame update
    void Start()
    {
        ballRigidbody = ball.GetComponent<Rigidbody>();
        ballController = ball.GetComponent<BallController>();
        InitMatch();
    }

    private void Update()
    {
        if (updateGO)
        {
            UpdateGameObjectData();
            updateGO = false;
        }
        UpdatePacket();
    }

    void InitMatch()
    {
        //currentMatchTime = GameManager.Instance.gm_MaxTime;
        currentMatchTime = maxTime;
        UpdateScoreUI();
        UpdateTimerUI();

        matchTimerCoroutine = StartCoroutine(MatchTimer());
    }

    private void UpdateScoreUI()
    {
        localScoreText.text = $"Local: {localGoals}";
        visitantScoreText.text = $"Visitant: {visitantGoals}";

        localScore3DText.text = localGoals.ToString();
        visitantScore3DText.text = visitantGoals.ToString();
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
        localGoals++;

        int id = ballController.Packet.LastHitPlayerId;
        if (NetworkManager.Instance.nm_ActivePlayers.ContainsKey(id))
        {
            Player p = NetworkManager.Instance.nm_ActivePlayers[id];
            if (p != null && localPlayers.Contains(p))
            {
                p.Packet.Score += 100;
                p.Packet.Goals++;
            }
        }

        UpdateScoreUI();

        TriggerGoalVFX(localGoalVFX);

        ResetBallPosition();
    }

    public void IncrementVisitantScore()
    {
        visitantGoals++;

        int id = ballController.Packet.LastHitPlayerId;
        if (NetworkManager.Instance.nm_ActivePlayers.ContainsKey(id))
        {
            Player p = NetworkManager.Instance.nm_ActivePlayers[id];
            if (p != null && visitantPlayers.Contains(p))
            {
                p.Packet.Score += 100;
                p.Packet.Goals++;
            }
        }

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
        GameState st = localGoals > visitantGoals ? GameState.WON : GameState.LOOSE;
        GameManager.Instance.SetGameState(st);

        Debug.Log("Match Ended!");
        Debug.Log($"Final Score: Local {localGoals} - {visitantGoals} Visitant");

        RestartMatch();
    }

    public void RestartMatch()
    {
        // Reset the scores
        localGoals = 0;
        visitantGoals = 0;
        UpdateScoreUI();

        // Reset the timer
        currentMatchTime = maxTime;
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
        if (ballRigidbody != null)
        {
            ballRigidbody.position = ballStartPosition.position;
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
        }
        ballController.Packet.LastHitPlayerId = -1;
    }

    void UpdatePlayerScore()
    {
        if (NetworkManager.Instance.nm_ActivePlayers.ContainsKey(ballController.Packet.LastHitPlayerId))
        {
            NetworkManager.Instance.nm_ActivePlayers[ballController.Packet.LastHitPlayerId].Packet.Score += 100;
            NetworkManager.Instance.nm_ActivePlayers[ballController.Packet.LastHitPlayerId].Packet.Goals += 1;
        }
    }

    void UpdateGameObjectData()
    {
        currentMatchTime = Packet.CurrentTime;
        localGoals = Packet.LocalGoals;
        visitantGoals = Packet.VisitantGoals;
        UpdateScoreUI();
    }

    void UpdatePacket()
    {
        Packet.CurrentTime = currentMatchTime;
        Packet.LocalGoals = localGoals;
        Packet.VisitantGoals = visitantGoals;
    }

    void CheckTeamsChange()
    {
        for (int i = 0; i < localPlayers.Count; i++)
        {
            if (!localPlayers[i].Packet.Team)
            {
                visitantPlayers.Add(localPlayers[i]);
                localPlayers.Remove(localPlayers[i]);
            }
        }
        
        for (int i = 0; i < visitantPlayers.Count; i++)
        {
            if (visitantPlayers[i].Packet.Team)
            {
                localPlayers.Add(visitantPlayers[i]);
                visitantPlayers.Remove(visitantPlayers[i]);
            }
        }
    }
}
