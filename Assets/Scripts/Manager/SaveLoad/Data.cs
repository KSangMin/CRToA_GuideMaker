//캐릭터, 아이템 등의 초기값 로드 용도
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

#region ItemData

//[Serializable]
//public class ItemData
//{
//    public int id;
//    public string name;
//    public string spriteName;
//    public string description;
//    public int attack;
//    public int defense;
//    public int hp;
//    public int critRate;
//}

//[Serializable]
//public class ItemDataLoader : ILoader<int, ItemData>
//{
//    public List<ItemData> data = new List<ItemData>();

//    public Dictionary<int, ItemData> MakeDict()
//    {
//        Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
//        foreach(ItemData item in data)
//        {
//            dict.Add(item.id, item);
//        }

//        return dict;
//    }
//}

#endregion