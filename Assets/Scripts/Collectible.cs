
using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class Collectible : NetworkBehaviour
{
    [Header("Puntuacion")]
    public int pointValue = 1;

    [Header("Tipo de coleccionable")]
    public bool isStunCollectible = false;
    public float stunDuration = 3f;

    [Header("Sonidos")]
    public AudioClip positiveSound;
    public AudioClip negativeSound;

    [Header("Tiempo de vida")]
    public float lifetime = 20f;

    private bool collected = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(LifetimeCountdown());
        }
    }

    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);

        if (IsSpawned && !collected)
        {
            NetworkObject.Despawn(true);
        }
    }

    [ClientRpc]
    private void PlayCollectSoundClientRpc(bool positive)
    {
        AudioClip sound = positive ? positiveSound : negativeSound;

        if (sound != null)
        {
            AudioSource.PlayClipAtPoint(sound, transform.position, 4f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || collected)
            return;

        NetworkObject player = other.GetComponent<NetworkObject>();

        if (player == null)
        {
            Debug.LogWarning(
                "El objeto que tocó el coleccionable no tiene NetworkObject: "
                + other.name
            );
            return;
        }

        Debug.Log(
            "Coleccionable tocado por el jugador con ID: "
            + player.OwnerClientId
        );

        if (isStunCollectible)
        {
            PlayerMovement playerMovement =
                player.GetComponent<PlayerMovement>();

            if (playerMovement == null)
            {
                Debug.LogWarning(
                    "El jugador no tiene PlayerMovement."
                );
                return;
            }

            collected = true;

            PlayCollectSoundClientRpc(false);

            playerMovement.StunPlayer(stunDuration);

            Debug.Log(
                "¡Jugador " + player.OwnerClientId +
                " aturdido durante " + stunDuration + " segundos!"
            );
        }
        else
        {
            PlayerScore playerScore = player.GetComponent<PlayerScore>();

            if (playerScore == null)
            {
                Debug.LogWarning(
                    "El jugador no tiene PlayerScore."
                );
                return;
            }

            collected = true;

            PlayCollectSoundClientRpc(pointValue > 0);

            playerScore.AddPoints(pointValue);
        }

        NetworkObject.Despawn(true);
    }
}
