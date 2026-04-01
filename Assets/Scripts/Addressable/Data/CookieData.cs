using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class SpriteType
{
    public AssetReferenceSprite sprite;
    public ControlType controlType;
}

[CreateAssetMenu(fileName = "CookieData_", menuName = "Scriptable Objects/CookieData")]
public class CookieData : ScriptableObject
{
    [Tooltip("4자리 숫자(0001~9999)")]
    public string cookieId;
    public string cookieName;
    public RarityType rarity;

    [Header("Sprites")]
    public AssetReferenceSprite Icon;
    public ElementalType Type;
    public ClassType Class;
    public AttackType AttackType;

    public List<SpriteType> BasicAttack;
    public List<SpriteType> SpecialAttack;
    public List<SpriteType> Ultimate;
    public List<SpriteType> Dash;

    private Dictionary<AssetReferenceSprite, Sprite> _loadedSprites = new();
    private Dictionary<AssetReferenceSprite, AsyncOperationHandle<Sprite>> _handles = new();

    public IEnumerator PreLoadAll()
    {
        List<AsyncOperationHandle> allHandles = new List<AsyncOperationHandle>();

        void AddLoadOperation(AssetReferenceSprite reference)
        {
            if (reference == null || !reference.RuntimeKeyIsValid()) return;
            if (_loadedSprites.ContainsKey(reference)) return;

            if (!_handles.TryGetValue(reference, out var handle))
            {
                handle = reference.LoadAssetAsync<Sprite>();
                _handles[reference] = handle;

                // 로드 완료 후 캐시에 등록하는 콜백 미리 등록
                handle.Completed += (h) => {
                    if (h.Status == AsyncOperationStatus.Succeeded)
                        _loadedSprites[reference] = h.Result;
                };
            }

            if (!handle.IsDone)
                allHandles.Add(handle);
        }

        // 로드 대상들 일괄 등록 (여기서는 로드가 시작만 됨)
        AddLoadOperation(Icon);
        BasicAttack.ForEach(x => AddLoadOperation(x.sprite));
        SpecialAttack.ForEach(x => AddLoadOperation(x.sprite));
        Ultimate.ForEach(x => AddLoadOperation(x.sprite));
        Dash.ForEach(x => AddLoadOperation(x.sprite));

        // 2. 모든 핸들이 완료될 때까지 대기
        if (allHandles.Count > 0)
        {
            // Addressables.ResourceManager.CreateGenericGroupOperation를 사용하여 묶어서 기다림
            var groupHandle = Addressables.ResourceManager.CreateGenericGroupOperation(allHandles);
            yield return groupHandle;
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

    public List<(Sprite sprite, ControlType controlType)> GetBasicAttackSprites()
    {
        List<(Sprite sprite, ControlType controlType)> sprites = new();
        if (BasicAttack.Count > 0)
        {
            foreach (var sprite in BasicAttack)
            {
                sprites.Add((GetSprite(sprite.sprite), sprite.controlType));
            }
        }
        else
        {
            DefaultSpriteData spriteData = AddressableManager.Instance.SpriteData;
            sprites.Add((spriteData.GetSprite(spriteData.basicAttack_Default), ControlType.Normal));
        }
        return sprites;
    }

    public List<(Sprite sprite, ControlType controlType)> GetSpecialAttackSprites()
    {
        List<(Sprite sprite, ControlType controlType)> sprites = new();
        if (SpecialAttack.Count > 0)
        {
            foreach (var sprite in SpecialAttack)
            {
                sprites.Add((GetSprite(sprite.sprite), sprite.controlType));
            }
        }
        else
        {
            DefaultSpriteData spriteData = AddressableManager.Instance.SpriteData;
            sprites.Add((spriteData.GetSprite(spriteData.specialAttack_Default), ControlType.Normal));
        }
        return sprites;
    }

    public List<(Sprite sprite, ControlType controlType)> GetUltimateSprites()
    {
        List<(Sprite sprite, ControlType controlType)> sprites = new();
        if (Ultimate.Count > 0)
        {
            foreach (var sprite in Ultimate)
            {
                sprites.Add((GetSprite(sprite.sprite), sprite.controlType));
            }
        }
        else
        {
            DefaultSpriteData spriteData = AddressableManager.Instance.SpriteData;
            sprites.Add((spriteData.GetSprite(spriteData.ultimate_Default), ControlType.Normal));
        }
        return sprites;
    }

    public List<(Sprite sprite, ControlType controlType)> GetDashSprites()
    {
        List<(Sprite sprite, ControlType controlType)> sprites = new();
        if (Dash.Count > 0)
        {
            foreach (var sprite in Dash)
            {
                sprites.Add((GetSprite(sprite.sprite), sprite.controlType));
            }
        }
        else
        {
            DefaultSpriteData spriteData = AddressableManager.Instance.SpriteData;
            sprites.Add((spriteData.GetSprite(spriteData.dash_Default), ControlType.Normal));
        }
        return sprites;
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