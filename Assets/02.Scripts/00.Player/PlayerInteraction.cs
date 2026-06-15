using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI interactionText;   // 상호작용 UI 텍스트

    [Header("Settings")]
    [SerializeField] private float interactRange = 3.0f;        // 상호작용 범위
    [SerializeField] private LayerMask interactLayerMask;       // 상호작용 가능한 레이어

    private void Start()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false); // 시작 시 UI 숨김
        }
        else
        {
            Debug.LogWarning("상호작용 UI 텍스트가 연결되지 않았습니다!");
        }
    }

    private void Update()
    {
        CheckForInteractables();
    }

    private void CheckForInteractables()
    {
        if (playerController == null || playerController.CameraTransform == null) return;

        // 기존의 CanDig()와 동일하게 카메라 정중앙에서 레이 발사
        Ray ray = new Ray(playerController.CameraTransform.position, playerController.CameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactLayerMask))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                // 1. UI 텍스트 표시 (아이템 이름과 무게를 동적으로 출력)
                if (interactionText != null)
                {
                    string objName = interactable.GetInteractName();
                    string objPrompt = interactable.GetInteractPrompt();

                    // 예시 출력 형태: "철광석\n줍기 (E) - 5.5kg"
                    interactionText.text = $"{objName}\n{objPrompt}";
                    interactionText.gameObject.SetActive(true);
                }
                // 2. E 키 입력 시 상호작용
                if (playerController.IsInteractPressed)
                {
                    interactable.Interact(gameObject);
                    HideUI();
                }
            }
            else
            {
                HideUI();
            }
        }
        else
        {
            HideUI();
        }
    }

    private void HideUI()
    {
        if (interactionText != null && interactionText.gameObject.activeSelf)
        {
            // 상호작용 버튼 입력 직후 텍스트가 깜빡이는 것을 방지하기 위해 
            // 프레임 종료 시점에 텍스트를 비워주는 것이 안전
            interactionText.text = string.Empty;
            interactionText.gameObject.SetActive(false);
        }
    }
}