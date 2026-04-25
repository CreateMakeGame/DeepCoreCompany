using UnityEngine;

public class ShovelIKHandler : MonoBehaviour
{
    [Header("IK Targets")]
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    [Range(0, 1)] public float ikWeight = 1f;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;
        ApplyHandIK(AvatarIKGoal.LeftHand, leftHandTarget);
        ApplyHandIK(AvatarIKGoal.RightHand, rightHandTarget);
    }

    void ApplyHandIK(AvatarIKGoal goal, Transform target)
    {
        if (target == null) return;
        animator.SetIKPositionWeight(goal, ikWeight);
        animator.SetIKRotationWeight(goal, ikWeight);
        animator.SetIKPosition(goal, target.position);
        animator.SetIKRotation(goal, target.rotation);
    }
}
