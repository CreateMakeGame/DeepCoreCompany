using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


[ExecuteAlways] // 에디터에서도 실행되도록 설정
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Soil Layer Settings (토양 층위 설정)")]
    public Gradient soilGradient;           // 토양 색상을 편집할 그라데이션
    public float maxDepth = 30f;            // 토양 층위의 최대 깊이 (이 깊이까지는 토양 색상을 적용)

    [Header("Grid Settings")]
    public int width = 100;                 // X축 길이 (가로)
    public int height = 100;                // Y축 전체 높이 공간
    public int depth = 100;                 // Z축 길이 (세로)
    public float surfaceLevel = 0.5f;       // 땅과 공기를 구분하는 기준값
   
    [Header("Dig Boundary Settings")]
    public bool useDigBounds = true;     // 굴착 제한 적용 여부
    public Vector3 digZoneOffset = Vector3.zero; // 굴착 제한 영역의 오프셋
    public Vector3 digZoneSize = new Vector3(10f, 20f, 10f); // 굴착 가능 영역 크기

    private float[,,] densities;                            // 3차원 공간의 밀도(땅인지 공기인지)를 저장하는 지도
    private VoxelType[,,] voxelTypes;                       // 각 점의 VoxelType을 저장하는 배열 (Air, Dirt, Iron 등)

    private List<Vector3> vertices = new List<Vector3>();   // 만들어질 메쉬의 꼭짓점들
    private List<Color> colors = new List<Color>();         //  버텍스 색상 리스트
    private List<int> triangles = new List<int>();          // 꼭짓점을 이어붙일 삼각형의 순서

    private Dictionary<Vector3, int> vertexIndexMap = new Dictionary<Vector3, int>();   // 동일한 위치의 버텍스를 재사용하기 위한 맵
    
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    [SerializeField] private VoxelSurfaceGenerator surfaceGen;      // 지형 표면 생성기
    [SerializeField] private VoxelCaveGenerator caveGen;            // 동굴 생성기
    [SerializeField] private VoxelItemGenerator itemGen;            // 매장 아이템 생성기

    void OnEnable()
    {
        InitializeComponents();
        GenerateTerrain();      // 시작하자마자 한 번 지형을 생성합니다
    }

    // 유니티 에디터(Inspector)에서 width, height 등의 숫자를 바꿀 때마다 자동으로 실행되는 함수
    void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) GenerateTerrain();
        };
#endif
    }
    private void InitializeComponents()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if (mesh == null)
        {
            mesh = new Mesh { name = "Voxel Terrain Mesh", indexFormat = IndexFormat.UInt32 };
        }
        meshFilter.sharedMesh = mesh; // MeshFilter에 메쉬 할당

        // 서브 컴포넌트 초기화
        surfaceGen = GetComponent<VoxelSurfaceGenerator>();
        caveGen = GetComponent<VoxelCaveGenerator>();
        itemGen = GetComponent<VoxelItemGenerator>();
    }

    // 지형을 생성하는 전체 과정을 하나로 묶은 함수
    public void GenerateTerrain()
    {
        GenerateDensities(); // 공간의 밀도(노이즈) 결정
        MarchAllCubes();     // 부드러운 보간을 적용해 삼각형 생성
        UpdateMesh();        // 실제 메쉬와 충돌체에 적용

        // 지형 및 메쉬 생성 완료 후 동굴 유물 스폰 실행 (densities, surfaceLevel 전달)
        if (itemGen != null && caveGen != null)
        {
            itemGen.SpawnCaveArtifacts(densities, caveGen.GetChamberCenters(), width, height, depth, surfaceLevel);
        }
    }

    // 공간을 가상의 큐브 격자로 나누고, 각 점에 노이즈를 주어 흙(1)인지 공기(0)인지 결정합니다.
    private void GenerateDensities()
    {
        // 큐브의 '모서리'를 기준으로 계산하므로 배열 크기는 width + 1 입니다.
        densities = new float[width + 1, height + 1, depth + 1];
        voxelTypes = new VoxelType[width + 1, height + 1, depth + 1];    // 각 점의 VoxelType을 저장하는 배열
        
        // 표면 지형 생성(필수)
        if(surfaceGen != null)
        {
            surfaceGen.GenerateSurface(densities, voxelTypes, width, height, depth, surfaceLevel);
        }

        // 동굴 생성(선택)
        if (caveGen != null)
        {
            caveGen.ApplyCaves(densities, voxelTypes, width, height, depth, surfaceGen);
        }

        if(itemGen != null)
        {
            itemGen.ApplyItemVoxels(densities, voxelTypes, width, height, depth, surfaceLevel, surfaceGen);
        }

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

        // 최하단 암반층 높이 계산 (surfaceGen이 null이면 0으로 설정)
        int bedrockLimit = surfaceGen != null ? surfaceGen.bottomBedrockHeight : 0;
        Bounds digZone = new Bounds(transform.position + digZoneOffset, digZoneSize);

        // 구형(Sphere) 형태로 밀도 맵 파내기
        for (int x = centerX - r; x <= centerX + r; x++)
        {
            for (int y = centerY - r; y <= centerY + r; y++)
            {
                for (int z = centerZ - r; z <= centerZ + r; z++)
                {
                    if( y <= bedrockLimit) continue; // 최하단 암반은 파내지 않음

                    // 배열 범위를 벗어나지 않도록 안전 검사
                    if (x >= 0 && x <= width && y >= 0 && y <= height && z >= 0 && z <= depth)
                    {
                        Vector3 voxelWorldPos = new Vector3(x, y, z) + transform.position - offset;
                        if (useDigBounds && !digZone.Contains(voxelWorldPos))
                        {
                            continue; // 굴착 제한 영역 밖이면 패스
                        }
                        // 중심점과의 거리를 계산하여 구 안에 있는지 확인
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
        if (voxelType == VoxelType.Dirt || voxelType == VoxelType.Air || itemGen == null) return;

        GameObject prefab = itemGen.GetFieldPrefab(voxelType);

        // 만약 해당 VoxelType에 대응하는 필드 아이템 프리팹이 존재하면, 그 위치에 스폰
        if (prefab != null)
        {
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
        // [참고] 만약 생성된 필드 아이템에 ItemData 정보(가치, 무게 등)를 넘겨주는 스크립트(예: FieldItem)가 붙어있다면 
        // 아래처럼 넘겨줄 수 있습니다.
        /*
        if (spawnedObj.TryGetComponent<FieldItemHolder>(out var holder))
        {
            holder.itemData = spawnData.itemData;
        }
        */
    }

    // 밀도 배열을 전체적으로 훑으면서 어디에 면(삼각형)을 만들지 결정합니다.
    private void MarchAllCubes()
    {
        vertices.Clear();
        colors.Clear();
        triangles.Clear();
        vertexIndexMap.Clear();

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

            // 버텍스를 무작정 ADD 하지 않고, 기존에 있던 위치면 그 번호를 재사용함.
            AddSharedVertex(GetEdgeCenter(x, y, z, edge0));
            AddSharedVertex(GetEdgeCenter(x, y, z, edge1));
            AddSharedVertex(GetEdgeCenter(x, y, z, edge2));
        }
    }
    // 동일한 위치의 버텍스를 재사용하여 매끈한 스무스 셰이딩을 가능하게 만드는 함수
    private void AddSharedVertex(Vector3 position)
    {
        Vector3 roundePos = new Vector3(
            Mathf.Round(position.x * 1000f) / 1000f,
            Mathf.Round(position.y * 1000f) / 1000f,
            Mathf.Round(position.z * 1000f) / 1000f
        );

        if (vertexIndexMap.TryGetValue(roundePos, out int index))
        {
            triangles.Add(index);   // 이미 존재하는 버텍스 인텍스 재사용함
        }
        else
        {
            int newindex = vertices.Count;
            vertices.Add(roundePos);

            Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);
            // 버텍스 색상 계산 (토양 색상 그라데이션 적용)
            float gridX = roundePos.x + offset.x;
            float gridY = roundePos.y + offset.y;
            float gridZ = roundePos.z + offset.z;

            float surfaceY = surfaceGen != null ? surfaceGen.GetSurfaceHeight(gridX, gridZ) : height;
            float depthFromSurface = Mathf.Max(0f, surfaceY - gridY);
            float normalizedDepth = Mathf.Clamp01(depthFromSurface / maxDepth);

            // soilGradient이 null이 아닌 경우에만 Evaluate를 호출하고, null이면 기본 색상(Color.white)을 사용
            Color vertColor = (soilGradient != null) ? soilGradient.Evaluate(normalizedDepth) : Color.white;
            colors.Add(vertColor);

            vertexIndexMap.Add(roundePos, newindex);
            triangles.Add(newindex);
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

    // 두 꼭짓점 사이의 값을 선형 보간하여 실제 메쉬 상에서의 위치를 계산하는 함수
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
        mesh.indexFormat = IndexFormat.UInt32;  // 꼭짓점이 많을 경우를 대비해 32비트 인덱스 사용
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); // 조명 효과를 위한 노말 계산

        // 물리 충돌 업데이트 (캐릭터가 밟고 서기 위해 필수)
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null; // 초기화 후 다시 대입해야 즉시 갱신됨
            meshCollider.sharedMesh = mesh;
        }
    }
}