using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelChunk : MonoBehaviour
{
    public static int ChunkSize = 16;   // Define the size of the chunk

    private Vector3Int chunkCoord;      // 월드 기준 청크 인덱스
    private VoxelWorld world;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    public class MeshData
    {
        public readonly List<Vector3> vertices = new List<Vector3>(2000);   // 만들어질 메쉬의 꼭짓점들
        public readonly List<Color> colors = new List<Color>(2000);         //  버텍스 색상 리스트
        public readonly List<int> triangles = new List<int>(6000);
        public readonly Dictionary<Vector3Int, int> vertexIndexMap = new Dictionary<Vector3Int, int>(2000);   // 동일한 위치의 버텍스를 재사용하기 위한 맵
    }

    private bool isUpdating = false;

    public void Initialize(VoxelWorld worldManager, Vector3Int coord)
    {
        world = worldManager;

        chunkCoord = coord;

        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        mesh = new Mesh { name = $"Chunk_{coord.x}_{coord.y}_{coord.z}", indexFormat = IndexFormat.UInt32 };
        meshFilter.sharedMesh = mesh;

        Vector3 worldOffset = world.GetWorldOffset();
        transform.localPosition = new Vector3(
            coord.x * ChunkSize, 
            coord.y * ChunkSize, 
            coord.z * ChunkSize) - worldOffset;
    }

    public async Task UpdateChunkAsync()
    {
        if (isUpdating) return; // 이미 메쉬를 갱신 중이면 중복 호출 방지
        isUpdating = true;

        // CPU 백그라운드 스레드에서 복셀 삼각형 및 버텍스 데이터 계산
        MeshData meshData = await Task.Run(() => MarchChunkCubes());

        // 메인 스레드에서 메쉬 할당 및 물리 베이킹
        await UpdateMeshAndColliderAsync(meshData);

        isUpdating = false;
    }

    // 밀도 배열을 전체적으로 훑으면서 어디에 면(삼각형)을 만들지 결정합니다.
    private MeshData MarchChunkCubes()
    {
        MeshData meshData = new MeshData();

        int startX = chunkCoord.x * ChunkSize;
        int startY = chunkCoord.y * ChunkSize;
        int startZ = chunkCoord.z * ChunkSize;

        for (int x = startX; x < startX + ChunkSize; x++)
        {
            for (int y = startY; y < startY + ChunkSize; y++)
            {
                for (int z = startZ; z < startZ + ChunkSize; z++)
                {
                    MarchCube(x, y, z, meshData);
                }
            }
        }

        return meshData;
    }

    // 큐브 한 칸(8개 꼭짓점)을 검사해서 적절한 메쉬를 MarchingTables에서 꺼내옵니다.
    private void MarchCube(int x, int y, int z, MeshData meshData)
    {
        int cubeIndex = 0;

        // 큐브의 8개 모서리를 돌면서 그곳이 땅인지 공기인지 확인합니다.
        for (int i = 0; i < 8; i++)
        {
            Vector3 cornerPos = new Vector3(x, y, z) + MarchingTables.CornerOffsets[i];

            // 밀도가 임계값보다 크면 땅으로 간주
            if (world.GetDensity((int)cornerPos.x, (int)cornerPos.y, (int)cornerPos.z) > world.surfaceLevel)
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
            AddSharedVertex(GetEdgeCenter(x, y, z, edge0), meshData);
            AddSharedVertex(GetEdgeCenter(x, y, z, edge1), meshData);
            AddSharedVertex(GetEdgeCenter(x, y, z, edge2), meshData);
        }
    }

    // 동일한 위치의 버텍스를 재사용하여 매끈한 스무스 셰이딩을 가능하게 만드는 함수
    private void AddSharedVertex(Vector3 globalPos, MeshData meshData)
    {
        // 이상 좌표(NaN 좌표) 필터링
        if (float.IsNaN(globalPos.x) || float.IsNaN(globalPos.y) || float.IsNaN(globalPos.z))
            return;

        // 청크 로컬 공간으로 변환
        Vector3 localPos = globalPos - new Vector3(
            chunkCoord.x * ChunkSize, 
            chunkCoord.y * ChunkSize, 
            chunkCoord.z * ChunkSize);

        Vector3Int key = new Vector3Int(
            Mathf.RoundToInt(localPos.x * 1000f),
            Mathf.RoundToInt(localPos.y * 1000f),
            Mathf.RoundToInt(localPos.z * 1000f)
        );

        if (meshData.vertexIndexMap.TryGetValue(key, out int index))
        {
            meshData.triangles.Add(index);   // 이미 존재하는 버텍스 인텍스 재사용함
        }
        else
        {
            int newindex = meshData.vertices.Count;
            meshData.vertices.Add(localPos);

            // 버텍스 컬러 결정 (글로벌 좌표 깊이 기준)
            float surfaceY = world.GetSurfaceHeight(globalPos.x, globalPos.z);
            float depthFromSurface = Mathf.Max(0f, surfaceY - globalPos.y);
            float normalizedDepth = Mathf.Clamp01(depthFromSurface / world.maxDepth);

            // soilGradient이 null이 아닌 경우에만 Evaluate를 호출하고, null이면 기본 색상(Color.white)을 사용
            Color vertColor = (world.soilGradient != null) ? world.soilGradient.Evaluate(normalizedDepth) : Color.white;
            meshData.colors.Add(vertColor);

            meshData.vertexIndexMap.Add(key, newindex);
            meshData.triangles.Add(newindex);
        }
    }

    // 두 꼭짓점 사이의 중심 좌표를 구하는 함수
    private Vector3 GetEdgeCenter(int x, int y, int z, int edge)
    {
        int v0 = MarchingTables.EdgeConnection[edge, 0];
        int v1 = MarchingTables.EdgeConnection[edge, 1];

        Vector3 corner0 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v0];
        Vector3 corner1 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v1];

        float val0 = world.GetDensity((int)corner0.x, (int)corner0.y, (int)corner0.z);
        float val1 = world.GetDensity((int)corner1.x, (int)corner1.y, (int)corner1.z);

        return GetInterpolatedEdge(corner0, corner1, val0, val1, world.surfaceLevel);
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

    private async Task UpdateMeshAndColliderAsync(MeshData meshData)
    {
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;  // 꼭짓점이 많을 경우를 대비해 32비트 인덱스 사용

        mesh.SetVertices(meshData.vertices);
        mesh.SetColors(meshData.colors);
        mesh.SetTriangles(meshData.triangles, 0);
        mesh.RecalculateNormals(); // 조명 효과를 위한 노말 계산
        mesh.RecalculateBounds();  // 충돌체 계산을 위한 바운딩 박스 갱신

        // 물리 충돌 업데이트 (캐릭터가 밟고 서기 위해 필수)
        if (meshCollider != null)
        {
            // 정점이 존재하는 경우에만 물리 메쉬 베이킹 진행
            if (meshData.vertices.Count > 0)
            {
                // 메인 스레드에서 미리 GetEntityId() 취득 (스레드 안전)
                var entityId = mesh.GetEntityId();

                // 최신 API인 Physics.BakeMesh(EntityId, bool)을 백그라운드 스레드에서 수행
                await Task.Run(() => Physics.BakeMesh(entityId, false));

                meshCollider.sharedMesh = null; // 초기화 후 다시 대입해야 즉시 갱신됨
                meshCollider.sharedMesh = mesh;
            }
            else
            {
                meshCollider.sharedMesh = null;
            }
        }
    }
}