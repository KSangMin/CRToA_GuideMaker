//캐릭터 정보 세이브 로드 용도
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[Serializable]
public abstract class ISaveLoader<Key, Value>
{
    public abstract Dictionary<Key, Value> MakeDict();
    public List<Value> data = new List<Value>();
}
