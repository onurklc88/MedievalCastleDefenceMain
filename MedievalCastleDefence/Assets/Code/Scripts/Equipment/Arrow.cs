using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Arrow : CharacterAttackBehaviour
{
    [Networked] public NetworkString<_16> OwnerNickname { get; set; }
    [Networked] public int OwnerWarriorType { get; set; }
    public bool IsArrowReleased { get; set; }
    [SerializeField] protected Rigidbody _rigidbody;
    [SerializeField] protected WeaponStats _weapon;
    protected bool _isArrowCollided = false;
    protected Transform _parentTransform;
    [SerializeField] protected Transform _interpolationTarget;
    [SerializeField] protected BoxCollider _collison;
    public PlayerStatsController OwnerInfo { get; set; }
    public virtual void InitOwnerStats(PlayerStatsController ownerInfo) { } 
    public override void FixedUpdateNetwork()
    {
        if (!IsArrowReleased) return;

        if (_isArrowCollided)
        {
             return;
        }
        if (_rigidbody.velocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
        }
    }

    protected void UpdateArrowRotation()
    {
        if (_rigidbody.velocity.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(_rigidbody.velocity);
        }
    }

    public void ExecuteShot(Vector3 pos)
    {
        IsArrowReleased = true;
        Vector3 initialForce = pos * 50f + transform.forward + Vector3.up * 1.75f + Vector3.right * 0.5f;
        transform.rotation = Quaternion.LookRotation(initialForce);
        _rigidbody.AddForce(initialForce, ForceMode.Impulse);
    }
    
}

#region Legacy
/*
protected void ArrowCollision(Collider collidedObject)
{
    if (!Object.HasStateAuthority) return;



    if (_isArrowCollided)
    {
        Debug.Log("arrow already collided");
        return;
    }



    _isArrowCollided = true;

    if (!Runner.IsClient) return;

    // Verify we hit something damageable and not our own layer
    if (collidedObject.gameObject.layer == 11) return;

    var damageable = collidedObject.transform.GetComponentInParent<IDamageable>();
    if (damageable == null) return;

    var opponentHealth = damageable as CharacterHealth;
    if (opponentHealth == null)
    {
        Debug.LogError("IDamageable is not CharacterHealth");
        return;
    }

    // Debug.Log($"Dealing damage to {opponentHealth.name} from {OwnerNickname}");

    try
    {
        // opponentHealth.DealDamageRPC(50f, OwnerNickname.ToString(), CharacterStats.CharacterType.Ranger);
        CurrentGamePhase = LevelManager.GamePhase.RoundStart;
        CheckAttackCollision(collidedObject.gameObject);
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Damage failed: {e.Message}");
        return;
    }

    /*
    // Handle arrow sticking
    Transform closestBone = GetClosestBone(collidedObject.transform, transform.position);
    if (closestBone != null)
    {
        transform.SetParent(closestBone);
        _rigidbody.isKinematic = true;
        _collison.enabled = false;
        GetComponent<NetworkRigidbody>().enabled = false;
    }
    else
    {
        StartCoroutine(DestroyArrow());
    }
    */
//StartCoroutine(DestroyArrow());
//}

/*
protected void OnTriggerEnter(Collider collidedObject)
{

   if (_isArrowCollided) return;
   _isArrowCollided = true;
   if (Runner.IsClient)
   {
       if (collidedObject.transform.GetComponentInParent<IDamageable>() != null && collidedObject.gameObject.layer != 11)
       {
           var opponentHealth = collidedObject.transform.GetComponentInParent<CharacterHealth>();
           //opponentHealth.DealDamageRPC(25f);
           _parentTransform = collidedObject.transform;

           Transform closestBone = GetClosestBone(collidedObject.transform, transform.position);
           if (closestBone != null)
           {
               transform.SetParent(closestBone);

               _rigidbody.isKinematic = true;
               _collison.enabled = false;
               var networkRigidbody = transform.GetComponent<NetworkRigidbody>();
               networkRigidbody.enabled = false;

           }

       }
       else
       {
           _rigidbody.isKinematic = true;
           var networkRigidbody = transform.GetComponent<NetworkRigidbody>();
           networkRigidbody.enabled = false;
           if (collidedObject.gameObject.layer != 11)
               StartCoroutine(DestroyArrow());
       }
   }


}

private Transform GetClosestBone(Transform root, Vector3 hitPosition)
{
    Transform closestBone = null;
    float minDistance = Mathf.Infinity;

    foreach (Transform bone in root.GetComponentsInChildren<Transform>())
    {
        float distance = Vector3.Distance(hitPosition, bone.position);
        if (distance < minDistance)
        {
            minDistance = distance;
            closestBone = bone;
        }
    }
    return closestBone;
}
*/
#endregion
