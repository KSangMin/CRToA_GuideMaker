using UnityEngine;
using UnityEngine.EventSystems;
public class Scene : MonoBehaviour
{
    protected void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        UIManager.Instance.Clear();
        if(FindObjectOfType<EventSystem>() == null) Util.InstantiatePrefab("UI/EventSystem");
    }

    public virtual void Clear()
    {

    }
}
