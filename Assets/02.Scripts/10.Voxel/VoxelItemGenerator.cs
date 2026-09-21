using UnityEngine;
using System.Collections.Generic;


public class VoxelItemGenerator : MonoBehaviour
{

    // [추가] 에디터 디버그용 매몰 아이템 정보 구조체 정의
    [System.Serializable]
    private struct BuriedItemDebugInfo
    {
        public Vector3 worldPos;
        public VoxelType type;
        public Color color;
    }


    [Header("Item Spawn Settings")]
    [SerializeField] private List<ItemSpawnData> itemSpawnList = new List<ItemSpawnData>();

    [Header("Cave Artifact Settings")]
    [SerializeField] private List<ItemData> caveArtifactDataList = new List<ItemData>();
    [Range(0f, 1f)]
    [SerializeField] private float artifactSpawnChance = 0.7f;
    [Header("Artifact Burial Depth Settings")]
    [Tooltip("땅속에 파묻히는 최소 높이 오프셋 (0.5 = 절반 매몰)")]
    [Range(0.3f, 1.0f)]
    [SerializeField] private float minArtifactHeightOffset = 0.5f;

    [Tooltip("땅속에 파묻히는 최대 높이 오프셋 (0.9 = 살짝 매몰)")]
    [Range(0.3f, 1.0f)]
    [SerializeField] private float maxArtifactHeightOffset = 0.85f;

    [SerializeField] private int generatedTotalValue = 0;


    [Header("Editor Debug Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private float gizmoSize = 0.8f;
    private List<BuriedItemDebugInfo> buriedItemDebugList = new List<BuriedItemDebugInfo>();

    /// <summary>
    /// 동굴 방(Chamber) 중심점들을 기반으로 동굴 바닥을 탐색하여 아이템 프리팹을 배치
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public void SpawnCaveArtifacts(float[,,] densities, List<Vector3Int> chamberCenters,
        int width, int height, int depth, float surfaceLevel)
    {
        generatedTotalValue = 0; // 전체 아이템 가치 초기화

        if (caveArtifactDataList == null || caveArtifactDataList.Count == 0 ||
            chamberCenters == null || chamberCenters.Count == 0) return;

        // 생성된 유물 정리용 컨테이너
        Transform artifactContainer = GetOrCreateContainer("[Group] Cave Artifacts", transform);
        ClearContainerChildren(artifactContainer);

        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);

        List<Vector3> validSpawnPositions = new List<Vector3>();

        foreach (var chamber in chamberCenters)
        {
            // 방 중심점이 실제로 뚫린 동굴(공기)인지 확인 (땅속이면 스폰하지 않음)
            if (densities[chamber.x, chamber.y, chamber.z] > surfaceLevel) continue;

            // 동굴 공기층에서 아래로 내려가며 가장 먼저 만나는 땅 복셀 감지
            int floorY = -1;
            for (int y = chamber.y; y >= 0; y--)
            {
                if (densities[chamber.x, y, chamber.z] > surfaceLevel)
                {
                    floorY = y;
                    break;
                }
            }

            // 땅 복셀 바로 위(floorY + 1.0f) 공기 공간에 정상 배치
            if (floorY != -1)
            {
                // 설정된 범위 내에서 랜덤하게 파묻히는 높이 계산
                float randomOffset = Random.Range(minArtifactHeightOffset, maxArtifactHeightOffset);

                Vector3 localPos = new Vector3(chamber.x, floorY + randomOffset, chamber.z) - offset;
                Vector3 worldSpawnPos = transform.TransformPoint(localPos);

                validSpawnPositions.Add(worldSpawnPos);
            }
        }

        if (validSpawnPositions.Count == 0) return;

        // 전체 맵 상한선 (할당량 * 1.5)
        int maxTargetValue = Mathf.RoundToInt(GameManager.Instance.currentQuota * 1.5f);

        // 무작위 셔플
        ShuffleList(validSpawnPositions);
        int spawnedCount = 0;

        foreach (var spawnPos in validSpawnPositions)
        {
            // 첫 번째 유물(spawnedCount == 0)은 확률 검사 무시하여 스폰 보장
            if (spawnedCount > 0 && Random.value > artifactSpawnChance) continue;

            ItemData selectedArtifact = caveArtifactDataList[Random.Range(0, caveArtifactDataList.Count)];
            if (selectedArtifact == null || selectedArtifact.fieldPrefab == null) continue;

            // 첫 번째 스폰이 아닐 때만 상한선 제한 검사
            if (spawnedCount > 0 && generatedTotalValue + selectedArtifact.baseValue > maxTargetValue) continue;

            Quaternion randomRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(selectedArtifact.fieldPrefab, spawnPos, randomRot, artifactContainer);

            generatedTotalValue += selectedArtifact.baseValue;
            spawnedCount++;
        }

        Debug.Log($"[ItemGenerator] 동굴 유물 스폰 완료 | 개수: {spawnedCount}개 | 총 가치: {generatedTotalValue}");
    }


    /// <summary>
    /// (동굴 유물 가치 + 땅속 광물 가치)가 [Quota * 1.2 ~ 1.5] 사이가 되도록 땅속에 광물을 매몰시킵니다.
    /// </summary>
    public void ApplyItemVoxels(float[,,] densities, VoxelType[,,] voxelTypes,
        int width, int height, int depth, float surfaceLevel, VoxelSurfaceGenerator surfaceGen)
    {
        // 디버그 리스트 초기화
        buriedItemDebugList.Clear();
        Vector3 offset = new Vector3(width / 2f, height / 2f, depth / 2f);

        int quota = GameManager.Instance.currentQuota;
        int minTargetValue = Mathf.RoundToInt(quota * 1.2f);
        int maxTargetValue = Mathf.RoundToInt(quota * 1.5f);
        int chosenTargetValue = Random.Range(minTargetValue, maxTargetValue + 1);        // 최종적으로 맞추고자 하는 목표 가치

        // 각 별로 조건에 맞는 좌표에 사전에 모아둠
        Dictionary<int, List<Vector3Int>> candidateDictionary = new Dictionary<int, List<Vector3Int>>();
        List<int> validIndices = new List<int>();

        for (int i = 0; i < itemSpawnList.Count; i++)
        {
            if (itemSpawnList[i].itemData == null) continue;

            List<Vector3Int> candidates = GetCandidatePositions(densities, voxelTypes, width, height, depth, surfaceLevel, surfaceGen, itemSpawnList[i]);
            if (candidates.Count > 0)
            {
                candidateDictionary.Add(i, candidates);
                validIndices.Add(i); // 후보지가 존재하는 인덱스만 스폰풀에 추가
            }
        }

        if (validIndices.Count == 0)
        {
            Debug.LogWarning("[ItemGenerator] 스폰 가능한 땅속 후보지가 없습니다.");
            return;
        }

        int buriedMineralCount = 0;
        int safetyAttempts = 0;
        int maxSafetyAttempts = 2000;

        // 최소 1개 생성 조건과 목표 가치 도달 조건을 통합 관리
        while ((buriedMineralCount < 1 || generatedTotalValue < chosenTargetValue)
               && safetyAttempts < maxSafetyAttempts
               && validIndices.Count > 0)
        {
            safetyAttempts++;

            int randomIndex = validIndices[Random.Range(0, validIndices.Count)];
            ItemSpawnData spawnData = itemSpawnList[randomIndex];

            if (buriedMineralCount > 0 && (generatedTotalValue + spawnData.itemData.baseValue > maxTargetValue))
            {
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

                // 종류별 Gizmo 색상 지정 (필요 시 수정)
                Color itemColor = GetGizmoColorForVoxelType(spawnData.voxelType);

                buriedItemDebugList.Add(new BuriedItemDebugInfo
                {
                    worldPos = worldPos,
                    type = spawnData.voxelType,
                    color = itemColor
                });

                // 가치 누적 및 사용된 좌표 제거
                generatedTotalValue += spawnData.itemData.baseValue;
                buriedMineralCount++;
                candidates.RemoveAt(posIndex);

                // 해당 종류의 남은 후보지가 없으면 선택 목록에서 제외
                if (candidates.Count == 0) validIndices.Remove(randomIndex);
            }
        }

        Debug.Log($"[ItemGenerator] 전체 아이템 스폰 완료! " +
            $"(매몰 광물: {buriedMineralCount}개 / 목표 범위: {minTargetValue}~{maxTargetValue} / " +
            $"실제 총 가치: {generatedTotalValue})");
    }

    /// <summary>
    /// 특정 아이템의 생성 조건에 부합하는 땅속 좌표 리스트 수집
    /// </summary>
    private List<Vector3Int> GetCandidatePositions(float[,,] densities, VoxelType[,,] voxelTypes,
        int width, int height, int depth, float surfaceLevel, VoxelSurfaceGenerator surfaceGen, ItemSpawnData spawnData)
    {
        List<Vector3Int> candidates = new List<Vector3Int>();

        for (int x = 0; x <= width; x++)
        {
            for (int z = 0; z <= depth; z++)
            {
                float surfaceHeight = surfaceGen != null ? surfaceGen.GetSurfaceHeight(x, z) : height;
                int startY = Mathf.Clamp(Mathf.FloorToInt(surfaceHeight), 0, height);

                for (int y = startY; y >= 0; y--)
                {
                    if (densities[x, y, z] > surfaceLevel && voxelTypes[x, y, z] == VoxelType.Dirt)
                    {
                        float depthFromSurface = surfaceHeight - y;
                        if (depthFromSurface >= spawnData.minHeight && depthFromSurface <= spawnData.maxHeight)
                        {
                            candidates.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }
        }

        return candidates;
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
        ItemSpawnData data = itemSpawnList.Find(s => s.voxelType == type);
        return data.itemData != null ? data.itemData.fieldPrefab : null;
    }

    public List<ItemSpawnData> GetSpawnList() => itemSpawnList;


    // VoxelType별 Gizmos 표시 색상 지정
    private Color GetGizmoColorForVoxelType(VoxelType type)
    {
        switch (type)
        {
            case VoxelType.Iron: return Color.gray;
            case VoxelType.Gold: return Color.yellow;
            default: return Color.green;
        }
    }
    // 에디터 Scene 뷰에 디버그용 기즈모 그리기 (수정됨)
    private void OnDrawGizmos()
    {
        if (!showGizmos || buriedItemDebugList == null) return;

        foreach (var item in buriedItemDebugList)
        {
            // 1. 와이어프레임 선 그리기
            Gizmos.color = item.color;
            Gizmos.DrawWireCube(item.worldPos, Vector3.one * gizmoSize);

            // 2. 반투명 큐브 채우기 (Gizmos.DrawCube 함수명으로 수정)
            Gizmos.color = new Color(item.color.r, item.color.g, item.color.b, 0.35f);
            Gizmos.DrawCube(item.worldPos, Vector3.one * gizmoSize);
        }
    }
}