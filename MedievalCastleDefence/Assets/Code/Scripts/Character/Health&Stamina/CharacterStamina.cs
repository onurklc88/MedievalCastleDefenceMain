using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Fusion;

using static BehaviourRegistry;
public class CharacterStamina : CharacterRegistry
{
    public float CurrentStamina { get; set; }
    public bool CanStaminaRegenerating { get; set; }
    private float _totalStamina;
    private const int PLAYER_MAX_JUMP_COUNT = 3;
    private int _playerJumpStamina;
    //[SerializeField] private CharacterStats _characterStats;
    private CharacterAnimationController _characterAnim;
    private CharacterMovement _characterMovement;
    private bool _isRegenerating = false;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _totalStamina = _characterStats.Stamina;
        CurrentStamina = _totalStamina;
        InitScript(this);
        _characterMovement = GetScript<CharacterMovement>();
        _playerJumpStamina = PLAYER_MAX_JUMP_COUNT;
    }

    public void ResetPlayerStamina()
    {
        CurrentStamina = _totalStamina;
    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        switch (_characterStats.WarriorType)
        {
            case CharacterStats.CharacterType.FootKnight:
                _characterAnim = GetScript<FootknightAnimation>();
                break;
            case CharacterStats.CharacterType.Gallowglass:
                _characterAnim = GetScript<GallowglassAnimation>();
                break;
            case CharacterStats.CharacterType.KnightCommander:
                _characterAnim = GetScript<KnightCommanderAnimation>();
                break;
        }
    }

    private void Update()
    {
        if (!Object.HasStateAuthority) return;
        
        if (CanStaminaRegenerating)
        {
            IncreaseCharacterStamina();
        }

       
    }
    public void DecreasePlayerStamina(float value)
    {
        if (!Object.HasStateAuthority) return;
        if(CurrentStamina >= 0)
        {
            CurrentStamina -= value;
            StartCoroutine(UpdateCharacterRegen());
        }
    }
    private void IncreaseCharacterStamina()
    {
        if (!Object.HasStateAuthority) return;
        if(CurrentStamina < _totalStamina)
        {
            CurrentStamina += Time.deltaTime * 20f;
        }
        else
        {
            CanStaminaRegenerating = false;
        }
        
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DecreaseStaminaRPC(float value)
    {
        DecreasePlayerStamina(value);
        if(_characterStats.WarriorType == CharacterStats.CharacterType.FootKnight)
            _characterAnim.UpdateDamageAnimationState();
      
        if (CurrentStamina < _characterStats.KnockbackStaminaLimit)
        {
            _characterAnim.UpdateStunAnimationState(CharacterAttackBehaviour.AttackDirection.Forward);
            _characterMovement.StartCoroutine(_characterMovement.KnockbackPlayer(CharacterAttackBehaviour.AttackDirection.Forward));
        }
        else
        {
            StartCoroutine(DelayBlockAnimation());
        }
    }
    private IEnumerator UpdateCharacterRegen()
    {
        if (CanStaminaRegenerating)
        {
            CanStaminaRegenerating = false;
        }
        yield return new WaitForSeconds(2f);
        CanStaminaRegenerating = true;
        
    }
    public bool CanPlayerJump()
    {
        if (_playerJumpStamina > 0)
        {
            _playerJumpStamina--;
            if(_playerJumpStamina == 0 && !_isRegenerating)
            {
                RegenerateJumps().Forget();
            }
            return true;
        }
        else
            return false;
    }
    private async UniTaskVoid RegenerateJumps()
   {
        _isRegenerating = true;
        while(_playerJumpStamina < PLAYER_MAX_JUMP_COUNT)
        {
            await UniTask.Delay(2000);
            _playerJumpStamina++;
        }

        _isRegenerating = false;
   }

    private IEnumerator DelayBlockAnimation()
    {
        //0.4f
        yield return new WaitForSeconds(0.3f);
        _characterAnim.PlayBlockAnimation();
    }
   

    private void OnDestroy()
    {
        //problemli
        base.OnObjectDestroy();
    }
}
