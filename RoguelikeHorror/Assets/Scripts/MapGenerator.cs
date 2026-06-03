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

    [Header("플레이어")]
    public GameObject player; // Hierarchy의 PF Player를 여기에 드래그 앤 드롭

    private int[,] mapData;
    private List<RectInt> rooms = new List<RectInt>();

    void Start()
    {
        GenerateMap();
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
}