using UnityEngine;

namespace HyperStrike
{
    public class PredictionManager
    {
        public Vector3 PredictPosition(Vector3 currentPosition, Vector3 velocity, float deltaTime)
        {
            return currentPosition + velocity * deltaTime;
        }

        public PlayerState PredictPlayerState(PlayerState currentState, float elapsedTime)
        {
            // Example logic for predicting states
            return currentState == PlayerState.RUNNING && elapsedTime > 1f ? PlayerState.IDLE : currentState;
        }
    }
}