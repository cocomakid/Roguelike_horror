using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("타일맵 연결")]
    public Tilemap groundTilemap;
    public Tilemap wallTilemap;

    [Header("사용할 타일 에셋")]
    public TileBase groundTile;
    public TileBase wallTile;

    [Header("맵 크기 전체 설정")]
    public int mapWidth = 60;
    public int mapHeight = 40;

    [Header("방 설정")]
    public int minRoomSize = 5;
    public int maxRoomSize = 10;
    public int maxRooms = 8; // 생성할 방의 개수

    [Header("Enemy Spawn Settings")]
    public GameObject enemyPrefab; // 일반 적 프리팹
    public int enemySpawnCount = 10; // 생성할 일반 적 마리 수

    // ⭐ 변수 이름의 오타 방지를 위해 기존 선언 그대로 유지합니다.
    [Header("Enemy Ghost Spawn Settings")]
    public GameObject enemyghostPrefab; // 유령 적 프리팹
    public int enemyghostCount = 10; // 생성할 유령 적 마리 수

    [Header("Key Spawn Settings")]
    public GameObject keyPrefab;
    public int keySpawnCount = 10;

    [Header("Decoration Spawn Settings")]
    public GameObject[] decorationPrefabs;
    public int decorationSpawnCount = 10;

    [Header("플레이어")]
    public GameObject player;

    private int[,] mapData;
    private List<RectInt> rooms = new List<RectInt>();

    void Start()
    {
        GenerateMap();
        RenderMap();       // 2. 타일맵 시각화
        SpawnEnemies();    // 3. 일반 적 소환
        SpawnEnemyGhosts(); // 🛠️ [추가] 유령 적 소환 함수 실행!
        SpawnKey();        // 4. 키 소환
        Decoration();
    }

    public void GenerateMap()
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        rooms.Clear();

        mapData = new int[mapWidth, mapHeight];

        for (int i = 0; i < maxRooms; i++)
        {
            int w = Random.Range(minRoomSize, maxRoomSize);
            int h = Random.Range(minRoomSize, maxRoomSize);
            int x = Random.Range(2, mapWidth - w - 2);
            int y = Random.Range(2, mapHeight - h - 2);

            RectInt newRoom = new RectInt(x, y, w, h);

            bool intersects = false;
            foreach (var room in rooms)
            {
                if (newRoom.Overlaps(room))
                {
                    intersects = true;
                    break;
                }
            }

            if (!intersects)
            {
                for (int rx = newRoom.x; rx < newRoom.xMax; rx++)
                {
                    for (int ry = newRoom.y; ry < newRoom.yMax; ry++)
                    {
                        mapData[rx, ry] = 1;
                    }
                }

                if (rooms.Count > 0)
                {
                    Vector2Int prevCenter = GetRoomCenter(rooms[rooms.Count - 1]);
                    Vector2Int currCenter = GetRoomCenter(newRoom);

                    CreateCorridor(prevCenter.x, currCenter.x, prevCenter.y, true);
                    CreateCorridor(prevCenter.y, currCenter.y, currCenter.x, false);
                }

                rooms.Add(newRoom);
            }
        }

        RenderMap();

        if (rooms.Count > 0 && player != null)
        {
            Vector2Int startPos = GetRoomCenter(rooms[0]);
            player.transform.position = new Vector3(startPos.x + 0.5f, startPos.y + 0.5f, 0);
        }
    }

    void CreateCorridor(int start, int end, int constant, bool isHorizontal)
    {
        int min = Mathf.Min(start, end);
        int max = Mathf.Max(start, end);

        for (int i = min; i <= max; i++)
        {
            if (isHorizontal)
                mapData[i, constant] = 1;
            else
                mapData[constant, i] = 1;
        }
    }

    Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
    }

    void RenderMap()
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
        }

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapData[x, y] == 1)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    wallTilemap.SetTile(tilePos, null);
                    groundTilemap.SetTile(tilePos, groundTile);
                }
            }
        }
    }

    // 💡 헬퍼 함수: 중복 코드를 줄이기 위해 맵의 모든 바닥 좌표를 가져오는 함수입니다.
    List<Vector2Int> GetValidGroundPositions()
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapData[x, y] == 1)
                {
                    validPositions.Add(new Vector2Int(x, y));
                }
            }
        }
        return validPositions;
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab이 MapGenerator에 등록되지 않았습니다!");
            return;
        }

        List<Vector2Int> validGroundPositions = GetValidGroundPositions();
        if (validGroundPositions.Count == 0) return;

        int spawnedCount = 0;
        while (spawnedCount < enemySpawnCount && validGroundPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validGroundPositions.Count);
            Vector2Int spawnGridPos = validGroundPositions[randomIndex];
            Vector3 spawnWorldPos = new Vector3(spawnGridPos.x + 0.5f, spawnGridPos.y + 0.5f, 0f);

            Instantiate(enemyPrefab, spawnWorldPos, Quaternion.identity);
            validGroundPositions.RemoveAt(randomIndex);
            spawnedCount++;
        }

        Debug.Log($"{spawnedCount}마리의 일반 적이 성공적으로 스폰되었습니다.");
    }

    // 🛠️ [신규 추가] 유령 적을 스폰하는 독립적인 함수입니다!
    void SpawnEnemyGhosts()
    {
        if (enemyghostPrefab == null)
        {
            Debug.LogWarning("Enemy Ghost Prefab이 MapGenerator에 등록되지 않았습니다!");
            return;
        }

        List<Vector2Int> validGroundPositions = GetValidGroundPositions();
        if (validGroundPositions.Count == 0) return;

        int spawnedCount = 0;
        while (spawnedCount < enemyghostCount && validGroundPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validGroundPositions.Count);
            Vector2Int spawnGridPos = validGroundPositions[randomIndex];
            Vector3 spawnWorldPos = new Vector3(spawnGridPos.x + 0.5f, spawnGridPos.y + 0.5f, 0f);

            // 유령 적 프리팹 생성
            Instantiate(enemyghostPrefab, spawnWorldPos, Quaternion.identity);

            // 일반 적이나 다른 유령과 스폰 자리가 겹치지 않도록 리스트에서 제거
            validGroundPositions.RemoveAt(randomIndex);
            spawnedCount++;
        }

        Debug.Log($"{spawnedCount}마리의 유령 적이 성공적으로 스폰되었습니다.");
    }

    void SpawnKey()
    {
        if (keyPrefab == null)
        {
            Debug.LogWarning("key Prefab이 MapGenerator에 등록되지 않았습니다!");
            return;
        }

        List<Vector2Int> validGroundPositions = GetValidGroundPositions();
        if (validGroundPositions.Count == 0) return;

        int spawnedCount = 0;
        while (spawnedCount < keySpawnCount && validGroundPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validGroundPositions.Count);
            Vector2Int spawnGridPos = validGroundPositions[randomIndex];
            Vector3 spawnWorldPos = new Vector3(spawnGridPos.x + 0.5f, spawnGridPos.y + 0.5f, 0f);

            Instantiate(keyPrefab, spawnWorldPos, Quaternion.identity);
            validGroundPositions.RemoveAt(randomIndex);
            spawnedCount++;
        }

        Debug.Log($"{spawnedCount}개의 열쇠가 성공적으로 스폰되었습니다.");
    }

    void Decoration()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0)
        {
            Debug.LogWarning("Decoration Prefab이 MapGenerator에 등록되지 않았습니다!");
            return;
        }

        List<Vector2Int> validGroundPositions = GetValidGroundPositions();
        if (validGroundPositions.Count == 0) return;

        int spawnedCount = 0;
        while (spawnedCount < decorationSpawnCount && validGroundPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validGroundPositions.Count);
            Vector2Int spawnGridPos = validGroundPositions[randomIndex];
            Vector3 spawnWorldPos = new Vector3(spawnGridPos.x + 0.5f, spawnGridPos.y + 0.5f, 0f);

            GameObject randomDecoration = decorationPrefabs[Random.Range(0, decorationPrefabs.Length)];
            Instantiate(randomDecoration, spawnWorldPos, Quaternion.identity);

            validGroundPositions.RemoveAt(randomIndex);
            spawnedCount++;
        }

        Debug.Log($"{spawnedCount}개의 데코레이션이 성공적으로 스폰되었습니다.");
    }
}