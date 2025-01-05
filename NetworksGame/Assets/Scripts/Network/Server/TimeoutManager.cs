using System.Collections.Generic;
using UnityEngine;

namespace HyperStrike
{
    public class TimeoutManager
    {
        private Dictionary<int, float> playerLastActivity = new Dictionary<int, float>();
        private float timeoutThreshold = 5f; // Timeout after 10 seconds of inactivity

        public void UpdateActivity(int playerId)
        {
            if (playerLastActivity.ContainsKey(playerId))
                playerLastActivity[playerId] = Time.time;
            else
                playerLastActivity.Add(playerId, Time.time);
        }

        public List<int> CheckTimeouts()
        {
            List<int> timedOutPlayers = new List<int>();

            foreach (var entry in playerLastActivity)
            {
                if (Time.time - entry.Value > timeoutThreshold)
                    timedOutPlayers.Add(entry.Key);
            }

            // Remove timed-out players from the dictionary
            foreach (int playerId in timedOutPlayers)
                playerLastActivity.Remove(playerId);

            return timedOutPlayers;
        }
    }
}
