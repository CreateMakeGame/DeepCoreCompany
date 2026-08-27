using UnityEngine;

public class ItemColorChanger : MonoBehaviour
{
    [Header("Color Cycle Settings")]
    [Tooltip("색상이 바뀌는 속도")]
    [Range(0f, 1f)]
    [SerializeField] private float changeSpeed = 0.5f;

    [Tooltip("채도(0: 흰색/회색, 1: 선명함")]
    [Range(0f, 1f)]
    [SerializeField] private float saturation = 0.8f;

    [Tooltip("명도 (0: 검은색, 1: 밝음)")]
    [Range(0f, 1f)]
    [SerializeField] private float brightness = 1.0f;

    private Material targetMaterial;

    private void Awake()
    {
        if (TryGetComponent<Renderer>(out var renderer))
        {
            targetMaterial = renderer.material;
        }
    }

    private void Update()
    {
        if(targetMaterial == null) return;

        // 시간에 따라 0 ~ 1사이를 반복하는 Hue(색상) 값 계산
        float hue = Mathf.Repeat(Time.time * changeSpeed, 1f);

        // HSV 값을 RGB 색상으로 변환
        Color color = Color.HSVToRGB(hue, saturation, brightness);

        // URP Lit 셰이더의 기본 색상 변환
        targetMaterial.color = color;
    }
}

