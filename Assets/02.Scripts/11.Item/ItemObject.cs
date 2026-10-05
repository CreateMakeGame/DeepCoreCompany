using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemObject : MonoBehaviour
{
    [Header("아이템 데이터 연결")]
    [SerializeField] private ItemDataSO itemData;

    [Header("매몰 및 물리 설정")]
    [SerializeField] private bool isBuriedOnSpawn = true;
    [Header("발굴 판정")]
    [Tooltip("이 거리 안의 복셀이 파괴되면 아이템이 발굴됩니다.")]
    [SerializeField] private float revealRadius = 1.5f;

    private Rigidbody rb;
    private Collider itemCollider;

    private bool isExposed = false;

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

    /// <summary>
    /// 아이템 데이터를 초기화합니다.
    /// buried = true  : 동굴에서 처음 생성되는 매몰 유물
    /// buried = false : 일반 드롭 또는 발굴 완료 아이템
    /// </summary>
    public void Initialize(ItemDataSO data, bool buried)
    {
        itemData = data;
        isBuriedOnSpawn = buried;
        isExposed = false;

        if (buried)
            ApplyBuriedState();
        else
            UnfreezePhysics();
    }

    /// <summary>
    /// VoxelWorld에서 실제로 복셀이 파괴되었을 때 호출됩니다.
    /// </summary>
    public void OnVoxelDug(Vector3 dugPosition)
    {
        if (!IsBuried)
            return;

        float sqrDistance =
            (transform.position - dugPosition).sqrMagnitude;

        float sqrRevealRadius =
            revealRadius * revealRadius;

        if (sqrDistance <= sqrRevealRadius)
        {
            ExposeItem();
        }
    }

    /// <summary>
    /// 아이템을 땅속에 묻힌 상태로 설정
    /// </summary>
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
                // 땅속에 있는 동안 물리 충돌로 튀어나오지 않도록 Trigger
                itemCollider.isTrigger = true;
            }
        }
    }


    /// <summary>
    /// 발굴 완료 처리
    /// </summary>
    private void ExposeItem()
    {
        if (isExposed)
            return;

        isExposed = true;
        isBuriedOnSpawn = false;

        if (itemData != null &&
            itemData.isSpecialCondition &&
            itemData.specialFieldPrefab != null)
        {
            SpawnSpecialFieldPrefab();
            return;
        }
     
        UnfreezePhysics();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 현재 fieldPrefab을 specialFieldPrefab으로 교체합니다.
    /// </summary>
    private void SpawnSpecialFieldPrefab()
    {
        if (itemData == null)
            return;

        if (itemData.specialFieldPrefab == null)
            return;

        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = transform.rotation;

        GameObject specialObject = Instantiate(
            itemData.specialFieldPrefab,
            spawnPosition,
            spawnRotation,
            transform.parent
        );

        // specialFieldPrefab에도 ItemObject가 있다면
        // 반드시 노출 상태로 초기화
        ItemObject specialItem =
            specialObject.GetComponent<ItemObject>();

        if (specialItem != null)
        {
            specialItem.Initialize(itemData, false);
        }

        Debug.Log(
            $"[ItemObject] 유물 발굴 완료 → " +
            $"Special Prefab 생성: {specialObject.name}"
        );

        Destroy(gameObject);
    }


    /// <summary>
    /// 외부에서 물리를 활성화할 때 호출
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