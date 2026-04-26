using UnityEngine;

//==============================================================================
// PistolWeapon
//==============================================================================
// PlayerMouseDirectionController가 가리키는 방향으로 탄환을 발사하는 기본형
// 권총 무기. 쿨다운에 맞춰 자동으로 발사한다.
//==============================================================================

public class PistolWeapon : WeaponBase
{
    // 발사에 사용될 투사체 데이터
    [SerializeField]
    private ProjectileBaseSO _projectileData;

    // 공격력, 속도 등을 계산하기 위한 스탯 프로바이더
    private AttackStatsProvider _stats;

    // 발사 간격 제어용 쿨다운 타이머
    private float _cooldownTimer;

    // 방향 계산을 위해 사용할 플레이어 마우스 방향 컨트롤러
    private PlayerMouseDirectionController _directionCtrl;

    //---------------------------------------------------------------------------
    // 무기 초기화
    //---------------------------------------------------------------------------
    public override void SetStatus()
    {
        base.SetStatus();

        if (_stats == null) _stats = AllManager.instance.GamemodeManager.Player.GetComponent<AttackStatsProvider>();

        _stats.SetTargetWeapon(this);

        if (_projectileData == null) _projectileData = AllManager.instance.ProjectileFactory.GetProjectileData(WeaponType);

        // 부모 객체에서 방향 컨트롤러를 찾아둔다
        if (_directionCtrl == null && transform.parent != null) _directionCtrl = transform.parent.GetComponent<PlayerMouseDirectionController>();

        _cooldownTimer = 0f;
    }

    //---------------------------------------------------------------------------
    // 매 프레임 호출되는 업데이트 로직
    //---------------------------------------------------------------------------
    public override void Update()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;

        base.Update();

        if (_stats == null) return; 
        _stats.SetTargetWeapon(this);

        float cooldown = _stats.GetAttackCooldown(this);

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer += Time.deltaTime;
            if (_cooldownTimer >= cooldown)
            {
                // 쿨다운이 찼으면 "발사 성공 시"에만 쿨다운을 소모한다
                if (TryShoot())
                {
                    _cooldownTimer = 0f;
                }
                else
                {
                    _cooldownTimer = cooldown; // 계속 대기(ready 상태 유지)
                }
            }
        }
        else
        {
            // 첫 발사는 "타겟이 있을 때만" 수행
            if (TryShoot())
            {
                _cooldownTimer += Time.deltaTime;
            }
        }
    }

    private bool TryShoot()
    {
        if (_projectileData == null || _stats == null) return false;

        float range = _stats.GetAttackRange(this);
        Vector3 facing = GetOwnerFacingDirection();
        if (!TryGetAimDirectionInCone(range, DefaultTargetConeHalfAngleDeg, transform.position, facing, out Vector3 dir))
        {
            return false;
        }

        int count = _stats.GetProjectileCount(this);
        for (int i = 0; i < count; i++)
        {
            var proj = AllManager.instance.ProjectileFactory.Create(_projectileData, transform.position, Quaternion.identity, this) as PistolWeaponProjectile;
            if (proj != null)
            {
                proj.Initialize(dir, _stats);
            }
            else
            {
                Debug.LogError("Failed to create PistolWeaponProjectile. Check if the prefab is set correctly in ProjectileFactory.");
            }
        }

        return true;
    }

    //---------------------------------------------------------------------------
    // 투사체 발사
    //---------------------------------------------------------------------------
    public override void Shoot()
    {
        base.Shoot();
        TryShoot();
    }

    public override void CoolDownReset(float cooldown)
    {
        _cooldownTimer = Mathf.Max(0f, cooldown);
    }
}
