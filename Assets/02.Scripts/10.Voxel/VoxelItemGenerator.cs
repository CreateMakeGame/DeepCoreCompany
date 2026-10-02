using UnityEngine;
using System.Collections.Generic;


public class VoxelItemGenerator : MonoBehaviour
{
    [Header("Item Spawn Settings")]
    [SerializeField] private List<ItemSpawnData> itemSpawnList = new List<ItemSpawnData>();

    [Header("Cave Artifact Settings")]
    [SerializeField] private List<ItemDataSO> caveArtifactDataList = new List<ItemDataSO>();

    [Header("Artifact Burial Depth Settings")]
    [Tooltip("땅속에 파묻히는 최소 높이 오프셋 (0.5 = 절반 매몰)")]
    [Range(0.3f, 1.0f)]
    [SerializeField] private float minArtifactHeightOffset = 0.5f;

    [Tooltip("땅속에 파묻히는 최대 높이 오프셋 (0.8 = 살짝 매몰)")]
    [Range(0.3f, 1.0f)]
    [SerializeField] private float maxArtifactHeightOffset = 0.85f;

    [Header("Spawn Ratio Settings")]
    [Tooltip("할당량 대비 전체 생성 가치 비율 (1.75 = 175%)")]
    [SerializeField] private float totalQuotaMultiplier = 1.75f;
    [Tooltip("동굴 유물 가치 비율 (0.2 = 20%)")]
    [SerializeField] private float caveArtifactRatio = 0.2f;

    [SerializeField] private int generatedTotalValue = 0;

    private List<Vector3> debugSpawnPositions = new List<Vector3>();

    /// <summary>
    /// 동굴 방(Chamber) 중심점들을 기반으로 동굴 바닥을 탐색하여 아이템 프리팹을 배치
    /// </summary>
    public void SpawnCaveArtifacts(float[,,] densities, List<Vector3Int> chamberCenters,
        int width, int height, int depth, float surfaceLevel)
    {
        debugSpawnPositions.Clear();
        generatedTotalValue = 0;    // 전체 아이템 가치 초기화

        if (caveArtifactDataList == null || caveArtifactDataList.Count == 0 ||
        chamberCenters == null || chamberCenters.Count == 0)
        {
            Debug.LogWarning("[ItemGenerator] 동굴 유물 데이터 또는 동굴 방(Chamber) 데이터가 없습니다.");
            return;
        }

        // 디버그용 카운터
        int skippedOutOfRange = 0;
        int skippedNoFloor = 0;

        // 생성된 유물 정리용 컨테이너
        Transform artifactContainer = GetOrCreateContainer("[Group] Cave Artifacts", transform);
        ClearContainerChildren(artifactContainer);

        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);
        List<Vector3> validSpawnPositions = new List<Vector3>();

        foreach (var chamber in chamberCenters)
        {
            // 범위 검사
            if (chamber.x < 0 || chamber.x >= width ||
                chamber.y < 0 || chamber.y >= height ||
                chamber.z < 0 || chamber.z >= depth)
            {
                skippedOutOfRange++;
                continue;
            }

            // (centerDensity 검사를 제거하여 공기/땅 밀도 판정 불일치 문제를 해결)
            int floorY = -1;
            bool inAirSpace = false;

            float startDensity = densities[chamber.x, chamber.y, chamber.z];
            for (int y = chamber.y; y >= 0; y--)
            {
                float currentDensity = densities[chamber.x, y, chamber.z];
                // 1. 현재 지점이 공기인지 확인 (density < surfaceLevel)
                if (currentDensity < surfaceLevel)
                {
                    inAirSpace = true; // 공기층 진입 확인
                }
                // 2. 공기층을 지난 후 처음 만나는 땅 (density >= surfaceLevel)
                else if (inAirSpace)
                {
                    floorY = y; // 여기가 진짜 동굴 바닥(땅)
                    break;
                }
            }

            // 바닥을 발견했으면 해당 좌표 등록 (floorY + 1 이 height 미만인지)
            if (floorY != -1 && (floorY + 1) < height)
            {
                float heightOffset = Random.Range(minArtifactHeightOffset, maxArtifactHeightOffset);

                // floorY는 땅, floorY + heightOffset은 땅 표면 위에 살짝 매몰된 위치
                Vector3 localPos = new Vector3(chamber.x, floorY + heightOffset, chamber.z) - offset;
                Vector3 worldSpawnPos = transform.TransformPoint(localPos);

                // [디버그 1] 콘솔 로그로 탐색 정보 확인
                Debug.Log($"[Artifact Debug] Chamber: {chamber} | Start Density: {startDensity} | Found FloorY: {floorY} | Spawn WorldPos: {worldSpawnPos}");

                // [디버그 2] 에디터 씬 뷰에서 시각적으로 확인할 디버그 레이 선 그리기 (30초간 유지)
                Debug.DrawLine(worldSpawnPos, worldSpawnPos + Vector3.up * 1.5f, Color.green, 10f);

                validSpawnPositions.Add(worldSpawnPos);
                debugSpawnPositions.Add(worldSpawnPos); // 디버그용 좌표 저장
            }
            else
            {
                skippedNoFloor++;
            }
        }

        // 좌표 탐색 실패 시 출력
        if (validSpawnPositions.Count == 0)
        {
            Debug.LogWarning($"[ItemGenerator] 동굴 바닥 스폰 좌표 찾기 실패! " +
                $"(전체 방: {chamberCenters.Count}개 | 범위이탈: {skippedOutOfRange} | 바닥탐색실패: {skippedNoFloor})");
            return;
        }

        // 모든 동굴 위치 후보를 무작위로 섞음 (특정 높이에 치우치는 문제 방지)
        ShuffleList(validSpawnPositions);

        // 목표 가치 계산: 전체 할당량 * 1.75 중 20%
        int currentQuota = (GameManager.Instance != null) ? GameManager.Instance.currentQuota : 0;
        int totalTargetQuota = Mathf.RoundToInt(currentQuota * totalQuotaMultiplier);
        int caveTargetValue = Mathf.RoundToInt(totalTargetQuota * caveArtifactRatio);

        int spawnedCount = 0;
        int caveTotalValue = 0;

        foreach (var spawnPos in validSpawnPositions)
        {
            // 가치를 달성하고 최소 1개 이상 생성했다면 루프 종료 (1개만 스폰되는 버그 수정)
            if (caveTotalValue >= caveTargetValue && spawnedCount > 0)
            {
                break;
            }

            ItemDataSO selectedArtifact = caveArtifactDataList[Random.Range(0, caveArtifactDataList.Count)];
            if (selectedArtifact == null) continue;

            // 스폰 시 사용할 프리팹 가져오기
            GameObject prefabToSpawn = selectedArtifact.GetSpawnPrefab();
            if (prefabToSpawn == null) continue;
            
            Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject spawnObject = Instantiate(prefabToSpawn, spawnPos, randomRot, artifactContainer);

            ItemObject itemObj = spawnObject.GetComponent<ItemObject>();
            if (itemObj != null)
            {
                itemObj.Initialize(selectedArtifact, true);
            }

            caveTotalValue += selectedArtifact.baseValue;
            spawnedCount++;
        }
        generatedTotalValue += caveTotalValue;
        Debug.Log($"[ItemGenerator] 동굴 유물 스폰 완료 | 개수: {spawnedCount}개 | 가치: {caveTotalValue} (목표: {caveTargetValue})");
    }

    /// <summary>
    /// 목표 가치의 80%에 도달하도록 땅속에 광물을 매몰시킵니다. (최소 1개 이상)
    /// </summary>
    public void ApplyItemVoxels(float[,,] densities, VoxelType[,,] voxelTypes,
        int width, int height, int depth, float surfaceLevel, VoxelSurfaceGenerator surfaceGen)
    {
        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);

        int currentQuota = (GameManager.Instance != null) ? GameManager.Instance.currentQuota : 0;
        int totalTargetQuota = Mathf.RoundToInt(currentQuota * totalQuotaMultiplier);
        int buriedTargetValue = Mathf.RoundToInt(totalTargetQuota * (1f - caveArtifactRatio));

        // 1회 순회로 모든 아이템 종류에 대한 후보지 수집 (최적화 적용)
        Dictionary<int, List<Vector3Int>> candidateDictionary = 
            CollectAllCandidatePositions(densities, voxelTypes, width, height, depth, surfaceLevel, surfaceGen); 
        List<int> validIndices = new List<int>(candidateDictionary.Keys);

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("[ItemGenerator] 스폰 가능한 땅속 후보지가 없습니다.");
            return;
        }

        int buriedMineralCount = 0;
        int buriedTotalValue = 0;
        int safetyAttempts = 0;
        int maxSafetyAttempts = 2000;

        // 최소 1개 생성 조건과 목표 가치 도달 조건을 통합 관리
        while ((buriedMineralCount < 1 || buriedTotalValue < buriedTargetValue)
               && safetyAttempts < maxSafetyAttempts
               && validIndices.Count > 0)
        {
            safetyAttempts++;

            int randomIndex = validIndices[Random.Range(0, validIndices.Count)];
            ItemSpawnData spawnData = itemSpawnList[randomIndex];

            // 2개 이상부터 가치 오버슈트 방지
            if (buriedMineralCount > 0 && (buriedTotalValue + spawnData.itemData.baseValue > buriedTargetValue + 50))
            {
                if (safetyAttempts > 100)
                {
                    validIndices.Remove(randomIndex);
                }
                // 이번 종류는 가치가 너무 크므로 무시하고 진행
                continue;
            }

            List<Vector3Int> candidates = candidateDictionary[randomIndex];

            if (candidates.Count > 0)
            {
                int posIndex = Random.Range(0, candidates.Count);
                Vector3Int pos = candidates[posIndex];

                // 이미 다른 광물이 매몰된 위치라면 해당 좌표만 제거 후 재시도
                if (voxelTypes[pos.x, pos.y, pos.z] != VoxelType.Dirt)
                {
                    candidates.RemoveAt(posIndex);
                    if (candidates.Count == 0) validIndices.Remove(randomIndex);
                    continue;
                }

                // 위치 결정 및 스폰
                voxelTypes[pos.x, pos.y, pos.z] = spawnData.voxelType;

                // [추가] 에디터 디버그용 월드 좌표 및 색상 저장
                Vector3 localPos = new Vector3(pos.x, pos.y, pos.z) - offset;
                Vector3 worldPos = transform.TransformPoint(localPos);


                // 가치 누적 및 사용된 좌표 제거
                buriedTotalValue += spawnData.itemData.baseValue;
                buriedMineralCount++;
                candidates.RemoveAt(posIndex);

                // 해당 종류의 남은 후보지가 없으면 선택 목록에서 제외
                if (candidates.Count == 0) validIndices.Remove(randomIndex);
            }
        }

        generatedTotalValue += buriedTotalValue;

        Debug.Log($"[ItemGenerator] 매몰 광물 스폰 완료 | 개수: " +
            $"{buriedMineralCount}개 | 가치: {buriedTotalValue} (목표: {buriedTargetValue}) | 전체 총 가치: {generatedTotalValue}");
    }


    /// <summary>
    /// 단 1회의 3중 루프 순회로 모든 스폰 아이템 데이터에 대한 후보지를 일괄 수집
    /// </summary>
    private Dictionary<int, List<Vector3Int>> CollectAllCandidatePositions(float[,,] densities, VoxelType[,,] voxelTypes,
        int width, int height, int depth, float surfaceLevel, VoxelSurfaceGenerator surfaceGen)
    {
        var result = new Dictionary<int, List<Vector3Int>>();

        for (int i = 0; i < itemSpawnList.Count; i++)
        {
            if (itemSpawnList[i].itemData != null)
            {
                result.Add(i, new List<Vector3Int>());
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float surfaceHeight = surfaceGen != null ? surfaceGen.GetSurfaceHeight(x, z) : height;
                int startY = Mathf.Clamp(Mathf.FloorToInt(surfaceHeight), 0, height);

                for (int y = startY; y >= 0; y--)
                {
                    if (densities[x, y, z] > surfaceLevel && voxelTypes[x, y, z] == VoxelType.Dirt)
                    {
                        float depthFromSurface = surfaceHeight - y;

                        for (int i = 0; i < itemSpawnList.Count; i++)
                        {
                            if (!result.ContainsKey(i)) continue;

                            var spawnData = itemSpawnList[i];
                            if (depthFromSurface >= spawnData.minHeight && depthFromSurface <= spawnData.maxHeight)
                            {
                                result[i].Add(new Vector3Int(x, y, z));
                            }
                        }
                    }
                }
            }
        }

        // 후보지가 0개인 항목은 딕셔너리에서 제거
        List<int> keysToRemove = new List<int>();
        foreach (var kvp in result)
        {
            if (kvp.Value.Count == 0) keysToRemove.Add(kvp.Key);
        }
        foreach (var key in keysToRemove)
        {
            result.Remove(key);
        }

        return result;
    }

    // 리스트 무작위 셔플
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
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
        }
        return container;
    }

    // 재생성 시 기존 오브젝트 중복 누적 방지용 삭제 함수
    private void ClearContainerChildren(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            GameObject child = container.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    public GameObject GetFieldPrefab(VoxelType type)
    {
        int index = itemSpawnList.FindIndex(s => s.voxelType == type);
        if (index < 0 || itemSpawnList[index].itemData == null) return null;

        return itemSpawnList[index].itemData.GetDropPrefab();
    }

    public List<ItemSpawnData> GetSpawnList() => itemSpawnList;

    private void OnDrawGizmosSelected()
    {
        if (debugSpawnPositions == null) return;

        Gizmos.color = Color.yellow;
        foreach (var pos in debugSpawnPositions)
        {
            // 스폰 지점에 구체 표시
            Gizmos.DrawSphere(pos, 0.4f);
            // 위쪽 방향으로 선 표시
            Gizmos.DrawRay(pos, Vector3.up * 1.5f);
        }
    }
}