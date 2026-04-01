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
        "Cookie"
        , "Artifact"
        , "Equipment"
        , "Potential"
        , "Thumbnail"
        , "Header"
        , "Card"
        , "Seasonite"
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

    #region 다운로드
    public void StartLoadingAddressable()
    {
        StartCoroutine(InitAddressable());
    }

    IEnumerator InitAddressable()
    {
        //if (Caching.ClearCache())
        //{
        //    Debug.Log("캐시 완전 삭제");
        //}
        //yield return null;

        startLoad?.Invoke();
        Debug.Log("init 로딩 시작");
        AsyncOperationHandle init = Addressables.InitializeAsync();
        init.Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("init 로딩 성공");
                //bool 변수 통해서 에러 체크
            }
        };

        while (!init.IsDone)
        {
            float progress = init.PercentComplete * 100f;
            Debug.Log($"Addressables 초기화 진행 중: {progress:F1}%");

            yield return null;
        }

        startCatalogCheck?.Invoke();

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.IsValid() && checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogsToUpdate = checkHandle.Result;

            if (catalogsToUpdate != null && catalogsToUpdate.Count > 0)
            {
                Debug.Log($"업데이트할 카탈로그 발견: {catalogsToUpdate.Count}개");

                var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate);

                while (!updateHandle.IsDone)
                {
                    Debug.Log($"카탈로그 업데이트 중: {updateHandle.PercentComplete * 100f:F1}%");
                    yield return null;
                }

                if (updateHandle.IsValid())
                {
                    Addressables.Release(updateHandle);
                }

                Debug.Log("카탈로그 업데이트 최종 완료");
            }
            else
            {
                Debug.Log("업데이트할 카탈로그가 없습니다. 최신 상태입니다.");
            }
        }

        if (checkHandle.IsValid())
        {
            Addressables.Release(checkHandle);
        }

        AsyncOperationHandle<long> handle = Addressables.GetDownloadSizeAsync(_defaultLabel);
        yield return handle;
        patchSize = handle.Result;

        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }

        StartCoroutine(PatchFiles());
    }

    IEnumerator PatchFiles()
    {
        startDownload?.Invoke();

        Debug.Log("다운로드 시작");
        if (patchSize > 0)
        {
            patchMap.Add(_defaultLabel.labelString, 0);

            var downloadHandle = Addressables.DownloadDependenciesAsync(_defaultLabel, false);

            while (!downloadHandle.IsDone)
            {
                patchMap[_defaultLabel.labelString] = downloadHandle.GetDownloadStatus().DownloadedBytes;
                yield return null;
            }

            Debug.Log("다운로드 완료");
            patchMap[_defaultLabel.labelString] = downloadHandle.GetDownloadStatus().TotalBytes;

            if (downloadHandle.IsValid())
            {
                Addressables.Release(downloadHandle);
            }
        }

        StartCoroutine(LoadAllCategories());
    }
    #endregion 다운로드

    #region 애셋 로드
    public IEnumerator LoadAllCategories()
    {
        startLoadAsset?.Invoke();
        Debug.Log("스프라이트 카테고리 로드 시작...");

        for (int i = 0; i < _labels.Count; i++)
        {
            string label = _labels[i];
            var handle = Addressables.LoadAssetsAsync<Sprite>(label);
            _handles.Add(handle);

            while (!handle.IsDone)
            {
                float progress = handle.PercentComplete * 100f;
                onLoadProgress?.Invoke(progress);
                Debug.Log($"{label} Asset 로드 진행 중: {progress:F1}%");

                yield return null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Dictionary<string, Sprite> tempDict = new Dictionary<string, Sprite>();
                foreach (var sprite in handle.Result)
                {
                    if (!tempDict.ContainsKey(sprite.name))
                    {
                        tempDict.Add(sprite.name, sprite);
                    }
                }

                _spriteDict[label] = tempDict;
                Debug.Log($"{label} 분류 완료: {tempDict.Count}개");
            }
        }

        Debug.Log("모든 스프라이트 로드 및 분류 완료!");

        var cookiedataHandle = Addressables.LoadAssetsAsync<CookieData>(_cookieDataLabel);
        _handles.Add(cookiedataHandle);

        while (!cookiedataHandle.IsDone)
        {
            float progress = cookiedataHandle.PercentComplete * 100f;
            onLoadProgress?.Invoke(progress);
            Debug.Log($"{_cookieDataLabel} Asset 로드 진행 중: {progress:F1}%");

            yield return null;
        }

        if (cookiedataHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<Coroutine> preloadCoroutines = new List<Coroutine>();

            foreach (var data in cookiedataHandle.Result)
            {
                if (!_cookieDataDict.ContainsKey(data.cookieId))
                {
                    _cookieDataDict.Add(data.cookieId, data);
                    preloadCoroutines.Add(StartCoroutine(data.PreLoadAll()));
                }
            }

            foreach (var coroutine in preloadCoroutines)
            {
                yield return coroutine;
            }

            Debug.Log($"{_cookieDataLabel} 분류 완료: {_cookieDataDict.Count}개");
        }

        var spritedataHandle = Addressables.LoadAssetsAsync<DefaultSpriteData>(_spriteDataLabel);
        _handles.Add(spritedataHandle);

        while (!spritedataHandle.IsDone)
        {
            float progress = spritedataHandle.PercentComplete * 100f;
            onLoadProgress?.Invoke(progress);
            Debug.Log($"{_spriteDataLabel} Asset 로드 진행 중: {progress:F1}%");

            yield return null;
        }

        if (cookiedataHandle.Status == AsyncOperationStatus.Succeeded)
        {
            _spriteData = spritedataHandle.Result.FirstOrDefault();
        }

        yield return StartCoroutine(_spriteData.PreLoadAll());

        Debug.Log("모든 데이터 로드 및 분류 완료!");

        endLoad?.Invoke();
    }

    public IEnumerator LoadAssetsByLabelAsync<T>(string label, Action<List<T>> callback = null) where T : UnityEngine.Object
    {
        Debug.Log($"{label} 라벨 로드 시작...");

        var handle = Addressables.LoadAssetsAsync<T>(label, null);
        _handles.Add(handle);

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // 해당 레이블용 딕셔너리가 없으면 생성
            if (!_labelAssetDict.ContainsKey(label))
            {
                _labelAssetDict[label] = new Dictionary<string, object>();
            }

            foreach (var asset in handle.Result)
            {
                if (!_labelAssetDict[label].ContainsKey(asset.name))
                {
                    _labelAssetDict[label].Add(asset.name, asset);
                }
            }

            callback?.Invoke(new List<T>(handle.Result));
        }
    }

    /// <summary>
    /// 개별 AssetReference를 로드하고 콜백으로 반환합니다.
    /// </summary>
    public void LoadAssetAsync<T>(AssetReference reference, Action<T> callback) where T : UnityEngine.Object
    {
        // 1. 유효하지 않은 참조 체크
        if (reference == null || !reference.RuntimeKeyIsValid())
        {
            Debug.LogWarning("유효하지 않은 어드레서블 참조입니다.");
            return;
        }

        // 2. 이미 로드 완료된 상태라면 결과 즉시 반환 (캐싱 성능 최적화)
        if (reference.OperationHandle.IsValid() && reference.OperationHandle.IsDone)
        {
            callback?.Invoke(reference.OperationHandle.Convert<T>().Result);
            return;
        }

        // 3. 로드 시작
        var handle = reference.LoadAssetAsync<T>();
        _handles.Add(handle); // 매니저가 핸들을 추적하여 나중에 일괄 해제 가능

        handle.Completed += (h) =>
        {
            if (h.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(h.Result);
            }
            else
            {
                Debug.LogError($"에셋 로드 실패: {reference.RuntimeKey}");
            }
        };
    }

    // 기존 GetAllSpriteByLabel을 대체하는 범용 메서드
    public List<T> GetAssetsByLabel<T>(string label) where T : UnityEngine.Object
    {
        if (!_labelAssetDict.TryGetValue(label, out var dict))
        {
            Debug.LogWarning($"{label} 라벨의 에셋이 로드되지 않았습니다.");
            return new List<T>();
        }

        // object 타입을 다시 T 타입으로 캐스팅해서 리스트로 반환
        return dict.Values.Cast<T>().ToList();
    }

    // 개별 에셋 하나만 가져올 때
    public T GetAsset<T>(string label, string assetName) where T : UnityEngine.Object
    {
        if (_labelAssetDict.TryGetValue(label, out var dict))
        {
            if (dict.TryGetValue(assetName, out object obj))
                return obj as T;
        }
        return null;
    }

    public List<Sprite> GetSpritesByLabel(string label)
    {
        if (!_spriteDict.ContainsKey(label))
        {
            Debug.LogWarning($"{label} 라벨 없음");
            return new();
        }

        return _spriteDict[label].Values.ToList();
    }

    public Sprite GetSprite(string label, string spriteName)
    {
        if (_spriteDict.TryGetValue(label, out var dict))
        {
            if (dict.TryGetValue(spriteName, out Sprite s))
                return s;
        }
        Debug.LogWarning($"스프라이트 없음: {label} / {spriteName}");
        return null;
    }

    public List<CookieData> GetAllCookieData()
    {
        return _cookieDataDict.Values.ToList();
    }

    public CookieData GetCookieData(string cookieId)
    {
        if (_cookieDataDict.TryGetValue(cookieId, out var data))
        {
            return data;
        }
        Debug.LogWarning($"쿠키 데이터 없음: {cookieId}");

        return null;
    }

    #endregion 애셋 로드

    private void OnDestroy()
    {
        foreach (var handle in _handles)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
        _spriteDict.Clear();
    }
}
