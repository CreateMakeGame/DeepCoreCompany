using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 16;
    public int height = 8;
    public int depth = 16;
    public float gridSpacing = 1.0f; // 격자 간격

    private float[,,] densityMap; // 밀도 데이터 (0: 공기, 1: 흙)
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        // 1. 밀도 맵 초기화
        densityMap = new float[width, height, depth];
        InitializeDensity();
    }

    void InitializeDensity()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // 일단 지표면(절반 높이) 아래는 꽉 차게 설정
                    densityMap[x, y, z] = (y < height / 2) ? 1f : 0f;
                }
            }
        }
    }

    public void Dig(Vector3 worldPosion, float radius)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosion) / gridSpacing; // 월드 좌표를 로컬 격자 좌표로 변환

        int centerX = Mathf.RoundToInt(localPos.x);
        int centerY = Mathf.RoundToInt(localPos.y);
        int centerZ = Mathf.RoundToInt(localPos.z);

        int r = Mathf.CeilToInt(radius / gridSpacing);
        bool isChanged = false;

        // 둥글게 파내기
        for (int x = centerX - r; x <= centerX + r; x++)
        {
            for (int y = centerY - r; y <= centerY + r; y++)
            {
                for (int z = centerZ - r; z <= centerZ + r; z++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < depth)
                    {
                        float distance = Vector3.Distance(new Vector3(x, y, z), localPos);
                        if (distance <= radius / gridSpacing)
                        {
                            densityMap[x, y, z] = 0f; // 공기로 설정
                            isChanged = true;
                        }
                    }
                }
            }
        }

        if (isChanged) UpdateMesh();
    }

    private void UpdateMesh()
    {
        Debug.Log("지형 데이터가 깎였습니다! (부드러운 메쉬 생성 로직 대기 중...)");
    }
}