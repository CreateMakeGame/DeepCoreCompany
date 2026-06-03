using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : Singleton<Inventory>
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData data;   // 아이템의 데이터 (아이템의 종류, 이름, 설명 등)
        public int quantity;    // 아이템의 수량

        public InventoryItem(ItemData data, int quantity)
        {
            this.data = data;
            this.quantity = quantity;
        }
    }

    [Header("인벤토리 데이터")]
    public List<InventoryItem> items = new List<InventoryItem>();
    public float maxWeight = 100f;      // 최대 무게
    public float currentWeight = 0f;    // 현재 무게
    public int totalValue = 0;          // 총 아이템 수량

    public event Action OnInventoryChanged;  // 인벤토리가 변경될 때 호출되는 이벤트

    protected override void Awake()
    {
        base.Awake();
        // 초기화 작업이 필요한 경우 여기에 작성
    }

    public bool AddItem(ItemData itemData)
    {
        if (currentWeight + itemData.weight > maxWeight)
        {
            Debug.LogWarning($"❌ 무게 초과! ({currentWeight + itemData.weight}kg / {maxWeight}kg)");
            return false;
        }

        InventoryItem existingItem = items.Find(item => item.data.itemID == itemData.itemID);
        if (existingItem != null)
        {
            existingItem.quantity++;
        }
        else
        {
            items.Add(new InventoryItem(itemData, 1));
        }
        UpdateInventoryStats();
        OnInventoryChanged?.Invoke();
        return true;
    }

    private void UpdateInventoryStats()
    {
        currentWeight = 0f;
        totalValue = 0;

        foreach (var item in items)
        {
            currentWeight += item.data.weight * item.quantity;
            totalValue += item.data.baseValue * item.quantity;
        }
    }
}
