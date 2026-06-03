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
    public GameObject enemyPrefab; // 유니티 에디터에서 드래그앤드롭으로 넣어줄 적 프리팹
    public int enemySpawnCount = 10; // 생성할 적의 마리 수 (원하는 숫자로 조절 가능)

    [Header("플레이어")]
    public GameObject player; // Hierarchy의 PF Player를 여기에 드래그 앤 드롭

    private int[,] mapData;
    private List<RectInt> rooms = new List<RectInt>();

    void Start()
    {
        GenerateMap();
        RenderMap();       // 2. 타일맵 시각화
        SpawnEnemies();    // 3. 적 소환 (이 줄을 추가해 주세요!)
    }

    public void GenerateMap()
    {
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        rooms.Clear();

        // 0: 비어있음/벽, 1: 바닥
        mapData = new int[mapWidth, mapHeight];

        // 1. 겹치지 않는 방들 랜덤 생성
        for (int i = 0; i < maxRooms; i++)
        {
            int w = Random.Range(minRoomSize, maxRoomSize);
            int h = Random.Range(minRoomSize, maxRoomSize);
            int x = Random.Range(2, mapWidth - w - 2);
            int y = Random.Range(2, mapHeight - h - 2);

            RectInt newRoom = new RectInt(x, y, w, h);

            // 다른 방과 겹치는지 체크
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
                // 맵 데이터에 방 파내기 (바닥 = 1)
                for (int rx = newRoom.x; rx < newRoom.xMax; rx++)
                {
                    for (int ry = newRoom.y; ry < newRoom.yMax; ry++)
                    {
                        mapData[rx, ry] = 1;
                    }
                }

                // 이전 방이 있다면 현재 방과 복도로 연결
                if (rooms.Count > 0)
                {
                    Vector2Int prevCenter = GetRoomCenter(rooms[rooms.Count - 1]);
                    Vector2Int currCenter = GetRoomCenter(newRoom);

                    CreateCorridor(prevCenter.x, currCenter.x, prevCenter.y, true);  // 가로 복도
                    CreateCorridor(prevCenter.y, currCenter.y, currCenter.x, false); // 세로 복도
                }

                rooms.Add(newRoom);
            }
        }

        // 2. 바닥(1)의 테두리를 감싸는 외곽 벽(0) 데이터 자동 정렬 및 렌더링
        RenderMap();

        // 3. 첫 번째로 생성된 방의 한가운데로 플레이어 강제 이동
        if (rooms.Count > 0 && player != null)
        {
            Vector2Int startPos = GetRoomCenter(rooms[0]);
            // 타일맵 좌표를 월드 좌표로 변환 (+0.5f는 타일의 중심점 맞추기)
            player.transform.position = new Vector3(startPos.x + 0.5f, startPos.y + 0.5f, 0);
        }
    }

    // 복도 파내는 함수
    void CreateCorridor(int start, int end, int constant, bool isHorizontal)
    {
        int min = Mathf.Min(start, end);
        int max = Mathf.Max(start, end);

        for (int i = min; i <= max; i++)
        {
            if (isHorizontal)
                mapData[i, constant] = 1; // 가로로 바닥 파기
            else
                mapData[constant, i] = 1; // 세로로 바닥 파기
        }
    }

    Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
    }

    void RenderMap()
    {
        // 기존 타일맵 깨끗하게 초기화
        groundTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();

        // 1단계: 맵의 전체 범위(mapWidth x mapHeight)를 무조건 '벽 타일'로 꽉 채워버립니다!
        // 이렇게 하면 맵 너머가 텅 빈 공백이 아니라 '벽 타일이 가득 찬 심연'이 되어 빛이 예쁘게 막힙니다.
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTile);
            }
        }

        // 2단계: 방과 복도 데이터(1)가 있는 자리만 '벽 타일을 지우고' 그 자리에 '바닥 타일'을 깝니다.
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapData[x, y] == 1)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);

                    // 벽 타일맵에서 이 자리를 지워야 바닥이 보입니다.
                    wallTilemap.SetTile(tilePos, null);

                    // 바닥 타일맵에 바닥 타일을 깔아줍니다.
                    groundTilemap.SetTile(tilePos, groundTile);
                }
            }
        }
    }

    void SpawnEnemies()
    {
        // 적 프리팹이 등록되지 않았다면 에러 방지를 위해 리턴
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab이 MapGenerator에 등록되지 않았습니다!");
            return;
        }

        // 1단계: 맵 전체에서 '바닥(1)'인 좌표들을 리스트에 전부 담습니다.
        List<Vector2Int> validGroundPositions = new List<Vector2Int>();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (mapData[x, y] == 1) // 1은 바닥(Ground)을 뜻함
                {
                    // 문제가 되는 줄을 지우고, 아래 Add 줄만 남겨둡니다.
                    validGroundPositions.Add(new Vector2Int(x, y));
                }
            }
        }

        // 만약 바닥 타일이 아예 없다면 소환을 중단합니다.
        if (validGroundPositions.Count == 0) return;

        // 2단계: 설정한 마리 수만큼 랜덤하게 자리를 뽑아 적을 소환합니다.
        int spawnedCount = 0;
        while (spawnedCount < enemySpawnCount && validGroundPositions.Count > 0)
        {
            // 바닥 좌표 리스트 중에서 랜덤한 인덱스 하나 선택
            int randomIndex = Random.Range(0, validGroundPositions.Count);
            Vector2Int spawnGridPos = validGroundPositions[randomIndex];

            // 유니티 세계관 좌표(Vector3)로 변환 (+0.5f를 해줘야 타일의 정중앙에 이쁘게 소환됩니다)
            Vector3 spawnWorldPos = new Vector3(spawnGridPos.x + 0.5f, spawnGridPos.y + 0.5f, 0f);

            // 적 오브젝트 생성!
            Instantiate(enemyPrefab, spawnWorldPos, Quaternion.identity);

            // ⚠️ 중요: 한 자리에 적이 겹쳐서 여러 마리 나오는 것을 막기 위해 
            // 방금 뽑힌 자리는 리스트에서 지워버립니다.
            validGroundPositions.RemoveAt(randomIndex);

            spawnedCount++;
        }

        Debug.Log($"{spawnedCount}마리의 적이 성공적으로 스폰되었습니다.");
    }
}