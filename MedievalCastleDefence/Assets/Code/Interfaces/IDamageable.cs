using Fusion;
using UnityEngine;

public interface IDamageable
{
    [Networked] public float NetworkedHealth { get; set; }
    public void DealDamageRPC(float givenDamage, string playerName, CharacterStats.CharacterType playerWarrior);
   
}
