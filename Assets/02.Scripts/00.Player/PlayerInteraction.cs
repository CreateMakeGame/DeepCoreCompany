using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI interactionText;   // 상호작용 UI 텍스트

    [Header("Settings")]
    [SerializeField] private float interactRange = 2.0f;     // 상호작용 범위
    [SerializeField] private LayerMask interactableLayer;       // 상호작용 가능한 레이어

    private void Update()
    {
        CheckForInteractables();
    }

    private void CheckForInteractables()
    {
        if (playerController == null || playerController.CameraTransform == null) return;

        // 기존의 CanDig()와 동일하게 카메라 정중앙에서 레이 발사
        Ray ray = new Ray(playerController.CameraTransform.position, playerController.CameraTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            // 상호작용 가능한 물체인지 태그나 컴포넌트로 확인 (여기서는 태그 예시)
            if (hit.collider.CompareTag("Interactable"))
            {
                // 1. UI 텍스트 표시
                if (interactionText != null)
                {
                    interactionText.text = "줍기 [E]";
                    interactionText.gameObject.SetActive(true);
                }

                // 2. 키 입력 확인 후 상호작용 실행
                if (playerController.IsInteractPressed)
                {
                    PerformInteraction(hit.collider.gameObject);
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

    private void PerformInteraction(GameObject target)
    {
        Debug.Log($"{target.name} 아이템 획득!");
        
        // TODO: 이곳에 인벤토리 획득 시스템 코드 추가 연동 (예: Inventory.Instance.AddItem(target))
        
        Destroy(target); // 우선 월드에서 제거
        HideUI();
    }

    private void HideUI()
    {
        if (interactionText != null && interactionText.gameObject.activeSelf)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}