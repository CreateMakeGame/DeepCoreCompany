using UnityEngine;

public class GlobalManagers : Singleton<GlobalManagers>
{
    public GameManager Game { get; private set; }
    public CameraManager Camera { get; private set; }
    public Inventory Inventory { get; private set; }
    public ContractManager Contract { get; private set; }
    public UIManager UI { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InitManagers();
    }

    private void InitManagers()
    {
        Game = GetOrCreateManager<GameManager>();
        Camera = GetOrCreateManager<CameraManager>();
        Inventory = GetOrCreateManager<Inventory>();
        Contract = GetOrCreateManager<ContractManager>();
        UI = GetOrCreateManager<UIManager>();
    }

    private T AddChildManager<T>() where T : MonoBehaviour
    {
        GameObject child = new GameObject(typeof(T).Name);
        child.transform.parent = transform;
        return child.AddComponent<T>();
    }

    private T GetOrCreateManager<T>() where T : MonoBehaviour
    {
        T manager = GetComponentInChildren<T>();
        if (manager == null)
        {
            manager = AddChildManager<T>();
        }
        return manager;
    }
}
