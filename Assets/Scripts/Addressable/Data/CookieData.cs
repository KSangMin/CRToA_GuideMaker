using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
}