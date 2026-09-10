using Unity.Cinemachine;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private VoxelTerrain terrain;
    [SerializeField] private VoxelSurfaceGenerator surfaceGenerator;
    [SerializeField] private GameObject playerPrefab;

    private bool hasSpawned = false;

    private void Awake()
    {
        if (terrain == null) terrain = FindAnyObjectByType<VoxelTerrain>();
        if (surfaceGenerator == null) surfaceGenerator = FindAnyObjectByType<VoxelSurfaceGenerator>();
    }

    public void SpawnPlayer()
    {
        if (hasSpawned) return; // 이미 스폰된 경우 중복 스폰 방지
        if (terrain == null || surfaceGenerator == null || playerPrefab == null) return;

        hasSpawned = true;

        float spawnX = terrain.width / 2f;
        float spawnZ = terrain.depth / 2f;

        float surfaceY = surfaceGenerator.GetSurfaceHeight(spawnX, spawnZ);
        // 월드 좌표로 변환하기 전에 지형의 중심으로 잡습니다.
        Vector3 worldOffset = new Vector3(terrain.width / 2f, terrain.height / 2f, terrain.depth / 2f);
        // 플레이어가 땅에 묻히는 것을 방지하기 위해 Y 좌표를 1.5f만큼 올립니다.
        Vector3 spawnPosition = new Vector3(spawnX, surfaceY + 1.5f, spawnZ) + terrain.transform.position - worldOffset;

        GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        if (CameraManager.Instance != null)
        {
            var axisController = spawnedPlayer.GetComponentInChildren<CinemachineInputAxisController>(true);
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
