using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Fusion;


using static BehaviourRegistry;
public class CharacterStamina : CharacterRegistry
{
    public float CurrentAttackStamina { get; set; }
    public float CurrentDefenceStamina { get; private set; }
    public bool CanAttackStaminaRegenerating { get; private set; }
    public bool CanDefenceStaminaRegenerating { get; private set; }
    private float _totalAttackStamina;
    private float _totalDefenceStamina;
    private const int PLAYER_MAX_JUMP_COUNT = 3;
    private int _playerJumpStamina;
   
    private CharacterAnimationController _characterAnim;
    private CharacterMovement _characterMovement;
    private PlayerVFXSytem _playerVFX;
    private CharacterAttackBehaviour _characterAttack;
    private CharacterCameraController _characterCameraController;
    private bool _isRegenerating = false;
    public override void Spawned()
    {
        if (!Object.HasStateAuthority) return;
        _totalAttackStamina = _characterStats.AttackStamina;
        _totalDefenceStamina = _characterStats.DefenceStamina;
        CurrentAttackStamina = _totalAttackStamina;
        CurrentDefenceStamina = _totalDefenceStamina;
        InitScript(this);
        _characterMovement = GetScript<CharacterMovement>();
        _playerJumpStamina = PLAYER_MAX_JUMP_COUNT;
    }

    public void ResetPlayerStamina()
    {
        CurrentAttackStamina = _totalAttackStamina;
    }

    private void Start()
    {
        if (!Object.HasStateAuthority) return;
        switch (_characterStats.WarriorType)
        {
            case CharacterStats.CharacterType.FootKnight:
                _characterAnim = GetScript<FootknightAnimation>();
                _playerVFX = GetScript<StormshieldVFXController>();
                _characterAttack = GetScript<FootKnightAttack>();
                break;
            case CharacterStats.CharacterType.Gallowglass:
                _characterAnim = GetScript<GallowglassAnimation>();
                _playerVFX = GetScript<BloodhandVFXController>();
                _characterAttack = GetScript<GallowglassAttack>();
                break;
            case CharacterStats.CharacterType.KnightCommander:
                _playerVFX = GetScript<IronheartVFXController>();
                _characterAnim = GetScript<KnightCommanderAnimation>();
                _characterAttack = GetScript<KnightCommanderAttack>();
                break;
            case CharacterStats.CharacterType.Ranger:
                _playerVFX = GetScript<RangerVFXController>();
                _characterAnim = GetScript<RangerAnimation>();
                _characterAttack = GetScript<ArcheryAttack>();
                break;
                
        }
        _characterCameraController = GetScript<CharacterCameraController>();
    }

    private void Update()
    {
        if (!Object.HasStateAuthority) return;
        
        if (CanAttackStaminaRegenerating)
        {
            IncreaseCharacterAttackStamina();
        }

        if (CanDefenceStaminaRegenerating)
        {
            IncreaseCharacterDefenceStamina();
        }
    }
    public void DecreaseCharacterAttackStamina(float value)
    {
        if (!Object.HasStateAuthority) return;
        if(CurrentAttackStamina >= 0)
        {
            CurrentAttackStamina -= value;
            StartCoroutine(RegenerateAttackStamina());
        }

        
    }
    private void IncreaseCharacterAttackStamina()
    {
        if (!Object.HasStateAuthority) return;
        if(CurrentAttackStamina < _totalAttackStamina)
        {
            CurrentAttackStamina += Time.deltaTime * 20f;
        }
        else
        {
            CanAttackStaminaRegenerating = false;
        }
        
    }

    private void IncreaseCharacterDefenceStamina()
    {
        if (!Object.HasStateAuthority) return;
        if (CurrentDefenceStamina < _totalDefenceStamina)
        {
            CurrentDefenceStamina += Time.deltaTime * 20f;
        }
        else
        {
            CanDefenceStaminaRegenerating = false;
        }

    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DecreaseDefenceStaminaRPC(float value)
    {
        //DecreaseCharacterAttackStamina(value);
        if (CurrentDefenceStamina >= 0)
        {
            CurrentDefenceStamina -= value;
            StartCoroutine(RegenerateDefenceStamina());
        }
        if (_characterStats.WarriorType == CharacterStats.CharacterType.FootKnight)
            _characterAnim.UpdateDamageAnimationState(); 
       Debug.Log("CurrentDefenceStamina: " + CurrentDefenceStamina);
      
        if (CurrentDefenceStamina <= _characterStats.KnockbackStaminaLimit)
        {
            //_characterAnim.UpdateStunAnimationState(3);
            StunPlayerRpc(3);
        }
        else
        {
            StartCoroutine(DelayBlockAnimation());
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public async void StunPlayerRpc(int stunDuration)
    {
        if (_characterMovement.IsInputDisabled) return;
        EventLibrary.OnImpulseRequested?.Invoke(1, 0.2f);
       _characterAttack.InterruptEnemyAction();
        _characterAttack.IsPlayerBlockingLocal = false;
        _characterMovement.IsInputDisabled = true;
        _playerVFX.PlayDisabledVFXRpc();
        _characterAnim.UpdateStunAnimationState(stunDuration * 1000);
        await UniTask.Delay(stunDuration * 1000);
        _characterMovement.IsInputDisabled = false;
    }

    private IEnumerator RegenerateAttackStamina()
    {
        if (CanAttackStaminaRegenerating)
        {
            CanAttackStaminaRegenerating = false;
        }
        yield return new WaitForSeconds(2f);
        CanAttackStaminaRegenerating = true;
        
    }

    private IEnumerator RegenerateDefenceStamina()
    {
        if (CanDefenceStaminaRegenerating)
        {
            CanDefenceStaminaRegenerating = false;
        }
        yield return new WaitForSeconds(10f);
        CanDefenceStaminaRegenerating = true;

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
        yield return new WaitForSeconds(0.1f);
        _characterAnim.PlayBlockAnimation();
    }
   

    private void OnDestroy()
    {
        //problemli
        base.OnObjectDestroy();
    }
}
