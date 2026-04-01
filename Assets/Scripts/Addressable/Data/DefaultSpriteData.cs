using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "DefaultSpriteData", menuName = "Scriptable Objects/DefaultSpriteData")]
public class DefaultSpriteData : ScriptableObject
{
    [Header("Default Sprites (Shared by all cookies)")]
    public AssetReferenceSprite basicAttack_Default;
    public AssetReferenceSprite specialAttack_Default;
    public AssetReferenceSprite ultimate_Default;
    public AssetReferenceSprite dash_Default;

    public List<AssetReferenceSprite> attackType;
    public List<AssetReferenceSprite> classType;
    public List<AssetReferenceSprite> elementalType;
    public List<AssetReferenceSprite> elementalType_frame;
    public List<AssetReferenceSprite> rarity_frame;

    private Dictionary<AssetReferenceSprite, Sprite> _loadedSprites = new();
    private Dictionary<AssetReferenceSprite, AsyncOperationHandle<Sprite>> _handles = new();

    public IEnumerator PreLoadAll()
    {
        yield return LoadSprite(basicAttack_Default);
        yield return LoadSprite(specialAttack_Default);
        yield return LoadSprite(ultimate_Default);
        yield return LoadSprite(dash_Default);

        foreach (var attr in attackType)
        {
            yield return LoadSprite(attr);
        }
        foreach (var attr in classType)
        {
            yield return LoadSprite(attr);
        }
        foreach (var attr in elementalType)
        {
            yield return LoadSprite(attr);
        }
        foreach (var attr in elementalType_frame)
        {
            yield return LoadSprite(attr);
        }
        foreach (var attr in rarity_frame)
        {
            yield return LoadSprite(attr);
        }
    }

    /// <summary>
    /// 스프라이트를 안전하게 가져오는 코루틴
    /// </summary>
    /// <param name="reference">대상 레퍼런스</param>
    /// <param name="callback">로드 완료 후 실행할 액션</param>
    public IEnumerator LoadSprite(AssetReferenceSprite reference, System.Action<Sprite> callback = null)
    {
        if (reference == null || !reference.RuntimeKeyIsValid())
        {
            callback?.Invoke(null);
            yield break;
        }

        // [STEP 1] 이미 로드가 완료되어 캐시에 있는지 확인
        if (_loadedSprites.TryGetValue(reference, out Sprite cachedSprite))
        {
            callback?.Invoke(cachedSprite);
            yield break; // 코루틴 즉시 종료
        }

        // [STEP 2] 캐시에는 없지만, 현재 로드 중인지 확인
        if (_handles.TryGetValue(reference, out AsyncOperationHandle<Sprite> handle))
        {
            // 이미 누군가 로드 요청을 했으므로, 그 핸들이 끝날 때까지만 대기
            if (!handle.IsDone) yield return handle;
        }
        else
        {
            // [STEP 3] 아무도 로드한 적이 없으므로 새로 로드 시작
            handle = reference.LoadAssetAsync<Sprite>();
            _handles[reference] = handle; // 핸들 등록

            yield return handle;

            // 로드 완료 직후 결과물을 _spriteCache에 저장!
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _loadedSprites[reference] = handle.Result;
            }
        }

        // [STEP 4] 최종 결과 반환
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            callback?.Invoke(handle.Result);
        }
        else
        {
            Debug.LogError($"Load Failed: {reference.RuntimeKey}");
            callback?.Invoke(null);
        }
    }

    public Sprite GetSprite(AssetReferenceSprite reference)
    {
        if (_loadedSprites.ContainsKey(reference))
        {
            return _loadedSprites[reference];
        }

        Debug.LogWarning($"Sprite not loaded yet: {reference.RuntimeKey}");

        return null;
    }

    // 메모리 해제 시 두 딕셔너리 모두 정리
    public void ClearAll()
    {
        foreach (var handle in _handles.Values)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
        _handles.Clear();
        _loadedSprites.Clear();
    }
}
