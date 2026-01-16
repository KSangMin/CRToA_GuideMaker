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
        "Cookie", "Equipment", "Artifact", "Potential", "Seasonite"
    };

    public long patchSize = default;
    public Dictionary<string, long> patchMap = new();
    private List<AsyncOperationHandle> _handles = new();
    private Dictionary<string, Dictionary<string, Sprite>> _spriteDict = new();

    public Action startLoad;
    public Action startCatalogCheck;
    public Action startDownload;
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
            onLoadProgress?.Invoke(progress);
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

                var updateHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);

                while (!updateHandle.IsDone)
                {
                    onLoadProgress?.Invoke(updateHandle.PercentComplete * 100f);
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

        if (patchSize > 0)
        {
            StartCoroutine(PatchFiles());
        }
        else
        {
            endLoad?.Invoke();
        }
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
    }
    #endregion 다운로드

    
}
