using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    protected override void Awake()
    {
        base.Awake();

    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<Loader>(textAsset.text);
    }
}
