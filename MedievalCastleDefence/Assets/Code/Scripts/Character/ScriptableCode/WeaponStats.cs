using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "ScriptableObjects/Weapon", order = 1)]
public class WeaponStats : ScriptableObject
{
    public float TimeBetweenSwings;
    public float Damage;
    public float StaminaWaste;
    public float SkillCooldown;

}
