using System;
using UnityEngine;

[Serializable]
public class PlayerAnimationData
{
    
    [SerializeField] private string speedParameterName = "Speed";
    [SerializeField] private string jumpParameterName = "Jump";
    [SerializeField] private string digParameterName = "Dig";

    public int SpeedParameterHash { get; private set; }
    public int JumpParameterHash { get; private set; }
    public int DigParameterHash { get; private set; }

    public void Initialize()
    {
        SpeedParameterHash = Animator.StringToHash(speedParameterName);
        JumpParameterHash = Animator.StringToHash(jumpParameterName);
        DigParameterHash = Animator.StringToHash(digParameterName);
    }
}
