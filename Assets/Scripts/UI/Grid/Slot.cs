using UnityEngine;

public class Slot : MonoBehaviour
{
    public void ClearSlot()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
