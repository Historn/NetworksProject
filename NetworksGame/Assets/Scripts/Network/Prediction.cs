using UnityEngine;

namespace HyperStrike
{
    public class Prediction
    {
        /// <summary>
        /// Predicts the position based on current position, velocity, and delta time.
        /// </summary>
        public Vector3 PredictPosition(Vector3 currentPosition, Vector3 velocity, float deltaTime)
        {
            return currentPosition + velocity * deltaTime;
        }

        public Vector3 PredictPositionWithCollisions(Vector3 currentPosition, Vector3 velocity, float deltaTime, Vector3 pitchMin, Vector3 pitchMax, ref Vector3 updatedVelocity)
        {
            Vector3 predictedPosition = currentPosition + velocity * deltaTime;

            // Check for collisions with pitch boundaries
            if (predictedPosition.x < pitchMin.x || predictedPosition.x > pitchMax.x)
            {
                // Reflect velocity on the X-axis
                updatedVelocity.x = -velocity.x;
                // Clamp position within bounds
                predictedPosition.x = Mathf.Clamp(predictedPosition.x, pitchMin.x, pitchMax.x);
            }

            if (predictedPosition.y < pitchMin.y || predictedPosition.y > pitchMax.y)
            {
                // Reflect velocity on the Y-axis
                updatedVelocity.y = -velocity.y;
                // Clamp position within bounds
                predictedPosition.y = Mathf.Clamp(predictedPosition.y, pitchMin.y, pitchMax.y);
            }

            if (predictedPosition.z < pitchMin.z || predictedPosition.z > pitchMax.z)
            {
                // Reflect velocity on the Z-axis
                updatedVelocity.z = -velocity.z;
                // Clamp position within bounds
                predictedPosition.z = Mathf.Clamp(predictedPosition.z, pitchMin.z, pitchMax.z);
            }

            return predictedPosition;
        }

        /// <summary>
        /// Predicts the rotation based on current rotation, angular velocity, and delta time.
        /// </summary>
        public Quaternion PredictRotation(Quaternion currentRotation, Vector3 angularVelocity, float deltaTime)
        {
            Quaternion deltaRotation = Quaternion.Euler(angularVelocity * Mathf.Rad2Deg * deltaTime);
            return currentRotation * deltaRotation;
        }

        public PlayerState PredictPlayerState(PlayerState currentState, float elapsedTime)
        {
            // Example logic for predicting states
            return currentState == PlayerState.RUNNING && elapsedTime > 1f ? PlayerState.IDLE : currentState;
        }
    }
}