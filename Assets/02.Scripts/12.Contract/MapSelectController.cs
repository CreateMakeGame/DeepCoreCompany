using System;
using UnityEngine;

public class MapSelectController : MonoBehaviour
{
    [Header("UI Reference")]
    public CardUI leftCardUI;
    public CardUI rightCardUI;

    [Header("Map Data Assets")]
    public MapDataSO leftMapData;
    public MapDataSO rightMapData;

    private void Start()
    {
        InintCards();
    }

    private void InintCards()
    {
        if (leftCardUI != null && leftMapData != null)
        {
            leftCardUI.Setup(leftMapData);
        }

        if (rightCardUI != null && rightMapData != null)
        {
            rightCardUI.Setup(rightMapData);
        }
    }
}
