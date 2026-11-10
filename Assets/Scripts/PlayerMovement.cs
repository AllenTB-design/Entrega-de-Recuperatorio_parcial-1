
using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 7f;

    [Header("Controls")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;

    private Rigidbody rb;
    private bool isGrounded;
    private GameTimer gameTimer;
    private Coroutine stunCoroutine;
    public bool IsLocallyPaused = false;

    // Estado de aturdimiento sincronizado entre jugadores.
    public NetworkVariable<bool> IsStunned = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool MatchEnded()
    {
        if (gameTimer == null)
        {
            gameTimer = FindFirstObjectByType<GameTimer>();
        }

        return gameTimer != null && gameTimer.IsFinished.Value;
    }

    public override void OnNetworkSpawn()
    {
        ApplyPlayerColor();

        if (OwnerClientId == 0)
        {
            // Player 1: Host
            forwardKey = KeyCode.A;
            backwardKey = KeyCode.D;
            leftKey = KeyCode.S;
            rightKey = KeyCode.W;
            jumpKey = KeyCode.Space;
        }
        else
        {
            // Player 2: Client
            forwardKey = KeyCode.LeftArrow;
            backwardKey = KeyCode.RightArrow;
            leftKey = KeyCode.DownArrow;
            rightKey = KeyCode.UpArrow;
            jumpKey = KeyCode.RightControl;
        }

        if (!IsOwner)
            return;

        StartCoroutine(PlaceAtSpawn());
    }

    // Activa el aturdimiento. Solo puede llamarlo el servidor.
    public void StunPlayer(float duration)
    {
        if (!IsServer)
            return;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }

        stunCoroutine = StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        IsStunned.Value = true;

        yield return new WaitForSeconds(duration);

        IsStunned.Value = false;
        stunCoroutine = null;

        Debug.Log("El jugador " + OwnerClientId + " recuperó el movimiento.");
    }

    private IEnumerator PlaceAtSpawn()
    {
        while (SceneManager.GetActiveScene().name != "Escena del Nivel")
        {
            yield return null;
        }

        yield return null;

        string spawnName = OwnerClientId == 0
            ? "SpawnPoint1"
            : "SpawnPoint2";

        GameObject spawnPoint = GameObject.Find(spawnName);

        if (spawnPoint != null)
        {
            Vector3 spawnPosition = spawnPoint.transform.position;

            rb.position = spawnPosition;

            GetComponent<NetworkTransform>().Teleport(
                spawnPosition,
                transform.rotation,
                transform.localScale
            );

            Debug.Log("Posición del jugador: " + transform.position);
        }
        else
        {
            Debug.LogError("No se encontró " + spawnName);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner || IsLocallyPaused || MatchEnded() || IsStunned.Value)
            return;

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (IsLocallyPaused || MatchEnded() || IsStunned.Value)
        {
            Vector3 stoppedVelocity = rb.linearVelocity;
            stoppedVelocity.x = 0f;
            stoppedVelocity.z = 0f;
            rb.linearVelocity = stoppedVelocity;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(leftKey)) horizontal -= 1f;
        if (Input.GetKey(rightKey)) horizontal += 1f;
        if (Input.GetKey(forwardKey)) vertical += 1f;
        if (Input.GetKey(backwardKey)) vertical -= 1f;

        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 velocity = rb.linearVelocity;
        velocity.x = movement.x * moveSpeed;
        velocity.z = movement.z * moveSpeed;

        rb.linearVelocity = velocity;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted +=
                OnSceneLoadCompleted;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -=
                OnSceneLoadCompleted;
        }
    }

    private void OnSceneLoadCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (sceneName != "Escena del Nivel")
            return;

        if (IsServer)
        {
            PlayerScore[] players = FindObjectsByType<PlayerScore>(
                FindObjectsSortMode.None
            );

            foreach (PlayerScore player in players)
            {
                if (player.IsSpawned)
                {
                    player.Score.Value = 0;
                }
            }
        }

        ApplyPlayerColor();

        if (IsOwner)
        {
            StartCoroutine(PlaceAtSpawn());
        }
    }

    private void ApplyPlayerColor()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        Color playerColor = OwnerClientId == 0
            ? Color.blue
            : Color.red;

        foreach (Renderer rend in renderers)
        {
            rend.material.color = playerColor;
        }
    }
}
