using UnityEngine;




[CreateAssetMenu(fileName = "CharacterStats", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterStats : ScriptableObject
{
   public enum CharacterType
   {
        None,
        FootKnight,
        KnightCommander,
        Gallowglass,
        Ranger
   }
   public CharacterType WarriorType;
   public float MoveSpeed;
   public float SprintSpeed;    
   public float TotalHealth;
   public float AttackStamina;
   public float DefenceStamina;
   public float KnockbackStaminaLimit;
   public float KnockBackDuration;
}
