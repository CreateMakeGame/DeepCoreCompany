using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Settings")]
    [SerializeField] private float interactRange = 3.0f;        // 상호작용 범위
    [SerializeField] private LayerMask interactLayerMask;       // 상호작용 가능한 레이어

    private void Update()
    {
        if (Cursor.visible)
        {
            HideUI();
            return; // UI가 열려 있을 때는 상호작용 체크하지 않음
        }
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
                if (UIManager.Instance != null && UIManager.Instance.InteractableUI != null)
                {
                    string objName = interactable.GetInteractName();
                    string objPrompt = interactable.GetInteractPrompt();

                    UIManager.Instance.InteractableUI.ShowPrompt(interactable.interactionType, objName, objPrompt);
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
        if (UIManager.Instance != null && UIManager.Instance.InteractableUI != null)
        {
            UIManager.Instance.InteractableUI.HidePrompt();
        }
    }
}