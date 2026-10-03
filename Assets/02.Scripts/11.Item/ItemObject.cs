using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemObject : MonoBehaviour
{
    [Header("아이템 데이터 연결")]
    [SerializeField] private ItemDataSO itemData;

    [Header("매몰 및 물리 설정")]
    [SerializeField] private bool isBuriedOnSpawn = true;
    [SerializeField] private float checkInterval = 0.2f;

    private Rigidbody rb;
    private Collider itemCollider;

    private bool isExposed = false;
    private float timer = 0f;

    public ItemDataSO ItemData => itemData;
    public bool IsBuried => isBuriedOnSpawn && !isExposed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();

        // Awake 시점에 기본 매몰 상태를 확실하게 적용
        if (isBuriedOnSpawn)
        {
            ApplyBuriedState();
        }
    }

    private void Update()
    {
        // 땅속에 매몰된 상태일 때만 땅이 파였는지 검사
        if (IsBuried)
        {
            timer += Time.deltaTime;
            if (timer >= checkInterval)
            {
                timer = 0f;
                CheckIfFullyExposed();
            }
        }
    }

    public void Initialize(ItemDataSO data, bool isBuried = false)
    {
        itemData = data;
        isBuriedOnSpawn = isBuried;
        if (isBuried)
            ApplyBuriedState();
        else
            UnfreezePhysics();
    }

    private void ApplyBuriedState()
    {
        if (IsBuried)
        {
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (itemCollider != null)
            {
                itemCollider.isTrigger = true;
            }
        }
    }

    private void CheckIfFullyExposed()
    {
        if (VoxelWorld.Instance == null) return;

        Vector3 worldPos = transform.position;
        Vector3 offset = VoxelWorld.Instance.GetWorldOffset();

        // 아이템의 중심 좌표를 복셀 좌표로 변환
        int x = Mathf.FloorToInt(worldPos.x + offset.x);
        int y = Mathf.FloorToInt(worldPos.y + offset.y);
        int z = Mathf.FloorToInt(worldPos.z + offset.z);

        float density = VoxelWorld.Instance.GetDensity(x, y, z);
        float surfaceLevel = VoxelWorld.Instance.surfaceLevel;

        if (density < surfaceLevel)
        {
            ExposeItem();
        }
    }

    private void ExposeItem()
    {
        isExposed = true;
        UnfreezePhysics();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 외부(버리기, 발굴 등)에서 물리를 켤 때 호출
    /// </summary>
    public void UnfreezePhysics()
    {
        isBuriedOnSpawn = false;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        if (itemCollider != null)
        {
            itemCollider.isTrigger = false;
        }
    }
}