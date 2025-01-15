using HyperStrike;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] GameObject[] Banners = new GameObject[6]; // Add them alternating the teams
    [SerializeField] TextMeshProUGUI[] LocalPlayerBanner_1 = new TextMeshProUGUI[3];
    [SerializeField] TextMeshProUGUI[] LocalPlayerBanner_2 = new TextMeshProUGUI[3];
    [SerializeField] TextMeshProUGUI[] LocalPlayerBanner_3 = new TextMeshProUGUI[3];

    [SerializeField] TextMeshProUGUI[] VisitantPlayerBanner_1 = new TextMeshProUGUI[3];
    [SerializeField] TextMeshProUGUI[] VisitantPlayerBanner_2 = new TextMeshProUGUI[3];
    [SerializeField] TextMeshProUGUI[] VisitantPlayerBanner_3 = new TextMeshProUGUI[3];

    [SerializeField] TextMeshProUGUI localScoreText;
    [SerializeField] TextMeshProUGUI visitantScoreText;

    // Update is called once per frame
    void Update()
    {
        if (NetworkManager.Instance.nm_ConnectedNewPlayer)
        {
            UpdatePlayersList(); 
            NetworkManager.Instance.nm_ConnectedNewPlayer = false;
        }
        UpdateScores();
    }

    void UpdatePlayersList()
    {
        //foreach (GameObject banner in Banners)
        //{
        //    banner.SetActive(false);
        //}

        int playerCount = NetworkManager.Instance.nm_ActivePlayers.Count;
        for (int i = 0; i < playerCount && i < Banners.Length; i++)
        {
            Banners[i].SetActive(true);
        }

        foreach (var p in NetworkManager.Instance.nm_ActivePlayers.Values)
        {
            if (p.Packet.Team && !NetworkManager.Instance.nm_Match.localPlayers.Contains(p)) 
            {
                NetworkManager.Instance.nm_Match.localPlayers.Add(p);

            }
            else if (!p.Packet.Team && !NetworkManager.Instance.nm_Match.visitantPlayers.Contains(p))
            {
                NetworkManager.Instance.nm_Match.visitantPlayers.Add(p);
            }
        }
    }

    void UpdateScores()
    {
        localScoreText.text = NetworkManager.Instance.nm_Match.localGoals.ToString();
        visitantScoreText.text = NetworkManager.Instance.nm_Match.visitantGoals.ToString();

        // Sort teams by score
        NetworkManager.Instance.nm_Match.localPlayers = NetworkManager.Instance.nm_Match.localPlayers.OrderByDescending(player => player.Packet.Score).ToList();
        NetworkManager.Instance.nm_Match.visitantPlayers = NetworkManager.Instance.nm_Match.visitantPlayers.OrderByDescending(player => player.Packet.Score).ToList();

        // Update UI for local players
        UpdatePlayerBanners(NetworkManager.Instance.nm_Match.localPlayers, LocalPlayerBanner_1, LocalPlayerBanner_2, LocalPlayerBanner_3);

        // Update UI for visitant players
        UpdatePlayerBanners(NetworkManager.Instance.nm_Match.visitantPlayers, VisitantPlayerBanner_1, VisitantPlayerBanner_2, VisitantPlayerBanner_3);
    }

    void UpdatePlayerBanners(List<Player> players, TextMeshProUGUI[] banner1, TextMeshProUGUI[] banner2, TextMeshProUGUI[] banner3)
    {
        var banners = new[] { banner1, banner2, banner3 };

        for (int i = 0; i < banners.Length; i++)
        {
            if (i < players.Count)
            {
                var player = players[i];
                banners[i][0].text = player.Packet.PlayerName; // Name
                banners[i][1].text = player.Packet.Score.ToString(); // Score
                banners[i][2].text = player.Packet.Goals.ToString(); // Goals
                Debug.Log($"Name: {player.Packet.PlayerName}, Score: {player.Packet.Score}, Goals: {player.Packet.Goals}, Team {player.Packet.Team}");
            }
            else
            {
                banners[i][0].text = ""; // Clear Name
                banners[i][1].text = ""; // Clear Score
                banners[i][2].text = ""; // Clear Goals
            }
        }
    }
}
