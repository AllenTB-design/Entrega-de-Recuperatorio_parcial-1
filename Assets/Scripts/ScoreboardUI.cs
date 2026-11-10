
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class ScoreboardUI : MonoBehaviour
{
    [Header("Textos del marcador")]
    public TMP_Text player1Text;
    public TMP_Text player2Text;

    private void Update()
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening)
        {
            return;
        }

        PlayerScore[] players = FindObjectsByType<PlayerScore>(
            FindObjectsSortMode.None
        );

        players = players
            .Where(player => player.IsSpawned)
            .OrderBy(player => player.OwnerClientId)
            .ToArray();

        player1Text.text = "Player 1: " +
            (players.Length > 0 ? players[0].Score.Value : 0);

        player2Text.text = "Player 2: " +
            (players.Length > 1 ? players[1].Score.Value : 0);
    }
}
