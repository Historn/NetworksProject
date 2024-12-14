using UnityEngine;

namespace HyperStrike
{
    public class GoalZone : MonoBehaviour
    {
        [SerializeField] private bool isLocalGoal; // TRUE = Local ; FALSE = Visitor
        [SerializeField] private GameManager gameManager;

        private void OnTriggerEnter(Collider other)
        {
            // Check if the ball enters the goal
            if (other.CompareTag("Ball"))
            {
                if (isLocalGoal)
                {
                    gameManager.IncrementLocalScore();
                }
                else
                {
                    gameManager.IncrementVisitantScore();
                }
            }
        }
    }
}
