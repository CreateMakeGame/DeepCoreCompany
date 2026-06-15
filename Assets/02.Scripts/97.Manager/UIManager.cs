using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance => GlobalManagers.Instance != null ? GlobalManagers.Instance.UI : null;

    [Header("Global UI Prefab (런타임에 소환할 프리팹)")]
    [SerializeField] private GameObject globalUIRootPrefab;
    public GameObject InventoryUI { get; private set; }
    public GameObject InteractableUI { get; private set; }

    // 현재 씬에 존재하는 로컬 UI들을 타입별로 안전하게 보관할 딕셔너리
    private Dictionary<System.Type, MonoBehaviour> localUIs = new Dictionary<System.Type, MonoBehaviour>();
    private List<GameObject> openUIStack = new List<GameObject>();

    private void Awake()
    {
        InitGlobalUIRoot();
    }

    private void InitGlobalUIRoot()
    {
        if (globalUIRootPrefab != null)
        {

            GameObject globalUIRoot = Instantiate(globalUIRootPrefab);
            globalUIRoot.name = "GlobalUIRoot";

            globalUIRoot.transform.SetParent(null); // 최상위로 설정
            DontDestroyOnLoad(globalUIRoot);

            // 글로벌 UI 요소들을 프리팹에서 찾아서 참조로 저장
            Transform invTransform = globalUIRoot.transform.Find("GlobalScreenCanvas/InventoryUI");
            if (invTransform != null) InventoryUI = invTransform.gameObject;

            Transform interTransform = globalUIRoot.transform.Find("GlobalHUDCanvas/InteractableUI");
            if (interTransform != null) InteractableUI = interTransform.gameObject;

            // 필요한 경우, 글로벌 UI 요소들을 초기 상태로 설정
            if (InventoryUI != null) InventoryUI.SetActive(false);
            if (InteractableUI != null) InteractableUI.SetActive(true);
        }
        
    }

    #region 로컬 UI 동적 등록 시스템
    public void RegisterLocalUI<T>(T uiInstance) where T : MonoBehaviour
    {
        System.Type type = typeof(T);
        if (!localUIs.ContainsKey(type))
        {
            localUIs.Add(type, uiInstance);
        }
    }

    public void UnregisterLocalUI<T>() where T : MonoBehaviour
    {
        System.Type type = typeof(T);
        if (localUIs.ContainsKey(type))
        {
            localUIs.Remove(type);
        }
    }

    // 다른 스크립트(예: BulletinBoard)에서 ContractBoardUI를 찾고 싶을 때 호출할 함수
    public T GetLocalUI<T>() where T : MonoBehaviour
    {
        System.Type type = typeof(T);
        if (localUIs.TryGetValue(type, out var ui))
        {
            return ui as T;
        }
        return null;
    }
    #endregion

    #region UI 열기/닫기 및 마우스 커서 제어
    public void OpenUI(GameObject uiPanel)
    {
        if (uiPanel == null || uiPanel.activeSelf) return;
        uiPanel.SetActive(true);
        if (!openUIStack.Contains(uiPanel)) openUIStack.Add(uiPanel);
        RefreshCursorState();
    }

    public void CloseUI(GameObject uiPanel)
    {
        if (uiPanel == null || !uiPanel.activeSelf) return;
        uiPanel.SetActive(false);
        openUIStack.Remove(uiPanel);
        RefreshCursorState();
    }

    private void RefreshCursorState()
    {
        if (openUIStack.Count > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    #endregion
}