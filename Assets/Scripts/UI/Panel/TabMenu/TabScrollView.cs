using System;
using System.Collections.Generic;
using UnityEngine;

public class TabScrollView : MonoBehaviour
{
    [SerializeField] private Transform content;
    [SerializeField] private GameObject tabSlotPrefab;

    public void Init(string label)
    {
        List<Sprite> sprites = AddressableManager.Instance.GetAllSpriteByLabel(label);

        if (!Enum.TryParse(label, out IconType iconType))
        {
            Debug.LogError($"TabScrollView Init Error: Unable to parse IconType from label '{label}'");
            return;
        }

        for (int i = 0; i < sprites.Count; i++)
        {
            GameObject slotObj = Instantiate(tabSlotPrefab, transform);
            slotObj.transform.SetParent(content);
            TabSlot slot = slotObj.GetComponent<TabSlot>();
            slot.SetSlot(iconType, i, sprites[i]);
        }
    }
}
