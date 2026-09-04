using UnityEngine;

public class AutoDestroyOnTerrainFall : MonoBehaviour
{
    [Header("지형 체크 설정")]
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private float checkDistance = 0.5f;

    private void Start()
    {
        // 성능을 위해 매 프레임 대신 0.2초마다 체크
        InvokeRepeating(nameof(CheckGround), 0.1f, 0.2f);
    }

    private void CheckGround()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        if (!Physics.Raycast(rayStart, Vector3.down, checkDistance, terrainLayer))
        {
            Destroy(gameObject);
        }
    }
}