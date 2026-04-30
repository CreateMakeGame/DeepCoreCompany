using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 20;
    public int height = 20;
    public int depth = 20;
    public float surfaceLevel = 0.5f;

    private float[,,] densities; // 통합된 밀도 맵
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Mesh mesh;
    private MeshCollider meshCollider;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshCollider = GetComponent<MeshCollider>();

        GenerateDensities(); // 1. 초기 지형 밀도 설정
        MarchAllCubes();     // 2. 큐브 계산
        UpdateMesh();        // 3. 메쉬 & 콜라이더 적용
    }

    private void GenerateDensities()
    {
        // 큐브의 '모서리'를 기준으로 계산하므로 배열 크기는 width + 1 입니다.
        densities = new float[width + 1, height + 1, depth + 1];

        for (int x = 0; x <= width; x++)
        {
            for (int z = 0; z <= depth; z++)
            {
                // 부드러운 언덕 모양을 위해 노이즈 적용
                float surfaceHeight = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * (height * 0.5f) + (height * 0.2f);

                for (int y = 0; y <= height; y++)
                {
                    // 지표면 높이보다 낮으면 땅(1), 높으면 공기(0)
                    densities[x, y, z] = (y < surfaceHeight) ? 1f : 0f;
                }
            }
        }
    }

    // 플레이어의 DigState에서 호출되는 함수
    public void Dig(Vector3 worldPos, float radius)
    {
        // 월드 좌표를 메쉬 내부의 로컬 배열 인덱스로 변환
        int centerX = Mathf.RoundToInt(worldPos.x - transform.position.x);
        int centerY = Mathf.RoundToInt(worldPos.y - transform.position.y);
        int centerZ = Mathf.RoundToInt(worldPos.z - transform.position.z);

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
                            densities[x, y, z] = 0f; // 파낸 곳을 공기(0)로 만듦
                        }
                    }
                }
            }
        }

        // 밀도가 변경되었으니 메쉬를 다시 계산하고 업데이트
        MarchAllCubes();
        UpdateMesh();
    }

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

    private void MarchCube(int x, int y, int z)
    {
        int cubeIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            Vector3 cornerPos = new Vector3(x, y, z) + MarchingTables.CornerOffsets[i];

            // 밀도가 임계값보다 크면 땅으로 간주
            if (densities[(int)cornerPos.x, (int)cornerPos.y, (int)cornerPos.z] > surfaceLevel)
            {
                cubeIndex |= 1 << i;
            }
        }

        for (int i = 0; MarchingTables.TriangleConnectionTable[cubeIndex, i] != -1; i += 3)
        {
            int edge0 = MarchingTables.TriangleConnectionTable[cubeIndex, i];
            int edge1 = MarchingTables.TriangleConnectionTable[cubeIndex, i + 1];
            int edge2 = MarchingTables.TriangleConnectionTable[cubeIndex, i + 2];

            vertices.Add(GetEdgeCenter(x, y, z, edge0));
            vertices.Add(GetEdgeCenter(x, y, z, edge1));
            vertices.Add(GetEdgeCenter(x, y, z, edge2));

            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
        }
    }

    private Vector3 GetEdgeCenter(int x, int y, int z, int edge)
    {
        int v0 = MarchingTables.EdgeConnection[edge, 0];
        int v1 = MarchingTables.EdgeConnection[edge, 1];

        // Vector2로 되어 있던 오타 수정 -> Vector3
        Vector3 pos0 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v0];
        Vector3 pos1 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v1];

        return (pos0 + pos1) / 2f;
    }

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
}