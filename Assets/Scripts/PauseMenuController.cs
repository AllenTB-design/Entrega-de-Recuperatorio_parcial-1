
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Panel de pausa")]
    public GameObject pausePanel;

    private bool isPaused = false;
    private PlayerMovement localPlayer;
    private GameTimer gameTimer;

    private void Start()
    {
        pausePanel.SetActive(false);

        gameTimer = FindFirstObjectByType<GameTimer>();

        PlayerMovement[] players =
            FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement player in players)
        {
            if (player.IsOwner)
            {
                localPlayer = player;
                break;
            }
        }
    }

    private void Update()
    {
        if (localPlayer == null)
            return;

        // Si terminó la partida, cerrar la pausa y no permitir abrirla.
        if (gameTimer != null && gameTimer.IsFinished.Value)
        {
            if (isPaused)
            {
                ContinueGame();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        pausePanel.SetActive(isPaused);
        localPlayer.IsLocallyPaused = isPaused;
    }

    public void ContinueGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);
        localPlayer.IsLocallyPaused = false;
    }

    public void ReturnToMenu()
    {
        if (NetworkManager.Singleton == null)
            return;

        // Si es el Host, vuelve al menú junto con el Client.
        if (NetworkManager.Singleton.IsServer)
        {
            if (gameTimer != null)
            {
                gameTimer.ReturnToMenuRpc();
            }

            return;
        }

        // Si es el Client, solamente él se desconecta.
        NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("MainMenu");
    }
}
