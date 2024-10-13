using Fusion;
using UnityEngine;

public interface IDamageable
{
    [Networked] public float NetworkedHealth { get; set; }
    public void DealDamageRPC(float givenDamage);
    public void DestroyObject();
}
