using UnityEngine;

[CreateAssetMenu(fileName = "Bomb", menuName = "ScriptableObjects/Throwable", order = 1)]

public class ThrowableProperties : ScriptableObject
{
    public float AOEWidth;
    public float Damage;
}
