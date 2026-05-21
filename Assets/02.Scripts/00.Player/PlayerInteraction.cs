using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

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
            ItemObject itemObj = hit.collider.GetComponent<ItemObject>();

            if (itemObj != null)
            {
                // 1. UI 텍스트 표시 (아이템 이름과 무게를 동적으로 출력)
                if (interactionText != null)
                {
                    interactionText.text = itemObj.GetInteractionText();
                    interactionText.gameObject.SetActive(true);
                }

                // 2. E 키 입력 시 상호작용
                if (playerController.IsInteractPressed)
                {
                    PerformInteraction(itemObj);
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

    private void PerformInteraction(ItemObject itemObj)
    {
        ItemData data = itemObj.Data;
        Debug.Log($"[인벤토리 예정] {data.itemName} 획득! 가치: {data.baseValue}, 소속: {data.companyType}");
        // TODO: 이곳에 인벤토리 획득 시스템 코드 추가 연동 (예: Inventory.Instance.AddItem(itemObj.Data))

        Destroy(itemObj.gameObject); // 우선 월드에서 제거
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