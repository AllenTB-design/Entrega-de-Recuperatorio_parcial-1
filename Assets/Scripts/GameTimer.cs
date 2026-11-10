
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameTimer : NetworkBehaviour
{
    [Header("Duracion de la partida")]
    public float matchDuration = 180f;

    public NetworkVariable<float> TimeRemaining =
        new NetworkVariable<float>(
            180f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<bool> IsFinished =
        new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            TimeRemaining.Value = matchDuration;
            IsFinished.Value = false;
        }
    }

    private void Update()
    {
        if (!IsServer || IsFinished.Value)
            return;

        int connectedPlayers =
            NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (connectedPlayers < 2)
            return;

        if (TimeRemaining.Value > 0f)
        {
            TimeRemaining.Value -= Time.deltaTime;
        }
        else
        {
            TimeRemaining.Value = 0f;
            IsFinished.Value = true;

            Debug.Log("¡La partida ha terminado!");
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RestartGameRpc()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            "Escena del Nivel",
            LoadSceneMode.Single
        );
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ReturnToMenuRpc()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted +=
            OnMainMenuLoaded;

        NetworkManager.Singleton.SceneManager.LoadScene(
            "MainMenu",
            LoadSceneMode.Single
        );
    }

    private void OnMainMenuLoaded(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (sceneName != "MainMenu")
            return;

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -=
            OnMainMenuLoaded;

        NetworkManager.Singleton.Shutdown();

        Debug.Log("Sesión multijugador cerrada correctamente.");
    }
}
