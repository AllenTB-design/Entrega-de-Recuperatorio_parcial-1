
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ResultsButtons : MonoBehaviour
{
    public GameTimer gameTimer;

    public void RestartGame()
    {
        if (gameTimer != null)
        {
            gameTimer.RestartGameRpc();
        }
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
