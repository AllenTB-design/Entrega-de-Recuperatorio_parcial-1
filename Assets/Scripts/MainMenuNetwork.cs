
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

public class MainMenuNetwork : MonoBehaviour
{
    [Header("Menu")]
    public TMP_InputField ipInput;

    [Header("Connection")]
    public ushort port = 7777;

    private UnityTransport transport;

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No se encontró el NetworkManager en la escena.");
            return;
        }

        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport == null)
        {
            Debug.LogError("No se encontró Unity Transport en el NetworkManager.");
        }
    }

    public void StartHost()
    {
        transport.SetConnectionData(
            "127.0.0.1",
            port,
            "0.0.0.0"
        );

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                "Escena del Nivel",
                LoadSceneMode.Single
            );
        }
        else
        {
            Debug.LogError("No se pudo iniciar el Host.");
        }
    }

    public void StartClient()
    {
        string ip = ipInput.text.Trim();

        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogError("Ingresá una dirección IP.");
            return;
        }

        transport.SetConnectionData(ip, port);
        NetworkManager.Singleton.StartClient();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
