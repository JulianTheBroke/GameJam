using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cut wire → gate opens → dual-ledge parkour → walls run → linked plates stop walls.
// Does not teleport on start when the chase is appended after a tutorial.
public class ChaseDirector : MonoBehaviour
{
    [SerializeField] PlatformerController player1;
    [SerializeField] PlatformerController player2;
    [SerializeField] CoopDoor startGate;
    [SerializeField] KillWall wallLeft;
    [SerializeField] KillWall wallRight;
    [SerializeField] float wallSpawnInterval = 3f;
    [SerializeField] PressurePlate plateLeft;
    [SerializeField] PressurePlate plateRight;
    [SerializeField] Transform startP1;
    [SerializeField] Transform startP2;
    [SerializeField] Transform chaseP1;
    [SerializeField] Transform chaseP2;
    [SerializeField] bool teleportOnStart;
    [SerializeField] float enterWorldZ = 68f;
    [SerializeField] float fallY = -1f;

    int checkpoint;
    bool chaseStarted;
    bool finished;
    bool inParkour;
    bool winShown;
    float respawnCooldown;
    Coroutine spawnRoutine;

    [SerializeField] PlayerConnection connection;
    [SerializeField] GameHud hud;

    Vector3 leftSpawnPos;
    Vector3 rightSpawnPos;
    Quaternion leftSpawnRot;
    Quaternion rightSpawnRot;
    Transform wallParent;
    KillWall leftSpawnTemplate;
    KillWall rightSpawnTemplate;
    readonly List<KillWall> spawnedWalls = new();

    void Start()
    {
        CacheWallSpawns();
        BuildSpawnTemplates();
        if (teleportOnStart)
            Teleport(startP1, startP2);
    }

    void CacheWallSpawns()
    {
        if (wallLeft != null)
        {
            leftSpawnPos = wallLeft.SpawnPosition;
            leftSpawnRot = wallLeft.transform.rotation;
            wallParent = wallLeft.transform.parent;
        }

        if (wallRight != null)
        {
            rightSpawnPos = wallRight.SpawnPosition;
            rightSpawnRot = wallRight.transform.rotation;
        }
    }

    void BuildSpawnTemplates()
    {
        if (wallLeft != null)
            leftSpawnTemplate = CreateSpawnTemplate(wallLeft, "KillWallL_Template");
        if (wallRight != null)
            rightSpawnTemplate = CreateSpawnTemplate(wallRight, "KillWallR_Template");
    }

    KillWall CreateSpawnTemplate(KillWall source, string templateName)
    {
        GameObject templateObj = Instantiate(source.gameObject, source.SpawnPosition, source.transform.rotation, wallParent);
        templateObj.name = templateName;
        templateObj.SetActive(false);
        return templateObj.GetComponent<KillWall>();
    }

    void Update()
    {
        if (player1 == null || player2 == null)
            return;

        if (respawnCooldown > 0f)
            respawnCooldown -= Time.deltaTime;

        float z1 = player1.transform.position.z;
        float z2 = player2.transform.position.z;
        if (!inParkour && (z1 > enterWorldZ || z2 > enterWorldZ))
            inParkour = true;

        if (chaseStarted && !finished && respawnCooldown <= 0f && AnyPlayerFell())
            Respawn();

        if (finished)
            return;

        if (!chaseStarted && startGate != null && startGate.IsOpen)
        {
            chaseStarted = true;
            checkpoint = 2;
            connection?.ActivateParkourPunishment();
            StartWallLoop();
        }

        if (plateLeft != null && plateRight != null
            && plateLeft.IsSatisfied && plateRight.IsSatisfied)
        {
            finished = true;
            checkpoint = 3;
            StopAllWalls();
            if (!winShown)
            {
                winShown = true;
                hud?.ShowWin();
            }
        }
    }

    void StartWallLoop()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        wallLeft?.StopAndReset();
        wallRight?.StopAndReset();
        wallLeft?.Begin();
        wallRight?.Begin();
        spawnRoutine = StartCoroutine(SpawnWallLoop());
    }

    IEnumerator SpawnWallLoop()
    {
        var wait = new WaitForSeconds(wallSpawnInterval);
        while (!finished)
        {
            yield return wait;
            if (finished)
                yield break;
            SpawnWallPair();
        }
    }

    void SpawnWallPair()
    {
        if (leftSpawnTemplate != null)
            spawnedWalls.Add(SpawnWall(leftSpawnTemplate, leftSpawnPos, leftSpawnRot));
        if (rightSpawnTemplate != null)
            spawnedWalls.Add(SpawnWall(rightSpawnTemplate, rightSpawnPos, rightSpawnRot));
    }

    KillWall SpawnWall(KillWall template, Vector3 spawnPos, Quaternion spawnRot)
    {
        GameObject cloneObj = Instantiate(template.gameObject, spawnPos, spawnRot, wallParent);
        cloneObj.SetActive(true);
        KillWall wall = cloneObj.GetComponent<KillWall>();
        wall.name = template.name.Replace("_Template", "_wave") + $"_{spawnedWalls.Count + 1}";
        wall.SetSpawnPosition(spawnPos);
        wall.Begin();
        return wall;
    }

    void StopAllWalls()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        wallLeft?.StopAndReset();
        wallRight?.StopAndReset();
        ClearSpawnedWalls();
    }

    void ClearSpawnedWalls()
    {
        for (int i = spawnedWalls.Count - 1; i >= 0; i--)
        {
            if (spawnedWalls[i] != null)
                Destroy(spawnedWalls[i].gameObject);
        }

        spawnedWalls.Clear();
    }

    void Respawn()
    {
        respawnCooldown = 0.75f;
        Teleport(startP1, startP2);
    }

    bool AnyPlayerFell()
    {
        return PlayerFell(player1) || PlayerFell(player2);
    }

    bool PlayerFell(PlatformerController player)
    {
        if (player == null)
            return false;

        float feetY = player.transform.position.y;
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
            feetY += cc.center.y - cc.height * 0.5f;

        return feetY < fallY;
    }

    void Teleport(Transform a, Transform b)
    {
        if (a != null) player1.Teleport(a.position);
        if (b != null) player2.Teleport(b.position);
    }
}
