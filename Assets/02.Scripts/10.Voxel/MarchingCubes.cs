using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubes : MonoBehaviour
{
    public int width = 20;
    public int height = 20;
    public int depth = 20;
    public float surfaceLevel = 0.5f;

    private float[,,] densities;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private Mesh mesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        GenerateDensities();    // 1. 밀도 데이터 생성
        MarchAllCubes();        // 2. 테이블 참조해서 메쉬 계산
        UpdateMesh();           // 3. Unity Mesh에 적용
    }
    
    private void GenerateDensities()
    {
        densities = new float[width + 1, height + 1, depth + 1];
        for(int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = 0; z <= depth; z++)
                {
                    // 표면 높이를 노이즈로 결정 (예: 전체 높이의 절반 정도)
                    float surfaceHeight = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * (height * 0.5f) + (height * 0.2f);                    // 높이가 노이즈보다 낮으면 밀도를 높게(땅) 설정
                    densities[x, y, z] = (y < surfaceHeight) ? 1f : 0f;
                }
            }
        }
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
            Vector3 cornerPos = new Vector3(x,y, z) + MarchingTables.CornerOffsets[i];
            if (densities[(int)cornerPos.x, (int)cornerPos.y, (int)cornerPos.z] > surfaceLevel)
            {
                cubeIndex |= 1 << i; // 해당 코너가 표면 위에 있으면 비트 설정
            }
        }
        for(int i = 0; MarchingTables.TriangleConnectionTable[cubeIndex, i] != -1; i+=3)
        {
            // 삼각형의 세 변(엣지) 인덱스
            int edge0 = MarchingTables.TriangleConnectionTable[cubeIndex, i];
            int edge1 = MarchingTables.TriangleConnectionTable[cubeIndex, i + 1];
            int edge2 = MarchingTables.TriangleConnectionTable[cubeIndex, i + 2];

            // 엣지의 중간 지점에 정점 생성 (보간을 생략한 단순 버전)
            vertices.Add(GetEdgeCenter(x, y, z, edge0));
            vertices.Add(GetEdgeCenter(x, y, z, edge1));
            vertices.Add(GetEdgeCenter(x, y, z, edge2));

            triangles.Add(vertices.Count - 3);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 1);
        }
    }

    private Vector3 GetEdgeCenter(int x, int y, int z, int edge0)
    {
        int v0 = MarchingTables.EdgeConnection[edge0, 0];
        int v1 = MarchingTables.EdgeConnection[edge0, 1];

        Vector2 pos0 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v0];
        Vector2 pos1 = new Vector3(x, y, z) + MarchingTables.CornerOffsets[v1];

        return (pos0 + pos1) / 2; // 단순히 중간 지점 반환 (보간 생략)
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); // 빛 반사를 위해 법선 계산

        MeshCollider collider = GetComponent<MeshCollider>();
        if(collider != null)
        {
            collider.sharedMesh = null; // 이전 메쉬 정보 초기화
            collider.sharedMesh = mesh;
        }
    }
}
