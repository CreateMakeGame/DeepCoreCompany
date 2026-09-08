using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelCaveGenerator : MonoBehaviour
{
    [Header("Seed Settings (시드 설정)")]
    public int seed = 0;
    public bool useRandomSeed = true;
    [SerializeField] private int currentAppliedSeed; // 인스펙터에서 수정 불가능하게 보이기만 함

    private Vector3 noiseOffsetA;
    private Vector3 noiseOffsetB;
    private Vector3 noiseOffsetC; // 넓은 방 생성용 오프셋

    [Header("Layered Cave Settings (다층 동굴 설정)")]
    public bool enableCaves = true;             // 동굴 생성 여부
    [Range(0.01f, 0.06f)] public float caveScale = 0.025f;      // 굴곡 주기 (작을수록 길게 뻗음)
    [Range(0.02f, 0.15f)] public float tunnelRadius = 0.06f;    // 통로 굵기 (작을수록 얇은 개미굴)

    [Header("Chamber / Cavern Settings (넓은 방 설정)")]
    [Tooltip("넓은 방의 출현 빈도 스케일 (작을수록 거대한 방이 띄엄띄엄 생성)")]
    [Range(0.005f, 0.03f)] public float chamberScale = 0.012f;
    [Tooltip("방이 생성되는 노이즈 문턱값 (높을수록 방이 희귀해짐)")]
    [Range(0.5f, 0.8f)] public float chamberThreshold = 0.62f;
    [Tooltip("방이 생성될 때 최대 굵기 (크면 거대한 동굴이 됨)")]
    [Range(0.15f, 0.5f)] public float maxChamberRadius = 0.35f;

    [Header("Cave Boundary Settings (동굴 생성 영역 제한)")]
    [Tooltip("X축 좌우 가장자리에서 동굴을 생성하지 않을 두께 (외벽 보호)")]
    public int caveMarginX = 3;
    [Tooltip("Z축 앞뒤 가장자리에서 동굴을 생성하지 않을 두께 (외벽 보호)")]
    public int caveMarginZ = 3;
    [Tooltip("지표면 바로 아래 동굴이 뚫리지 않도록 보호할 천장 두께")]
    public int caveSurfaceMargin = 15;
    [Tooltip("동굴이 생성될 최소 Y 높이")]
    public int caveMinHeight = 2;
    [Tooltip("동굴이 생성될 최대 Y 높이")]
    public int caveMaxHeight = 40;

    [Header("Boundary Fade Settings (경계선 부드러운 감쇄)")]
    [Tooltip("경계선에 도달하기 전 동굴이 자연스럽게 막히도록 하는 마감 거리")]
    [Range(1f, 15f)] public float boundaryFadeDistance = 6f;

    // int 타입 좌표 리스트로 변경하여 복셀 인덱스 관리를 명확히 함
    private List<Vector3Int> chamberCenters = new List<Vector3Int>();
    public List<Vector3Int> GetChamberCenters() => chamberCenters;

    private void OnValidate()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null && gameObject.activeInHierarchy)
            {
                GetComponent<VoxelTerrain>()?.GenerateTerrain();
            }
        };
#endif
    }

    public void InitializeOffsets()
    {
        currentAppliedSeed = useRandomSeed ? Random.Range(-999999, 999999) : seed;
        Random.InitState(currentAppliedSeed);

        noiseOffsetA = new Vector3(Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f));
        noiseOffsetB = new Vector3(Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f));
        noiseOffsetC = new Vector3(Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f));
    }

    private void UpdateEditorOffsets()
    {
        Random.InitState(seed);

        noiseOffsetA = new Vector3(Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f));
        noiseOffsetB = new Vector3(Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f));
        noiseOffsetC = new Vector3(Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f));
    }

    public void ApplyCaves(float[,,] densities, VoxelType[,,] voxelTypes,
        int width, int height, int depth, VoxelSurfaceGenerator surfaceGen)
    {
        if (!enableCaves) return;

        if (noiseOffsetA == Vector3.zero && noiseOffsetB == Vector3.zero)
        {
            if (Application.isPlaying) UpdateEditorOffsets();
        }

        chamberCenters.Clear(); // 이전에 생성된 방 중심 좌표 초기화

        for (int x = caveMarginX; x <= width - caveMarginX; x++)
        {
            for (int z = caveMarginZ; z <= depth - caveMarginZ; z++)
            {
                float surfaceHeight = surfaceGen != null ? surfaceGen.GetSurfaceHeight(x, z) : height;

                for (int y = caveMinHeight; y <= caveMaxHeight; y++)
                {
                    // 각 축의 경계선까지 남은 거리 계산 (동굴 생성 범위 제한)
                    float distX = Mathf.Min(x - caveMarginX, (width - caveMarginX) - x);
                    float distZ = Mathf.Min(z - caveMarginZ, (depth - caveMarginZ) - z);
                    float distY = Mathf.Min(y - caveMinHeight, (surfaceHeight - caveSurfaceMargin) - y);

                    if (distX < 0 || distZ < 0 || distY < 0) continue;

                    // 경계면에 다가갈수록 1.0에서 0.0으로 부드럽게 떨어지는 페이드 가중치 계산
                    float fadeX = Mathf.Clamp01(distX / boundaryFadeDistance);
                    float fadeZ = Mathf.Clamp01(distZ / boundaryFadeDistance);
                    float fadeY = Mathf.Clamp01(distY / boundaryFadeDistance);

                    float boundaryFade = Mathf.SmoothStep(0f, 1f, fadeX * fadeZ * fadeY);

                    float n1 = Get3DNoise(x + noiseOffsetA.x, y + noiseOffsetA.y, z + noiseOffsetA.z, caveScale);
                    float n2 = Get3DNoise(x + noiseOffsetB.x, y + noiseOffsetB.y, z + noiseOffsetB.z, caveScale);

                    // 두 노이즈의 중심축(0.5)으로부터의 거리 계산
                    float d1 = Mathf.Pow(n1 - 0.5f, 2);
                    float d2 = Mathf.Pow(n2 - 0.5f, 2);
                    float tunnelDist = Mathf.Sqrt(d1 + d2);

                    // 넓은 방 생성용 저주파 노이즈 연산
                    float chamberNoise = Get3DNoise(x + noiseOffsetC.x, y + noiseOffsetC.y, z + noiseOffsetC.z, chamberScale);
                    // 기본 통로 굵기로 시작
                    float currentRadius = tunnelRadius;

                    if (chamberNoise > chamberThreshold)
                    {
                        float chamberFactor = (chamberNoise - chamberThreshold) / (1f - chamberThreshold);
                        chamberFactor = Mathf.SmoothStep(0f, 1f, chamberFactor);
                        // 통로 반지름을 방 크기로 확장하는 핵심 코드 추가
                        currentRadius = Mathf.Lerp(tunnelRadius, maxChamberRadius, chamberFactor);

                        Vector3Int currentPos = new Vector3Int(x, y, z);
                        if (chamberNoise > chamberThreshold + 0.02f && IsFarFromOtherChambers(currentPos, 12f))
                        {
                            chamberCenters.Add(currentPos);
                        }
                    }

                    // 동굴 굴착 및 바닥 평탄화 가공
                    if (tunnelDist < currentRadius)
                    {
                        float carveFactor = Mathf.SmoothStep(0f, 1f, (1f - (tunnelDist / currentRadius)) * boundaryFade);

                        // 방 구역일 경우 아래쪽 바닥을 좀 더 평평하게 깎아냄
                        if (chamberNoise > chamberThreshold && y < caveMaxHeight)
                        {
                            carveFactor = Mathf.Pow(carveFactor, 0.7f);
                        }

                        densities[x, y, z] = Mathf.Lerp(densities[x, y, z], -1.0f, carveFactor);

                        if (densities[x, y, z] <= 0.5f)
                        {
                            voxelTypes[x, y, z] = VoxelType.Air;
                        }
                    }
                }
            }
        }
        //Debug.Log($"[VoxelCaveGenerator] 생성된 방(Chamber) 개수: {chamberCenters.Count}");
    }

    // 방 중심점끼리 너무 가깝게 붙지 않도록 거리를 검사하는 함수
    private bool IsFarFromOtherChambers(Vector3 position, float minDistance)
    {
        foreach (var center in chamberCenters)
        {
            if (Vector3.Distance(center, position) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    // X, Z 축 스케일과 Y 축 스케일을 분리한 3D 노이즈 함수
    private float Get3DNoise(float x, float y, float z, float scale)
    {
        float sx = x * scale;
        float sy = y * scale;
        float sz = z * scale;

        float xy = Mathf.PerlinNoise(sx, sy);
        float yz = Mathf.PerlinNoise(sy, sz);
        float zx = Mathf.PerlinNoise(sz, sx);

        return (xy + yz + zx) / 3f;
    }
}
