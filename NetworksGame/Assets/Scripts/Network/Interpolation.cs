using UnityEngine;

namespace HyperStrike
{
    public class Interpolation
    {
        private float baseSpeed = 30f;
        private float distanceFactor = 10f;

        public bool IsStateChanged(Vector3 currentPosition, Vector3 packetPosition) { return currentPosition != packetPosition ? true : false; }

        public bool IsStateChanged(Quaternion currentRotation, Quaternion packetRotation) { return currentRotation != packetRotation ? true : false; }

        public Vector3 Interpolate(Vector3 currentPosition, Vector3 targetPosition)
        {
            float distance = Vector3.Distance(currentPosition, targetPosition);
            float dynamicSpeed = baseSpeed + distance * distanceFactor;
            return Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * dynamicSpeed);
        }

        public Quaternion Interpolate(Quaternion currentRotation, Quaternion targetRotation)
        {
            return Quaternion.Lerp(currentRotation, targetRotation, Time.deltaTime * baseSpeed);
        }

        public void SetBaseSpeed(float speed)
        {
            baseSpeed = speed;
        }
    }
}
