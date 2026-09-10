using UnityEngine;

public class VoxelSurfaceGenerator : MonoBehaviour
{
    [Header("Seed Settings (시드 설정)")]
    public bool useRandomSeed = true;
    [SerializeField] private int currentAppliedSeed; // 인스펙터에서 수정 불가능하게 보이기만 함
    private Vector2 surfaceOffset;

    [Header("Surface Settings")]
    public float baseTerrainHeight = 20f; // 기본 지면 높이
    public float mountainHeight = 30f;    // 산/언덕의 최대 높이
    [Range(0.005f, 0.2f)]
    public float terrainScale = 0.04f;    // 지형 굴곡 크기

    [Header("Bedrock Settings (최하단 암반 / 추락 방지)")]
    [Tooltip("맵 최하단에 절대 파지지 않는 땅의 두께 (플레이어 맵 밖 추락 방지)")]
    public int bottomBedrockHeight = 3;

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
        if(useRandomSeed)
        {
            currentAppliedSeed = Random.Range(-999999, 999999);
        }
        else
        {
            currentAppliedSeed = 0; // 기본 시드값
        }
        Random.InitState(currentAppliedSeed);

        surfaceOffset = new Vector2(Random.Range(-50000f, 50000f), Random.Range(-50000f, 50000f));
    }

    public void GenerateSurface(float[,,] densities, VoxelType[,,] voxelTypes, 
        int width, int height, int depth, float surfaceLevel)
    {
        for (int x = 0; x <= width; x++)
        {
            for (int z = 0; z <= depth; z++)
            {
                // PerlinNoise로 표면 높이 계산
                float surfaceHeight = GetSurfaceHeight(x, z);

                for (int y = 0; y <= height; y++)
                {
                    // 최하단 암반 처리 (바닥 뚫림 및 추락 완전 방지)
                    if (y <= bottomBedrockHeight)
                    {
                        densities[x, y, z] = 2f; // 강제로 아주 단단한 땅 설정
                        voxelTypes[x, y, z] = VoxelType.Dirt;
                        continue;
                    }

                    float density = surfaceHeight - y + surfaceLevel;
                    densities[x, y, z] = Mathf.Clamp(density, -1f, 2f);

                    if (densities[x, y, z] > surfaceLevel)
                    {
                        voxelTypes[x,y,z] = VoxelType.Dirt; // 지면 위는 Dirt
                    }
                    else
                    {
                        voxelTypes[x, y, z] = VoxelType.Air;
                    }
                }
            }
        }
    }
    public float GetSurfaceHeight(float x, float z)
    {
        return baseTerrainHeight + Mathf.PerlinNoise(
            x * terrainScale + surfaceOffset.x, 
            z * terrainScale + surfaceOffset.y) * mountainHeight;
    }
}
