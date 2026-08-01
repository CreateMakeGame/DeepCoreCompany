using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance => GlobalManagers.Instance != null ? GlobalManagers.Instance.UI : null;

    [Header("Global UI Prefab (런타임에 소환할 프리팹)")]
    [SerializeField] private GameObject globalUIRootPrefab;
    private GameObject globalUIRootInstance;

    public GameObject InventoryUI { get; private set; }
    public InteractableUI InteractableUI { get; private set; }
    public GameObject MainHUDPanel { get; private set; }

    // 현재 씬에 존재하는 로컬 UI들을 타입별로 안전하게 보관할 딕셔너리
    private Dictionary<System.Type, MonoBehaviour> localUIs = new Dictionary<System.Type, MonoBehaviour>();
    private List<GameObject> openUIStack = new List<GameObject>();

    private void Awake()
    {
        InitGlobalUIRoot();
    }
    private void OnEnable()
    {
        // 씬 로드 이벤트 구독 (씬 이동 시 카메라 재연결용)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitGlobalUIRoot()
    {
        if (globalUIRootPrefab != null)
        {

            globalUIRootInstance = Instantiate(globalUIRootPrefab);
            globalUIRootInstance.name = "GlobalUIRoot";

            globalUIRootInstance.transform.SetParent(null); // 최상위로 설정
            DontDestroyOnLoad(globalUIRootInstance);

            // 글로벌 UI 요소들을 프리팹에서 찾아서 참조로 저장
            PrefabReference(globalUIRootInstance);

            // 필요한 경우, 글로벌 UI 요소들을 초기 상태로 설정
            if (InventoryUI != null) InventoryUI.SetActive(false);

            RefreshCanvasCamera();// 초기 카메라 설정
        }
        else
        {
            Debug.LogError("[UIManager] globalUIRootPrefab이 인스펙터에 할당되지 않았습니다!");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환이 완료되면 새로 로드된 메인 카메라를 Canvas에 재바인딩
        RefreshCanvasCamera();
    }

    /// <summary>
    /// GlobalUIRoot 내의 ScreenSpaceCamera 모드 Canvas들에 현재 씬의 Main Camera를 연결
    /// </summary>
    private void RefreshCanvasCamera()
    {
        if (globalUIRootInstance == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Canvas[] canvases = globalUIRootInstance.GetComponentsInChildren<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas.worldCamera = mainCam;
            }
        }
    }

    private void PrefabReference(GameObject globalUIRoot)
    {
        // 문자열 경로 대신 컴포넌트 타입으로 안전하게 자식 탐색 (비활성화 상태 포함)
        InteractableUI = globalUIRoot.GetComponentInChildren<InteractableUI>(true);

        foreach (Transform child in globalUIRoot.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "MainHUDPanel")
            {
                MainHUDPanel = child.gameObject;
                break;
            }
        }
        //// InventoryUI는 컴포넌트가 따로 없다면 이름으로 찾되 안전하게 검색
        //Transform invTransform = globalUIRoot.transform.Find("GlobalScreenCanvas/InventoryUI");
        //if (invTransform == null)
        //{
        //    // 혹시 Canvas 이름이 변경되었을 경우를 대비한 유연한 탐색
        //    foreach (Transform child in globalUIRoot.GetComponentsInChildren<Transform>(true))
        //    {
        //        if (child.name == "InventoryUI")
        //        {
        //            invTransform = child;
        //            break;
        //        }
        //    }
        //}
        //if (invTransform != null) InventoryUI = invTransform.gameObject;
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

        if (localUIs.TryGetValue(type, out var ui) && ui != null)
        {
            return ui as T;
        }
        // 만약 딕셔너리에 없다면 비활성화 된 오브젝트를 포함하여 씬에서 지연 검색
        T foundUI = FindFirstObjectByType<T>(FindObjectsInactive.Include);
        
        if(foundUI != null)
        {
            RegisterLocalUI(foundUI);
            return foundUI;
        }
        return null;
    }
    #endregion

    #region UI 열기/닫기 및 마우스 커서 제어
    public void SetActiveHUD(bool isActive)
    {
        if (globalUIRootInstance != null)
        {
            globalUIRootInstance.SetActive(isActive);
        }
    }

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
        bool isAnyPopupOpen = openUIStack.Count > 0;

        // 마우스 커서 상태를 UI 열림 여부에 따라 조정
        if (isAnyPopupOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if(MainHUDPanel != null) MainHUDPanel.SetActive(!isAnyPopupOpen);

        // 팝업 창이 열릴 때 상호작용 프롬프트(InteractableUI)도 숨기고 싶다면 주석 해제
         if (InteractableUI != null)
        {
            InteractableUI.gameObject.SetActive(!isAnyPopupOpen);
        }
    }
    #endregion
}