using UnityEngine;

// 벽에 반사되는 레이저 무기

public class LaserWeapon : WeaponBase
{
    // 레이저 투사체 데이터
    [SerializeField]
    private ProjectileBaseSO _projectileData;

    // 스탯 계산용 프로바이더
    private AttackStatsProvider _stats;
    // 공격 쿨타임 타이머
    private float _cooldownTimer;

    // 무기 사용을 위한 기본 설정
    public override void SetStatus()
    {
        base.SetStatus();

        if (_stats == null) _stats = AllManager.instance.GamemodeManager.Player.GetComponent<AttackStatsProvider>();

        _stats.SetTargetWeapon(this);

        if (_projectileData == null) _projectileData = AllManager.instance.ProjectileFactory.GetProjectileData(WeaponType);

        _cooldownTimer = 0f;
    }

    // 매 프레임 호출되어 레이저 발사를 관리한다
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
                if (TryShoot())
                {
                    _cooldownTimer = 0f;
                }
                else
                {
                    _cooldownTimer = cooldown;
                }
            }
        }
        else
        {
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
        if (!TryGetAimDirectionInCone(range, DefaultTargetConeHalfAngleDeg, transform.position, facing, out Vector3 aimDir))
        {
            return false;
        }

        int count = _stats.GetProjectileCount(this);
        for (int i = 0; i < count; i++)
        {
            var proj = AllManager.instance.ProjectileFactory.Create(_projectileData, transform.position, Quaternion.identity, this) as LaserWeaponProjectile;
            if (proj != null)
            {
                proj.Initialize(aimDir, _stats);
            }
            else
            {
                Debug.LogError("Failed to create LaserWeaponProjectile. Check if the prefab is set correctly in ProjectileFactory.");
            }
        }

        return true;
    }

    // 레이저 투사체를 발사한다
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
