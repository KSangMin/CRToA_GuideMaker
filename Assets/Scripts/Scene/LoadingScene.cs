using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : Scene
{

    private void Start()
    {
        AddressableManager.Instance.StartLoadingAddressable();
    }
}
