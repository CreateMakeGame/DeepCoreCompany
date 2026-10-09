using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private VoxelWorld world;
    [SerializeField] private VoxelSurfaceGenerator surfaceGenerator;
    [SerializeField] private SpawnObject spawnObject;
    [SerializeField] private GameObject playerPrefab;

    [Header("플레이어 스폰 설정")]
    [SerializeField] private float spawnRadius = 5f; // 플레이어 스폰 반경
    [SerializeField] private float spawnHeightOffset = 1.5f; // 플레이어 스폰 높이 오프셋
    [SerializeField] private int maxSpawnAttempts = 10; // 최대 스폰 시도 횟수

    private bool hasSpawned = false;

    private void Awake()
    {
        if (world == null) world = FindAnyObjectByType<VoxelWorld>();
        if (surfaceGenerator == null) surfaceGenerator = FindAnyObjectByType<VoxelSurfaceGenerator>();
        if (spawnObject == null) spawnObject = FindAnyObjectByType<SpawnObject>();
    }

    public void SpawnPlayer()
    {
        if (hasSpawned) return; // 이미 스폰된 경우 중복 스폰 방지
        if (world == null || surfaceGenerator == null || playerPrefab == null || spawnObject == null) return;

        Transform midObject = spawnObject.MidObjectTransform;

        if (midObject == null) return;

        Vector3 spawnPosition = Vector3.zero;
        bool foundPosition = false;

        // 중앙 오브젝트 주변에서 유효한 지면 위치 탐색
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomDirection =  Random.insideUnitCircle.normalized;

            float distance = Random.Range(spawnRadius * 0.6f, spawnRadius); // 최소 거리와 최대 거리 사이에서 랜덤 선택

            float candidateX = midObject.position.x + randomDirection.x * distance;
            float candidateZ = midObject.position.z + randomDirection.y * distance;

            if (spawnObject.TryGetTerrainPositionAt(candidateX, candidateZ, out Vector3 terrainPosition))
            {
                spawnPosition = terrainPosition + Vector3.up * spawnHeightOffset; // 높이 오프셋 적용
                foundPosition = true;
                break;
            }
        }

        if (!foundPosition) return; // 유효한 위치를 찾지 못한 경우 스폰하지 않음

        GameObject spawnPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        hasSpawned = true;

        if (CameraManager.Instance != null)
        {
            var axisController = spawnPlayer.GetComponentInChildren<CinemachineInputAxisController>(true);

            if (axisController != null)
            {
                // 생성된 오브젝트의 컨트롤러를 명시적으로 카메라 매니저에 등록
                CameraManager.Instance.RegisterPlayerCamera(axisController);
            }
            else
            {
                // 만약 전달받지 못한 경우 예비책으로 전체 탐색 실행
                CameraManager.Instance.BindPlayerCamera();
            }
        }
    }

    // 씬 전환/재시작 시 플래그 리셋이 필요한 경우를 위한 메서드
    public void ResetSpawner()
    {
        hasSpawned = false;
    }
}
