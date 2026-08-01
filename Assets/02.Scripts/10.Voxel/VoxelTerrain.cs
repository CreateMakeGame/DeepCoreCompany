using UnityEngine;
using System.Collections.Generic;

public enum VoxelType
{
    Air,
    Dirt,
    Iron,
    Gold,
    Artifact,
    Sample,
    Hazardous,
}

[System.Serializable]
public struct ItemSpawnData
{
    public ItemData itemData;           // 생성할 ScriptableObject 아이템 데이터
    public VoxelType voxelType;         // 해당 아이템이 매립될 복셀 타입
    [Range(0f, 1f)]
    public float spawnChance;           // 스폰 확률 (0 ~ 1)
    public int minHeight;               // 생성 최소 높이
    public int maxHeight;               // 생성 최대 높이
}

[ExecuteAlways] // 에디터에서도 실행되도록 설정
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;              // X축 길이 (가로)
    public int height = 20;             // Y축 길이 (높이)
    public int depth = 20;              // Z축 길이 (세로)
    public float surfaceLevel = 0.5f;   // 땅과 공기를 구분하는 기준값 (0.5보다 크면 땅)

    [Header("Cave Settings (동굴 설정)")]
    public bool enableCaves = true;      // 동굴 생성 여부
    [Range(0.01f, 0.2f)]
    public float caveScale = 0.08f;      // 동굴 터널 크기/주기
    [Range(0f, 1f)]
    public float caveThreshold = 0.52f;  // 동굴 빈 공간 비율
    public int caveMaxHeight = 15;       // 동굴이 생성될 최대 높이

    [Header("Dig Boundary Settings")]
    public bool useDigBounds = true;    // 굴착 제한 적용 여부
    public Vector3 digZoneOffset = Vector3.zero; // 굴착 제한 영역의 오프셋
    public Vector3 digZoneSize = new Vector3(10f, 20f, 10f); // 굴착 가능 영역 크기 (X: 가로, Y: 깊이, Z: 세로)


    [Header("Item Spawn Settings")]
    [SerializeField] private List<ItemSpawnData> itemSpawnList = new List<ItemSpawnData>();

    private float[,,] densities;                            // 3차원 공간의 밀도(땅인지 공기인지)를 저장하는 지도
    private VoxelType[,,] voxelTypes;                       // 각 점의 VoxelType을 저장하는 배열 (Air, Dirt, Iron 등)
    private List<Vector3> vertices = new List<Vector3>();   // 만들어질 메쉬의 꼭짓점들
    private List<int> triangles = new List<int>();          // 꼭짓점을 이어붙일 삼각형의 순서

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    void OnEnable()
    {
        InitializeComponents();
        GenerateTerrain();      // 시작하자마자 한 번 지형을 생성합니다
    }

    // 유니티 에디터(Inspector)에서 width, height 등의 숫자를 바꿀 때마다 자동으로 실행되는 함수
    void OnValidate()
    {
        // 씬이 로딩 중일 때는 에러가 날 수 있으니 가볍게 무시
        if (gameObject.activeInHierarchy)
        {
            InitializeComponents();
            GenerateTerrain();
        }
    }

    private void InitializeComponents()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Voxel Terrain Mesh";
        }
        meshFilter.sharedMesh = mesh; // MeshFilter에 메쉬 할당
    }

    // 지형을 생성하는 전체 과정을 하나로 묶은 함수
    private void GenerateTerrain()
    {
        GenerateDensities(); // 1. 공간의 밀도(노이즈) 결정
        MarchAllCubes();     // 2. 부드러운 보간을 적용해 삼각형 생성
        UpdateMesh();        // 3. 실제 메쉬와 충돌체에 적용
    }

    // 공간을 가상의 큐브 격자로 나누고, 각 점에 노이즈를 주어 흙(1)인지 공기(0)인지 결정합니다.
    private void GenerateDensities()
    {
        // 큐브의 '모서리'를 기준으로 계산하므로 배열 크기는 width + 1 입니다.
        densities = new float[width + 1, height + 1, depth + 1];
        voxelTypes = new VoxelType[width + 1, height + 1, depth + 1];    // 각 점의 VoxelType을 저장하는 배열

        for (int x = 0; x <= width; x++)
        {
            for (int z = 0; z <= depth; z++)
            {
                // PerlinNoise로 표면 높이 계산
                float surfaceHeight = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * (height * 0.5f) + (height * 0.2f);

                for (int y = 0; y <= height; y++)
                {
                    float density = surfaceHeight - y + surfaceLevel;
                    bool isCave = false;

                    // 3D Noise 기반 동굴 처리
                    if (enableCaves && y < caveMaxHeight && y < surfaceHeight - 2)
                    {
                        float caveNoise = Get3DNoise(x, y, z, caveScale);
                        if (caveNoise > caveThreshold)
                        {
                            isCave = true;
                        }
                    }

                    if (isCave)
                    {
                        densities[x, y, z] = 0f;
                        voxelTypes[x, y, z] = VoxelType.Air;
                    }
                    else
                    {
                        densities[x, y, z] = Mathf.Clamp(density, -1f, 2f);

                        if (densities[x, y, z] > surfaceLevel)
                        {
                            // ItemSpawnData 리스트 기반으로 스폰될 VoxelType 결정
                            voxelTypes[x, y, z] = DetermineVoxelType(y, surfaceHeight);
                        }
                        else
                        {
                            voxelTypes[x, y, z] = VoxelType.Air;
                        }
                    }
                }
            }
        }
    }
    // 높이에 맞는 ItemData 기반의 VoxelType 결정
    private VoxelType DetermineVoxelType(int y, float surfaceHeight)
    {
        foreach (var spawnData in itemSpawnList)
        {
            if (spawnData.itemData == null) continue;

            // 표면(surfaceHeight) 바로 아래 ~ 5칸 아래 사이에 매립되도록 설정
            float depthFromSurface = surfaceHeight - y;

            if (depthFromSurface >= spawnData.minHeight && depthFromSurface <= spawnData.maxHeight)
            {
                if (Random.value < spawnData.spawnChance)
                {
                    return spawnData.voxelType;
                }
            }
        }
        return VoxelType.Dirt; // 기본적으로 흙으로 설정
    }

    private float Get3DNoise(float x, float y, float z, float scale)
    {
        float xy = Mathf.PerlinNoise(x * scale, y * scale);
        float yz = Mathf.PerlinNoise(y * scale, z * scale);
        float zx = Mathf.PerlinNoise(z * scale, x * scale);

        float yx = Mathf.PerlinNoise(y * scale, x * scale);
        float zy = Mathf.PerlinNoise(z * scale, y * scale);
        float xz = Mathf.PerlinNoise(x * scale, z * scale);

        return (xy + yz + zx + yx + zy + xz) / 6f;
    }

    // 플레이어의 DigState에서 호출되는 함수
    public void Dig(Vector3 worldPos, float radius)
    {
        // 메쉬를 중앙으로 옮겼으므로, 인덱스를 찾을 때는 반대로 오프셋을 더해줘야 합니다.
        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);

        // 월드 좌표를 메쉬 내부의 로컬 배열 인덱스로 변환
        int centerX = Mathf.RoundToInt(worldPos.x - transform.position.x + offset.x);
        int centerY = Mathf.RoundToInt(worldPos.y - transform.position.y + offset.y);
        int centerZ = Mathf.RoundToInt(worldPos.z - transform.position.z + offset.z);
        int r = Mathf.CeilToInt(radius);

        Bounds digZone = new Bounds(transform.position + digZoneOffset, digZoneSize);

        // 구형(Sphere) 형태로 밀도 맵 파내기
        for (int x = centerX - r; x <= centerX + r; x++)
        {
            for (int y = centerY - r; y <= centerY + r; y++)
            {
                for (int z = centerZ - r; z <= centerZ + r; z++)
                {
                    // 배열 범위를 벗어나지 않도록 안전 검사
                    if (x >= 0 && x <= width && y >= 0 && y <= height && z >= 0 && z <= depth)
                    {
                        Vector3 voxelWorldPos = new Vector3(x, y, z) + transform.position - offset;
                        if (useDigBounds && !digZone.Contains(voxelWorldPos))
                        {
                            continue; // 굴착 제한 영역 밖이면 패스
                        }

                        float dist = Vector3.Distance(new Vector3(x, y, z), new Vector3(centerX, centerY, centerZ));
                        if (dist <= radius)
                        {
                            // 아직 파괴되지 않은 땅(밀도 > surfaceLevel)이었는지 확인
                            if (densities[x, y, z] > surfaceLevel)
                            {
                                // 땅을 파냈을 때 연결된 ItemData의 fieldPrefab 스폰
                                SpawnItemIfExist(voxelTypes[x, y, z], new Vector3(x, y, z) + transform.position - offset);
                                // 파내졌으므로 물질 상태를 공기(Air)로 변경
                                voxelTypes[x, y, z] = VoxelType.Air;
                            }
                            densities[x, y, z] = 0f;
                        }
                    }
                }
            }
        }
        // 밀도가 변경되었으니 메쉬를 다시 계산하고 업데이트
        MarchAllCubes();
        UpdateMesh();
    }

    private void SpawnItemIfExist(VoxelType voxelType, Vector3 spawnPosition)
    {
        if (voxelType == VoxelType.Dirt || voxelType == VoxelType.Air) return;
        ItemSpawnData spawnData = itemSpawnList.Find(s => s.voxelType == voxelType);

        // ItemData와 그 안의 fieldPrefab이 할당되어 있는지 확인 후 스폰
        if (spawnData.itemData != null && spawnData.itemData.fieldPrefab != null)
        {
            GameObject spawnedObj = Instantiate(spawnData.itemData.fieldPrefab, spawnPosition, Quaternion.identity);

            // [참고] 만약 생성된 필드 아이템에 ItemData 정보(가치, 무게 등)를 넘겨주는 스크립트(예: FieldItem)가 붙어있다면 
            // 아래처럼 넘겨줄 수 있습니다.
            /*
            if (spawnedObj.TryGetComponent<FieldItemHolder>(out var holder))
            {
                holder.itemData = spawnData.itemData;
            }
            */
        }
    }

    // 밀도 배열을 전체적으로 훑으면서 어디에 면(삼각형)을 만들지 결정합니다.
    private void MarchAllCubes()
    {
        vertices.Clear();
        triangles.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    MarchCube(x, y, z);
                }
            }
        }
    }

    // 큐브 한 칸(8개 꼭짓점)을 검사해서 적절한 메쉬를 MarchingTables에서 꺼내옵니다.
    private void MarchCube(int x, int y, int z)
    {
        int cubeIndex = 0;

        // 큐브의 8개 모서리를 돌면서 그곳이 땅인지 공기인지 확인합니다.
        for (int i = 0; i < 8; i++)
        {
            Vector3 cornerPos = new Vector3(x, y, z) + MarchingTables.CornerOffsets[i];

            // 밀도가 임계값보다 크면 땅으로 간주
            if (densities[(int)cornerPos.x, (int)cornerPos.y, (int)cornerPos.z] > surfaceLevel)
            {
                cubeIndex |= 1 << i;
            }
        }

        // 정답지(TriangleConnectionTable)를 보고 삼각형들을 생성합니다.
        for (int i = 0; MarchingTables.TriangleConnectionTable[cubeIndex, i] != -1; i += 3)
        {
            int edge0 = MarchingTables.TriangleConnectionTable[cubeIndex, i];
            int edge1 = MarchingTables.TriangleConnectionTable[cubeIndex, i + 1];
            int edge2 = MarchingTables.TriangleConnectionTable[cubeIndex, i + 2];

            // 만들어진 꼭짓점들을 리스트에 추가합니다.
            vertices.Add(GetEdgeCenter(x, y, z, edge0));
            vertices.Add(GetEdgeCenter(x, y, z, edge1));
            vertices.Add(GetEdgeCenter(x, y, z, edge2));

            // 어떤 순서로 삼각형을 그릴지 인덱스를 지정합니다.
            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
        }
    }

    // 두 꼭짓점 사이의 중심 좌표를 구하는 함수
    private Vector3 GetEdgeCenter(int x, int y, int z, int edge)
    {
        int v0 = MarchingTables.EdgeConnection[edge, 0];
        int v1 = MarchingTables.EdgeConnection[edge, 1];

        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f); // 월드 중심을 기준으로 좌표를 조정하기 위한 오프셋

        Vector3 corner0 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v0];
        Vector3 corner1 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v1];

        float val0 = densities[(int)corner0.x, (int)corner0.y, (int)corner0.z];
        float val1 = densities[(int)corner1.x, (int)corner1.y, (int)corner1.z];

        Vector3 pos0 = corner0 - offset;
        Vector3 pos1 = corner1 - offset;

        return GetInterpolatedEdge(pos0, pos1, val0, val1, surfaceLevel);
    }


    private Vector3 GetInterpolatedEdge(Vector3 p1, Vector3 p2, float val1, float val2, float surfaceLevel)
    {
        // 0으로 나누어지는 오차 방지 및 예외 처리
        if (Mathf.Abs(surfaceLevel - val1) < 0.00001f) return p1;
        if (Mathf.Abs(surfaceLevel - val2) < 0.00001f) return p2;
        if (Mathf.Abs(val1 - val2) < 0.00001f) return p1;

        // 선형 보간 함수 작성
        float t = (surfaceLevel - val1) / (val2 - val1);

        return Vector3.Lerp(p1, p2, t);
    }


    // 계산된 꼭짓점과 삼각형 데이터를 실제 Unity Mesh에 밀어 넣습니다.
    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); // 조명 효과를 위한 노말 계산

        // 물리 충돌 업데이트 (캐릭터가 밟고 서기 위해 필수)
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null; // 초기화 후 다시 대입해야 즉시 갱신됨
            meshCollider.sharedMesh = mesh;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 굴착 가능 제한 영역을 에디터 상에 표시 (녹색 박스)
        if (useDigBounds)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f); // 투명한 녹색
            Gizmos.DrawCube(transform.position + digZoneOffset, digZoneSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + digZoneOffset, digZoneSize);
        }

        // 게임이 실행 중이지 않거나 배열이 생성되지 않았다면 패스
        if (voxelTypes == null) return;

        // 기즈모 색상을 잘 보이는 색(예: 빨간색)으로 설정
        Gizmos.color = Color.red;

        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);

        // 전체 격자를 돌면서 철광석이 있는 위치를 찾아냅니다.
        for (int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = 0; z <= depth; z++)
                {
                    // 해당 좌표의 데이터가 Iron이라면!
                    if (voxelTypes[x, y, z] == VoxelType.Iron)
                    {
                        // 인덱스 좌표를 유니티 월드 좌표로 변환
                        Vector3 worldPos = new Vector3(x, y, z) + transform.position - offset;

                        // 그 위치에 0.3 크기의 선으로 된 큐브(상자)를 그립니다.
                        Gizmos.DrawWireCube(worldPos, Vector3.one * 0.3f);
                    }
                }
            }
        }
    }
    private Color GetGizmoColor(VoxelType type)
    {
        return type switch
        {
            VoxelType.Iron => Color.gray,
            VoxelType.Gold => Color.yellow,
            VoxelType.Artifact => Color.cyan,
            VoxelType.Sample => Color.magenta,
            VoxelType.Hazardous => Color.red,
            _ => Color.white,
        };
    }
}