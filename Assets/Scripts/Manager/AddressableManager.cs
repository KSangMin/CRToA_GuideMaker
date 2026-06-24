using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : Singleton<AddressableManager>
{
    private AssetLabelReference _defaultLabel = new() { labelString = "default" };
    private List<string> _labels = new()
    {
        "Cookie",
        //"Artifact",
        //"Equipment",
        //"Potential",
        //"Thumbnail",
        //"Header",
        //"Card",
        //"Seasonite"
    };

    public List<string> Labels { get { return _labels; } }

    private string _cookieDataLabel = "CookieData";
    private string _spriteDataLabel = "SpriteData";

    public long patchSize = default;
    public Dictionary<string, long> patchMap = new();

    private List<AsyncOperationHandle> _handles = new();
    private Dictionary<string, Dictionary<string, object>> _labelAssetDict = new();
    private Dictionary<string, Dictionary<string, Sprite>> _spriteDict = new();
    private Dictionary<string, CookieData> _cookieDataDict = new();
    private DefaultSpriteData _spriteData;

    public DefaultSpriteData SpriteData { get { return _spriteData; } }

    public Action startLoad;
    public Action startCatalogCheck;
    public Action startDownload;
    public Action startLoadAsset;
    public Action<float> onLoadProgress;
    public Action endLoad;
    public Action<string> loadFailed;

    public void StartLoadingAddressable()
    {
        StartCoroutine(InitAddressable());
    }

    private void FailLoading(string message, AsyncOperationHandle handle = default)
    {
        string exceptionMessage = handle.IsValid() && handle.OperationException != null
            ? $" ({handle.OperationException.Message})"
            : string.Empty;

        Debug.LogError($"{message}{exceptionMessage}");
        loadFailed?.Invoke(message);
    }

    private IEnumerator InitAddressable()
    {
        startLoad?.Invoke();
        Debug.Log("Addressables initialize started.");

        AsyncOperationHandle initHandle = Addressables.InitializeAsync();

        while (!initHandle.IsDone)
        {
            Debug.Log($"Addressables initialize progress: {initHandle.PercentComplete * 100f:F1}%");
            yield return null;
        }

        if (initHandle.IsValid() && initHandle.Status != AsyncOperationStatus.Succeeded)
        {
            FailLoading("Addressables initialize failed.", initHandle);
            yield break;
        }

        Debug.Log("Addressables initialize completed.");
        startCatalogCheck?.Invoke();

        AsyncOperationHandle<List<string>> checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Status != AsyncOperationStatus.Succeeded)
        {
            FailLoading("Addressables catalog check failed.", checkHandle);
            if (checkHandle.IsValid()) Addressables.Release(checkHandle);
            yield break;
        }

        List<string> catalogsToUpdate = checkHandle.Result;

        if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
        {
            Debug.Log($"Addressables catalogs to update: {catalogsToUpdate.Count}");

            var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);

            while (!updateHandle.IsDone)
            {
                Debug.Log($"Addressables catalog update progress: {updateHandle.PercentComplete * 100f:F1}%");
                yield return null;
            }

            if (updateHandle.Status != AsyncOperationStatus.Succeeded)
            {
                FailLoading("Addressables catalog update failed.", updateHandle);
                if (updateHandle.IsValid()) Addressables.Release(updateHandle);
                if (checkHandle.IsValid()) Addressables.Release(checkHandle);
                yield break;
            }

            if (updateHandle.IsValid()) Addressables.Release(updateHandle);
            Debug.Log("Addressables catalog update completed.");
        }
        else
        {
            Debug.Log("Addressables catalogs are already up to date.");
        }

        if (checkHandle.IsValid()) Addressables.Release(checkHandle);

        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(_defaultLabel);
        yield return sizeHandle;

        if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
        {
            FailLoading("Addressables download size check failed.", sizeHandle);
            if (sizeHandle.IsValid()) Addressables.Release(sizeHandle);
            yield break;
        }

        patchSize = Math.Max(0, sizeHandle.Result);

        if (sizeHandle.IsValid()) Addressables.Release(sizeHandle);

        yield return StartCoroutine(PatchFiles());
    }

    private IEnumerator PatchFiles()
    {
        startDownload?.Invoke();
        patchMap.Clear();

        Debug.Log($"Addressables dependency download started. Size: {patchSize} bytes");

        if (patchSize > 0)
        {
            patchMap[_defaultLabel.labelString] = 0;

            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(_defaultLabel, false);

            while (!downloadHandle.IsDone)
            {
                DownloadStatus status = downloadHandle.GetDownloadStatus();
                patchMap[_defaultLabel.labelString] = Math.Min(status.DownloadedBytes, patchSize);
                yield return null;
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                FailLoading("Addressables dependency download failed.", downloadHandle);
                if (downloadHandle.IsValid()) Addressables.Release(downloadHandle);
                yield break;
            }

            patchMap[_defaultLabel.labelString] = patchSize;
            Debug.Log("Addressables dependency download completed.");

            if (downloadHandle.IsValid()) Addressables.Release(downloadHandle);
        }

        yield return StartCoroutine(LoadAllCategories());
    }

    public IEnumerator LoadAllCategories()
    {
        startLoadAsset?.Invoke();
        Debug.Log("Addressables sprite category load started.");

        _spriteDict.Clear();
        _cookieDataDict.Clear();
        _spriteData = null;

        for (int i = 0; i < _labels.Count; i++)
        {
            string label = _labels[i];
            AsyncOperationHandle<IList<Sprite>> handle = Addressables.LoadAssetsAsync<Sprite>(label, null);
            _handles.Add(handle);

            while (!handle.IsDone)
            {
                float progress = ((i + handle.PercentComplete) / _labels.Count) * 100f;
                onLoadProgress?.Invoke(progress);
                Debug.Log($"{label} sprite load progress: {handle.PercentComplete * 100f:F1}%");
                yield return null;
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                FailLoading($"Sprite label load failed: {label}", handle);
                yield break;
            }

            Dictionary<string, Sprite> tempDict = new Dictionary<string, Sprite>();
            foreach (Sprite sprite in handle.Result)
            {
                if (sprite != null && !tempDict.ContainsKey(sprite.name))
                {
                    tempDict.Add(sprite.name, sprite);
                }
            }

            _spriteDict[label] = tempDict;
            Debug.Log($"{label} sprite load completed: {tempDict.Count}");
        }

        AsyncOperationHandle<IList<CookieData>> cookieDataHandle = Addressables.LoadAssetsAsync<CookieData>(_cookieDataLabel, null);
        _handles.Add(cookieDataHandle);

        while (!cookieDataHandle.IsDone)
        {
            onLoadProgress?.Invoke(cookieDataHandle.PercentComplete * 100f);
            Debug.Log($"{_cookieDataLabel} load progress: {cookieDataHandle.PercentComplete * 100f:F1}%");
            yield return null;
        }

        if (cookieDataHandle.Status != AsyncOperationStatus.Succeeded)
        {
            FailLoading($"{_cookieDataLabel} load failed.", cookieDataHandle);
            yield break;
        }

        foreach (CookieData data in cookieDataHandle.Result)
        {
            if (data != null && !_cookieDataDict.ContainsKey(data.cookieId))
            {
                _cookieDataDict.Add(data.cookieId, data);
                yield return StartCoroutine(data.PreLoadAll());

                if (!data.IsPreLoadSucceeded)
                {
                    FailLoading($"CookieData sprite preload failed: {data.cookieId}");
                    yield break;
                }
            }
        }

        Debug.Log($"{_cookieDataLabel} load completed: {_cookieDataDict.Count}");

        AsyncOperationHandle<IList<DefaultSpriteData>> spriteDataHandle = Addressables.LoadAssetsAsync<DefaultSpriteData>(_spriteDataLabel, null);
        _handles.Add(spriteDataHandle);

        while (!spriteDataHandle.IsDone)
        {
            onLoadProgress?.Invoke(spriteDataHandle.PercentComplete * 100f);
            Debug.Log($"{_spriteDataLabel} load progress: {spriteDataHandle.PercentComplete * 100f:F1}%");
            yield return null;
        }

        if (spriteDataHandle.Status != AsyncOperationStatus.Succeeded)
        {
            FailLoading($"{_spriteDataLabel} load failed.", spriteDataHandle);
            yield break;
        }

        _spriteData = spriteDataHandle.Result.FirstOrDefault();

        if (_spriteData == null)
        {
            FailLoading($"{_spriteDataLabel} is empty.");
            yield break;
        }

        yield return StartCoroutine(_spriteData.PreLoadAll());

        if (!_spriteData.IsPreLoadSucceeded)
        {
            FailLoading($"{_spriteDataLabel} sprite preload failed.");
            yield break;
        }

        onLoadProgress?.Invoke(100f);
        Debug.Log("All addressable data loaded.");

        endLoad?.Invoke();
    }

    public IEnumerator LoadAssetsByLabelAsync<T>(string label, Action<List<T>> callback = null) where T : UnityEngine.Object
    {
        Debug.Log($"{label} label load started.");

        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null);
        _handles.Add(handle);

        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"{label} label load failed.");
            callback?.Invoke(new List<T>());
            yield break;
        }

        if (!_labelAssetDict.ContainsKey(label))
        {
            _labelAssetDict[label] = new Dictionary<string, object>();
        }

        foreach (T asset in handle.Result)
        {
            if (asset != null && !_labelAssetDict[label].ContainsKey(asset.name))
            {
                _labelAssetDict[label].Add(asset.name, asset);
            }
        }

        callback?.Invoke(new List<T>(handle.Result));
    }

    public void LoadAssetAsync<T>(AssetReference reference, Action<T> callback) where T : UnityEngine.Object
    {
        if (reference == null || !reference.RuntimeKeyIsValid())
        {
            Debug.LogWarning("Invalid addressable asset reference.");
            callback?.Invoke(null);
            return;
        }

        if (reference.OperationHandle.IsValid() && reference.OperationHandle.IsDone)
        {
            AsyncOperationHandle<T> loadedHandle = reference.OperationHandle.Convert<T>();

            if (loadedHandle.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(loadedHandle.Result);
            }
            else
            {
                Debug.LogError($"Asset was already loaded but failed: {reference.RuntimeKey}");
                callback?.Invoke(null);
            }

            return;
        }

        AsyncOperationHandle<T> handle = reference.LoadAssetAsync<T>();
        _handles.Add(handle);

        handle.Completed += h =>
        {
            if (h.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(h.Result);
            }
            else
            {
                Debug.LogError($"Asset load failed: {reference.RuntimeKey}");
                callback?.Invoke(null);
            }
        };
    }

    public List<T> GetAssetsByLabel<T>(string label) where T : UnityEngine.Object
    {
        if (!_labelAssetDict.TryGetValue(label, out Dictionary<string, object> dict))
        {
            Debug.LogWarning($"{label} label assets are not loaded.");
            return new List<T>();
        }

        return dict.Values.Cast<T>().ToList();
    }

    public T GetAsset<T>(string label, string assetName) where T : UnityEngine.Object
    {
        if (_labelAssetDict.TryGetValue(label, out Dictionary<string, object> dict))
        {
            if (dict.TryGetValue(assetName, out object obj))
            {
                return obj as T;
            }
        }

        return null;
    }

    public List<Sprite> GetSpritesByLabel(string label)
    {
        if (!_spriteDict.ContainsKey(label))
        {
            Debug.LogWarning($"{label} label does not exist.");
            return new List<Sprite>();
        }

        return _spriteDict[label].Values.ToList();
    }

    public Sprite GetSprite(string label, string spriteName)
    {
        if (_spriteDict.TryGetValue(label, out Dictionary<string, Sprite> dict))
        {
            if (dict.TryGetValue(spriteName, out Sprite sprite))
            {
                return sprite;
            }
        }

        Debug.LogWarning($"Sprite not found: {label} / {spriteName}");
        return null;
    }

    public List<CookieData> GetAllCookieData()
    {
        return _cookieDataDict.Values.ToList();
    }

    public CookieData GetCookieData(string cookieId)
    {
        if (_cookieDataDict.TryGetValue(cookieId, out CookieData data))
        {
            return data;
        }

        Debug.LogWarning($"Cookie data not found: {cookieId}");
        return null;
    }

    private void OnDestroy()
    {
        foreach (AsyncOperationHandle handle in _handles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        _spriteDict.Clear();
    }
}
