
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Linq;

public class ResultsUI : MonoBehaviour
{
    [Header("Panel de resultados")]
    public GameObject resultsPanel;

    [Header("Textos")]
    public TMP_Text winnerText;
    public TMP_Text scoresText;

    private bool resultsShown = false;

    private void Update()
    {
        if (resultsShown)
            return;

        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening)
            return;

        GameTimer timer = FindFirstObjectByType<GameTimer>();

        if (timer == null || !timer.IsFinished.Value)
            return;

        ShowResults();
    }

    private void ShowResults()
    {
        PlayerScore[] players = FindObjectsByType<PlayerScore>(
            FindObjectsSortMode.None
        );

        players = players
            .Where(player => player.IsSpawned)
            .OrderBy(player => player.OwnerClientId)
            .ToArray();

        int player1Score = players.Length > 0
            ? players[0].Score.Value
            : 0;

        int player2Score = players.Length > 1
            ? players[1].Score.Value
            : 0;

        if (player1Score > player2Score)
            winnerText.text = "¡GANA EL PLAYER 1!";

        else if (player2Score > player1Score)
            winnerText.text = "¡GANA EL PLAYER 2!";

        else
            winnerText.text = "¡EMPATE!";

        scoresText.text =
            "Player 1: " + player1Score + " puntos\n\n" +
            "Player 2: " + player2Score + " puntos";

        resultsPanel.SetActive(true);

        resultsShown = true;
    }
}
