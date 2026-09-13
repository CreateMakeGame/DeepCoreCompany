using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class VoxelWorld : Singleton<VoxelWorld>
{
    [Header("Soil Layer Settings (토양 층위 설정)")]
    public Gradient soilGradient;           // 토양 색상을 편집할 그라데이션
    public float maxDepth = 30f;            // 토양 층위의 최대 깊이 (이 깊이까지는 토양 색상을 적용)

    [Header("Grid Settings")]
    public int width = 96;                 // X축 길이 (가로), 16의 배수 권장 (16 * 6 = 96)
    public int height = 96;                // Y축 전체 높이 공간
    public int depth = 96;                 // Z축 길이 (세로)
    public float surfaceLevel = 0.5f;       // 땅과 공기를 구분하는 기준값

    [Header("Dig Boundary Settings")]
    public bool useDigBounds = true;                                // 굴착 제한 적용 여부
    public Vector3 digZoneOffset = Vector3.zero;                    // 굴착 제한 영역의 오프셋
    public Vector3 digZoneSize = new Vector3(10f, 20f, 10f);        // 굴착 가능 영역 크기

    [Header("Material")]
    public Material terrainMaterial; // 모든 청크가 공유할 버텍스 컬러 호환 머티리얼

    private float[,,] densities;                                    // 3차원 공간의 밀도(땅인지 공기인지)를 저장하는 지도
    private VoxelType[,,] voxelTypes;                               // 각 점의 VoxelType을 저장하는 배열 (Air, Dirt, Iron 등)
    private VoxelChunk[,,] chunks;                                  // 각 청크의 정보를 받아오기

    [SerializeField] private VoxelSurfaceGenerator surfaceGen;      // 지형 표면 생성기
    [SerializeField] private VoxelCaveGenerator caveGen;            // 동굴 생성기
    [SerializeField] private VoxelItemGenerator itemGen;            // 매장 아이템 생성기

    private int numChunksX, numChunksY, numChunksZ; // 청크 배열

    private Transform chunkContainer;


    protected override void Awake()
    {
        base.Awake();
        InitializeGenerators();
    }

    private async void Start()
    {
        await GenerateTerrain();
    }

    private void InitializeGenerators()
    {
        if (surfaceGen == null) surfaceGen = GetComponent<VoxelSurfaceGenerator>();
        if (caveGen == null) caveGen = GetComponent<VoxelCaveGenerator>();
        if (itemGen == null) itemGen = GetComponent<VoxelItemGenerator>();

        if (surfaceGen != null) surfaceGen.InitializeOffsets();
        if (caveGen != null) caveGen.InitializeOffsets();
    }

    // 지형을 생성하는 전체 과정을 하나로 묶은 함수
    public async Task GenerateTerrain()
    {
        // 빈 부모 컨테이너 생성 및 월드 하위에 정렬
        if (chunkContainer == null)
        {
            GameObject containerObj = new GameObject("ChunkContainer");
            containerObj.transform.SetParent(transform);
            containerObj.transform.localPosition = Vector3.zero;
            containerObj.transform.localRotation = Quaternion.identity;
            chunkContainer = containerObj.transform;
        }

        // 큐브의 '모서리'를 기준으로 계산하므로 배열 크기는 width + 1 입니다.
        densities = new float[width + 1, height + 1, depth + 1];
        voxelTypes = new VoxelType[width + 1, height + 1, depth + 1];    // 각 점의 VoxelType을 저장하는 배열

        if (surfaceGen != null) surfaceGen.GenerateSurface(densities, voxelTypes, width, height, depth, surfaceLevel);
        if (caveGen != null) caveGen.ApplyCaves(densities, voxelTypes, width, height, depth, surfaceGen);
        if (itemGen != null) itemGen.ApplyItemVoxels(densities, voxelTypes, width, height, depth, surfaceLevel, surfaceGen);

        // 청크 배열 계산 및 오브젝트 동적 생성
        numChunksX = Mathf.CeilToInt((float)width / VoxelChunk.ChunkSize);
        numChunksY = Mathf.CeilToInt((float)height / VoxelChunk.ChunkSize);
        numChunksZ = Mathf.CeilToInt((float)depth / VoxelChunk.ChunkSize);

        chunks = new VoxelChunk[numChunksX, numChunksY, numChunksZ];

        List<Task> updateTasks = new List<Task>();

        for (int x = 0; x < numChunksX; x++)
        {
            for (int y = 0; y < numChunksY; y++)
            {
                for (int z = 0; z < numChunksZ; z++)
                {
                    GameObject chunkObj = new GameObject($"Chunk_{x}_{y}_{z}");
                    chunkObj.transform.SetParent(chunkContainer);

                    // 부모(VoxelWorld)의 레이어를 청크 오브젝트에 그대로 적용
                    chunkObj.layer = gameObject.layer;

                    var renderer = chunkObj.AddComponent<MeshRenderer>();
                    renderer.sharedMaterial = terrainMaterial;

                    VoxelChunk chunk = chunkObj.AddComponent<VoxelChunk>();
                    chunk.Initialize(this, new Vector3Int(x, y, z));
                    chunks[x, y, z] = chunk;

                    // 병렬 생성 예약
updateTasks.Add(chunk.UpdateChunkAsync());                }
            }
        }

        // 모든 청크 초기 메쉬 생성 완료까지 대기
        await Task.WhenAll(updateTasks);

        // 지형 및 메쉬 생성 완료 후 동굴 유물 스폰 실행 (densities, surfaceLevel 전달)
        if (itemGen != null && caveGen != null)
        {
            itemGen.SpawnCaveArtifacts(densities, caveGen.GetChamberCenters(), width, height, depth, surfaceLevel);
        }

        PlayerSpawner spawner = FindAnyObjectByType<PlayerSpawner>();
        if (spawner != null) spawner.SpawnPlayer();
    }

    // 플레이어의 DigState에서 호출되는 함수
    public async void Dig(Vector3 worldPos, float radius, float digStrength = 1.0f)
    {
        int digCount = 0; // 디버깅용 카운터
        // 메쉬를 중앙으로 옮겼으므로, 인덱스를 찾을 때는 반대로 오프셋을 더해줘야 합니다.
        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);

        // 월드 좌표를 메쉬 내부의 로컬 배열 인덱스로 변환
        int centerX = Mathf.RoundToInt(worldPos.x - transform.position.x + offset.x);
        int centerY = Mathf.RoundToInt(worldPos.y - transform.position.y + offset.y);
        int centerZ = Mathf.RoundToInt(worldPos.z - transform.position.z + offset.z);
        int r = Mathf.CeilToInt(radius);

        // 최하단 암반층 높이 계산 (surfaceGen이 null이면 0으로 설정)
        int bedrockLimit = surfaceGen != null ? surfaceGen.bottomBedrockHeight : 0;
        Bounds digZone = new Bounds(transform.position + digZoneOffset, digZoneSize);

        // 수정이 필요한 청크 목록 (중복 방지)
        HashSet<VoxelChunk> dirtyChunks = new HashSet<VoxelChunk>();

        // 구형(Sphere) 형태로 밀도 맵 파내기
        for (int x = centerX - r; x <= centerX + r; x++)
        {
            for (int y = centerY - r; y <= centerY + r; y++)
            {
                for (int z = centerZ - r; z <= centerZ + r; z++)
                {
                    if (y <= bedrockLimit) continue; // 최하단 암반은 파내지 않음

                    // 배열 범위를 벗어나지 않도록 안전 검사
                    if (x >= 0 && x <= width && y >= 0 && y <= height && z >= 0 && z <= depth)
                    {
                        Vector3 voxelWorldPos = new Vector3(x, y, z) + transform.position - offset;
                        if (useDigBounds && !digZone.Contains(voxelWorldPos)) continue;// 굴착 제한 영역 밖이면 패스

                        // 중심점과의 거리를 계산하여 구 안에 있는지 확인
                        float dist = Vector3.Distance(new Vector3(x, y, z), new Vector3(centerX, centerY, centerZ));

                        if (dist <= radius)
                        {
                            float falloff = Mathf.SmoothStep(1f, 0f, dist / radius);
                            float removeAmount = digStrength * falloff;

                            // 아직 파괴되지 않은 땅(밀도 > -1.0)이었는지 확인
                            if (densities[x, y, z] > -1.0f)
                            {
                                float oldDensity = densities[x, y, z];
                                densities[x, y, z] = Mathf.Max(-1.0f, densities[x, y, z] - removeAmount); // 밀도 감소

                                if (oldDensity > surfaceLevel && densities[x, y, z] <= surfaceLevel)
                                {
                                    SpawnItemIfExist(voxelTypes[x, y, z], voxelWorldPos);
                                    voxelTypes[x, y, z] = VoxelType.Air;
                                }
                                // 실제로 밀도 변경이 일어났다면 플래그 설정
                                if (!Mathf.Approximately(oldDensity, densities[x, y, z]))
                                {
                                    AddDirtyChunks(x, y, z, dirtyChunks);
                                    digCount++;
                                }
                            }
                        }
                    }
                }
            }
        }

        // 변경된 1~4개 청크만 비동기 갱신
        List<Task> refreshTasks = new List<Task>();
        foreach (var chunk in dirtyChunks)
        {
            refreshTasks.Add(chunk.UpdateChunkAsync());
        }

        await Task.WhenAll(refreshTasks);
    }

    private void AddDirtyChunks(int x, int y, int z, HashSet<VoxelChunk> dirtyChunks)
    {
        int cx = x / VoxelChunk.ChunkSize;
        int cy = y / VoxelChunk.ChunkSize;
        int cz = z / VoxelChunk.ChunkSize;

        AddChunkIfValid(cx, cy, cz, dirtyChunks);

        // 경계면에 걸친 이웃 청크들 추가(양방향 완벽 처리)
        int modX = x % VoxelChunk.ChunkSize;
        int modY = y % VoxelChunk.ChunkSize;
        int modZ = z % VoxelChunk.ChunkSize;

        // 청크 경계면 복셀인 경우, 이웃 청크도 같이 메쉬 갱신 (경계면 끊김 방지)
        if (modX == 0 && cx > 0) AddChunkIfValid(cx - 1, cy, cz, dirtyChunks);
        if (modX == VoxelChunk.ChunkSize - 1 && cx < numChunksX - 1) AddChunkIfValid(cx + 1, cy, cz, dirtyChunks);

        if (modY == 0 && cy > 0) AddChunkIfValid(cx, cy - 1, cz, dirtyChunks);
        if (modY == VoxelChunk.ChunkSize - 1 && cy < numChunksY - 1) AddChunkIfValid(cx, cy + 1, cz, dirtyChunks);

        if (modZ == 0 && cz > 0) AddChunkIfValid(cx, cy, cz - 1, dirtyChunks);
        if (modZ == VoxelChunk.ChunkSize - 1 && cz < numChunksZ - 1) AddChunkIfValid(cx, cy, cz + 1, dirtyChunks);
    }

    private void AddChunkIfValid(int cx, int cy, int cz, HashSet<VoxelChunk> dirtyChunks)
    {
        if (cx >= 0 && cx < numChunksX && cy >= 0 && cy < numChunksY && cz >= 0 && cz < numChunksZ)
        {
            if (chunks[cx, cy, cz] != null)
                dirtyChunks.Add(chunks[cx, cy, cz]);
        }
    }

    private void SpawnItemIfExist(VoxelType voxelType, Vector3 spawnPosition)
    {
        if (voxelType == VoxelType.Dirt || voxelType == VoxelType.Air || itemGen == null) return;

        GameObject prefab = itemGen.GetFieldPrefab(voxelType);

        // 만약 해당 VoxelType에 대응하는 필드 아이템 프리팹이 존재하면, 그 위치에 스폰
        if (prefab != null)
        {
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }

    public Vector3 GetWorldOffset() => new Vector3(width / 2f, height / 2f, depth / 2f);
    public float GetDensity(int x, int y, int z)
    {
        if (densities == null) return -1f;
        return densities[Mathf.Clamp(x, 0, width), Mathf.Clamp(y, 0, height), Mathf.Clamp(z, 0, depth)];
    }
    public float GetSurfaceHeight(float x, float z) => surfaceGen != null ? surfaceGen.GetSurfaceHeight(x, z) : height;

    //// VoxelWorld.cs 클래스 내부에 추가
    //private void OnDrawGizmosSelected()
    //{
    //    if (useDigBounds)
    //    {
    //        Gizmos.color = Color.green;
    //        // 굴착 가능 영역을 녹색 와이어프레임 상자로 표시
    //        Gizmos.DrawWireCube(transform.position + digZoneOffset, digZoneSize);
    //    }
    //}
}

