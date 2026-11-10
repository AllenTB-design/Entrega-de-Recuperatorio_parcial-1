
using UnityEngine;
using Unity.Netcode;

public class PlayerScore : NetworkBehaviour
{
    public NetworkVariable<int> Score = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public void AddPoints(int points)
    {
        if (!IsServer)
            return;

        Score.Value += points;

        Debug.Log(
            "Jugador " + OwnerClientId +
            " tiene " + Score.Value + " puntos."
        );
    }
}
