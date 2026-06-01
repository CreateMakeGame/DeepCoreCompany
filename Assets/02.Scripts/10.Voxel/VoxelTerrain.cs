using UnityEngine;
using System.Collections.Generic;

public enum VoxelType
{
    Air,
    Dirt,
    Iron,
    Artifact,
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

    [Header("Mineral Settings")]
    [SerializeField] private GameObject ironPrefab;     // 철 광물 프리팹
    [SerializeField][Range(0f, 1f)] private float ironChance = 0.05f; // 철광석이 매립될 확률 (5%)
    [SerializeField] private int ironMaxHeight = 5;


    private float[,,] densities;            // 3차원 공간의 밀도(땅인지 공기인지)를 저장하는 지도
    private VoxelType[,,] voxelTypes;        // 각 점의 VoxelType을 저장하는 배열 (Air, Dirt, Iron 등)
    private List<Vector3> vertices = new List<Vector3>();   // 만들어질 메쉬의 꼭짓점들
    private List<int> triangles = new List<int>();          // 꼭짓점을 이어붙일 삼각형의 순서

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    

    void OnEnable()
    {
        InitializeComponents();
        GenerateTerrain(); // 시작하자마자 한 번 지형을 생성합니다
    }

    // 유니티 에디터(Inspector)에서 width, height 등의 숫자를 바꿀 때마다 자동으로 실행되는 함수입니다.
    void OnValidate()
    {
        // 씬이 로딩 중일 때는 에러가 날 수 있으니 가볍게 무시해줍니다.
        if (gameObject.activeInHierarchy)
        {
            InitializeComponents();
            GenerateTerrain();
        }
    }

    private void InitializeComponents()
    {
        if(meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if(meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if(mesh == null)
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
        MarchAllCubes();     // 2. 밀도에 따라 삼각형 생성 (Marching Cubes 알고리즘)
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
                // PerlinNoise를 이용해 자연스러운 굴곡(언덕)을 만듭니다.
                float surfaceHeight = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * (height * 0.5f) + (height * 0.2f);

                for (int y = 0; y <= height; y++)
                {
                    if( y < surfaceHeight)
                    {
                        densities[x, y, z] = 1f;

                        // 땅으로 간주되는 점에 대해 VoxelType을 결정합니다.
                        if (y < ironMaxHeight && Random.value < ironChance)
                        {
                            voxelTypes[x, y, z] = VoxelType.Iron; // 철광석
                        }
                        else
                        {
                            voxelTypes[x, y, z] = VoxelType.Dirt; // 일반 흙
                        }
                    }
                    else
                    {
                        densities[x, y, z] = 0f;
                        voxelTypes[x, y, z] = VoxelType.Air;
                    }
                    
                }
            }
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
                        float dist = Vector3.Distance(new Vector3(x, y, z), new Vector3(centerX, centerY, centerZ));
                        if (dist <= radius)
                        {
                            // 아직 파괴되지 않은 땅(밀도 > surfaceLevel)이었는지 확인
                            if (densities[x, y, z] > surfaceLevel)
                            {
                                // 그 자리가 철광석 데이터였다면 프리팹 생성!
                                if (voxelTypes[x, y, z] == VoxelType.Iron && ironPrefab != null)
                                {
                                    // 인덱스 좌표를 다시 유니티 월드 좌표로 역변환
                                    Vector3 spawnPos = new Vector3(x, y, z) + transform.position - offset;
                                    Instantiate(ironPrefab, spawnPos, Quaternion.identity);
                                }

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

        Vector3 pos0 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v0] - offset;
        Vector3 pos1 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v1] - offset;

        return (pos0 + pos1) / 2f;
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
}