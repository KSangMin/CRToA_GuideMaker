using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    public List<AssetReferenceSprite> rarity_frame;
    public List<AssetReferenceSprite> elementalType_frame;
}
