using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnItem
{
    public string itemName = "오브젝트 이름";
    public GameObject prefab;

    [Header("스폰 모드 설정")]
    [Tooltip("CHECK: 잔디/꽃밭처럼 뭉쳐서 스폰, UNCHECK: 나무처럼 낱개 스폰")]
    public bool isClustered = false;

    [Header("개수 설정")]
    [Tooltip("낱개 스폰 시: 총 생성 개수 / 군집 스폰 시: 생성할 '군집(무더기)'의 개수")]
    public int count = 5;

    [Header("군집(Cluster) 상세 옵션 (isClustered = true 일 때만)")]
    [Tooltip("한 무더기 안에 들어갈 오브젝트 개수")]
    public int itemsPerCluster = 10;
    [Tooltip("무더기의 퍼짐 반지름")]
    public float clusterRadius = 3f;
}

public class SpawnObject : MonoBehaviour
{
    [Header("복셀 시스템 참조")]
    [SerializeField] private VoxelWorld voxelWorld;
    [SerializeField] private VoxelSurfaceGenerator surfaceGenerator;
    [SerializeField] private float surfaceLevel = 0.5f;

    [Header("스폰 대상")]
    [SerializeField] private GameObject midSpawnObject;
    [SerializeField] private List<SpawnItem> spawnItems = new List<SpawnItem>();

    [Header("지형 및 스폰 설정")]
    //[SerializeField] private Vector3 mapSize = new Vector3(100f, 50f, 100f);
    [SerializeField] private float maxSlopeAngle = 35f;     // 스폰 가능한 최대 경사각
    [SerializeField] private float spawnYOffset = 0.1f;     // 땅속 매립 방지용 오프셋

    //[SerializeField] private LayerMask terrainLayer;        // 지형(Voxel) 레이어

    private Transform spawnRootContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    public void StartSpawning()
    {
        spawnRootContainer = GetOrCreateContainer("_SpawnedObjects_Root", transform);

        SpawnMidObject();
        SpawnAllItems();
    }

    // 맵 중앙(VoxelWorld의 월드 중앙)에 1개 배치
    private void SpawnMidObject()
    {
        if (midSpawnObject == null) return;

        Vector3 worldCenter = voxelWorld.transform.position;
        Vector3? spawnPos = GetTerrainPositionAt(worldCenter.x, worldCenter.z);

        if (spawnPos.HasValue)
        {
            Transform midContainer = GetOrCreateContainer("[Group] MidObject", spawnRootContainer);
            Instantiate(midSpawnObject, spawnPos.Value, Quaternion.identity, midContainer);
        }
    }

    private void SpawnAllItems()
    {
        if (spawnItems == null || spawnItems.Count == 0) return;

        foreach (var item in spawnItems)
        {
            if (item.prefab == null) continue;

            string containerName = string.IsNullOrEmpty(item.itemName) ? $"[Group] {item.prefab.name}" : $"[Group] {item.itemName}";
            Transform itemContainer = GetOrCreateContainer(containerName, spawnRootContainer);

            if (item.isClustered)
            {
                SpawnClusters(item, itemContainer);
            }
            else
            {
                SpawnSingleItems(item, itemContainer);
            }
        }

    }
    private void SpawnSingleItems(SpawnItem item, Transform parentContainer)
    {
        int spawnedCount = 0;
        int maxAttempts = item.count * 5;
        int attempts = 0;

        while (spawnedCount < item.count && attempts < maxAttempts)
        {
            attempts++;
            Vector3? spawnPos = GetRandomTerrainPosition();
            if (spawnPos.HasValue)
            {
                CreateObject(item.prefab, spawnPos.Value, parentContainer);
                spawnedCount++;
            }
        }
    }
    private void SpawnClusters(SpawnItem item, Transform parentContainer)
    {
        for (int i = 0; i < item.count; i++)
        {
            // 군집 중심 위치 결정
            Vector3? clusterCenter = GetRandomTerrainPosition();
            if (!clusterCenter.HasValue) continue;

            // 중심점 주변 반지름 내에 자식 오브젝트 배치
            for (int j = 0; j < item.itemsPerCluster; j++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * item.clusterRadius;
                float targetWorldX = clusterCenter.Value.x + randomCircle.x;
                float targetWorldZ = clusterCenter.Value.z + randomCircle.y;

                // 월드 좌표 직접 감지
                Vector3? childSpawnPos = GetTerrainPositionAt(targetWorldX, targetWorldZ);

                if (childSpawnPos.HasValue)
                {
                    CreateObject(item.prefab, childSpawnPos.Value, parentContainer);
                }
            }
        }
    }
    // 맵 전체 범위 (-Half ~ +Half)에서 임의의 지표면 좌표 탐색
    private Vector3? GetRandomTerrainPosition()
    {
        Vector3 worldCenter = voxelWorld.transform.position;
        float halfWidth = voxelWorld.width * 0.5f;
        float halfDepth = voxelWorld.depth * 0.5f;

        float randomX = Random.Range(-halfWidth, halfWidth) + worldCenter.x;
        float randomZ = Random.Range(-halfWidth, halfWidth) + worldCenter.z;

        return GetTerrainPositionAt(randomX, randomZ);
    }
    // 특정 월드 X, Z 좌표 상공에서 지표면 높이 탐색
    private Vector3? GetTerrainPositionAt(float worldX, float worldZ)
    {
        if (surfaceGenerator == null || voxelWorld == null) return null;

        Vector3 worldCenter = voxelWorld.transform.position;
        Vector3 worldOffset = voxelWorld.GetWorldOffset(); // (width / 2, height / 2, depth / 2)

        // 월드 좌표를 VoxelWorld 내부 그리드 인덱스(0 ~ width/depth)로 환산
        float gridX = (worldX - worldCenter.x) + worldOffset.x;
        float gridZ = (worldZ - worldCenter.z) + worldOffset.z;

        // 복셀 배열 바깥 영역 예외 처리
        if (gridX < 0 || gridX > voxelWorld.width || gridZ < 0 || gridZ > voxelWorld.depth)
            return null;

        // 1. 표면 높이 계산 (로컬 그리드 Y)
        float localHeightY = surfaceGenerator.GetSurfaceHeight(gridX, gridZ);

        // 2. 월드 Y 좌표로 변환
        float worldSurfaceY = (localHeightY - worldOffset.y) + worldCenter.y + surfaceLevel;

        // 3. 경사각(Slope Angle) 올바른 수학적 산출
        float delta = 0.1f;
        float hL = surfaceGenerator.GetSurfaceHeight(gridX - delta, gridZ);
        float hR = surfaceGenerator.GetSurfaceHeight(gridX + delta, gridZ);
        float hD = surfaceGenerator.GetSurfaceHeight(gridX, gridZ - delta);
        float hU = surfaceGenerator.GetSurfaceHeight(gridX, gridZ + delta);

        // 높이 차이 변화율 기반 Normal 계산 수식 교정
        Vector3 normal = new Vector3(hL - hR, 2f * delta, hD - hU).normalized;
        float slopeAngle = Vector3.Angle(normal, Vector3.up);

        if (slopeAngle > maxSlopeAngle)
        {
            return null; // 가파른 경사면 스폰 제외
        }

        return new Vector3(worldX, worldSurfaceY + spawnYOffset, worldZ);
    }

    // 프리팹 생성 함수
    private void CreateObject(GameObject prefab, Vector3 position, Transform parentTarget)
    {
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        GameObject spawnedObj = Instantiate(prefab, position, randomRotation, parentTarget);
        spawnedObj.transform.localScale *= Random.Range(0.8f, 1.2f);
    }

    private Transform GetOrCreateContainer(string containerName, Transform parent)
    {
        Transform container = parent.Find(containerName);
        if (container == null)
        {
            GameObject go = new GameObject(containerName);
            container = go.transform;
            container.SetParent(parent);
            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
        }
        return container;
    }
}