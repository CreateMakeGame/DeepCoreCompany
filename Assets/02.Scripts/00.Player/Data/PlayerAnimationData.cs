using System;
using UnityEngine;

[Serializable]
public class PlayerAnimationData
{
    
    [SerializeField] private string speedParameterName = "Speed";

    public int SpeedParameterHash { get; private set; }

    public void Initialize()
    {
        SpeedParameterHash = Animator.StringToHash(speedParameterName);

    }
}
