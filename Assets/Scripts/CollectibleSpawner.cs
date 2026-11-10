
using UnityEngine;
using Unity.Netcode;

public class CollectibleSpawner : NetworkBehaviour
{
    [Header("Coleccionables")]
    public GameObject[] collectiblePrefabs;

    [Header("Aparicion")]
    public float spawnInterval = 3f;
    public int maxCollectibles = 10;

    [Header("Zona del suelo")]
    public Vector3 areaCenter = Vector3.zero;
    public Vector3 areaSize = new Vector3(50f, 0f, 48f);
    public float spawnHeight = 2.5f;

    [Header("Plataformas de puntaje")]
    public Transform platformParent;
    [Range(0f, 1f)]
    public float platformSpawnChance = 0.4f;

    [Header("Evitar obstaculos")]
    public LayerMask obstacleMask;
    public float spawnCheckRadius = 0.6f;
    public int maxSpawnAttempts = 20;

    [Header("Deteccion de superficies")]
    public float raycastHeight = 5f;
    public float raycastDistance = 20f;
    public float collectibleHeight = 0.9f;

    private float timer;

    private void Update()
    {
        if (!IsServer)
            return;
        
        GameTimer gameTimer = GetComponent<GameTimer>();

        if (gameTimer != null && gameTimer.IsFinished.Value)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnCollectible();
        }
    }

    private void SpawnCollectible()
    {
        if (collectiblePrefabs == null ||
            collectiblePrefabs.Length == 0)
            return;

        int currentCount = FindObjectsByType<Collectible>(
            FindObjectsSortMode.None
        ).Length;

        if (currentCount >= maxCollectibles)
            return;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 position;

            bool usePlatform =
                platformParent != null &&
                Random.value < platformSpawnChance;

            bool foundPosition = usePlatform
                ? TryPlatformPosition(out position)
                : TryFloorPosition(out position);

            if (!foundPosition)
                continue;

            int randomIndex = Random.Range(
                0, collectiblePrefabs.Length
            );

            GameObject prefab = collectiblePrefabs[randomIndex];

            GameObject newCollectible = Instantiate(
                prefab,
                position,
                Quaternion.identity
            );

            NetworkObject networkObject =
                newCollectible.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.Spawn();
            }
            else
            {
                Destroy(newCollectible);
                Debug.LogError("Falta NetworkObject en el coleccionable.");
            }

            return;
        }

        Debug.Log("No se encontró una posición libre.");
    }

    private bool TryFloorPosition(out Vector3 position)
    {
        position = new Vector3(
            areaCenter.x + Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            spawnHeight,
            areaCenter.z + Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
        );

        return IsPositionFree(position, null);
    }

    private bool TryPlatformPosition(out Vector3 position)
    {
        position = Vector3.zero;

        Collider[] platforms =
            platformParent.GetComponentsInChildren<Collider>();

        if (platforms.Length == 0)
        {
            Debug.LogWarning("No se encontraron plataformas.");
            return false;
        }

        Collider platform = platforms[
            Random.Range(0, platforms.Length)
        ];

        Bounds bounds = platform.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);

        Vector3 rayOrigin = new Vector3(
            x,
            bounds.max.y + raycastHeight,
            z
        );

        RaycastHit hit;

        if (!Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            raycastDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            Debug.LogWarning("No se detectó ninguna superficie.");
            return false;
        }

        if (hit.collider != platform)
        {
            Debug.LogWarning(
                "El Raycast detectó otra superficie: " + hit.collider.name +
                ". Se esperaba: " + platform.name
            );

            return false;
        }

        position = hit.point + hit.normal * collectibleHeight;

        return IsPositionFree(position, platform);
    }

    private bool IsPositionFree(Vector3 position, Collider ignoredCollider)
    {
        Collider[] overlaps = Physics.OverlapSphere(
            position,
            spawnCheckRadius,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider overlap in overlaps)
        {
            if (overlap != ignoredCollider)
            {
                return false;
            }
        }

        return true;
    }
}