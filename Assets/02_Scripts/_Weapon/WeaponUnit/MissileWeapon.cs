using UnityEngine;

// 적을 추적하는 미사일 무기

public class MissileWeapon : WeaponBase
{
    // 미사일 투사체 데이터
    [SerializeField]
    private ProjectileBaseSO _projectileData;

    // 공격력, 속도 등을 계산하기 위한 스탯 프로바이더
    private AttackStatsProvider _stats;
    // 발사 간격을 조절하기 위한 쿨다운 타이머
    private float _cooldownTimer;

    public override void SetStatus()
    {
        base.SetStatus();

        if (_stats == null) _stats = AllManager.instance.GamemodeManager.Player.GetComponent<AttackStatsProvider>();

        _stats.SetTargetWeapon(this);

        if (_projectileData == null) _projectileData = AllManager.instance.ProjectileFactory.GetProjectileData(WeaponType);

        _cooldownTimer = 0f;
    }

    // 매 프레임 호출되어 미사일 발사 타이밍을 제어한다
    public override void Update()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;

        base.Update();

        if (_stats == null) return;
        _stats.SetTargetWeapon(this);

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer += Time.deltaTime;
            if (_stats != null && _cooldownTimer >= _stats.GetAttackCooldown(this))
            {
                _cooldownTimer = 0f;
                Shoot();
            }
        }
        else
        {
            Shoot();
            _cooldownTimer += Time.deltaTime;
        }
    }

    // 미사일 투사체를 실제로 생성하여 발사한다
    public override void Shoot()
    {
        base.Shoot();
        if (_projectileData == null || _stats == null) return;

        float range = _stats.GetAttackRange(this);
        Vector3 facing = GetOwnerFacingDirection();
        TryGetAimDirectionInCone(range, DefaultTargetConeHalfAngleDeg, transform.position, facing, out Vector3 aimDir);

        int count = _stats.GetProjectileCount(this);
        for (int i = 0; i < count; i++)
        {
            var proj = AllManager.instance.ProjectileFactory.Create(_projectileData, transform.position, Quaternion.identity, this) as MissileWeaponProjectile;
            if (proj != null)
            {
                proj.Initialize(aimDir, _stats);
            }
            else
            {
                Debug.LogError("Failed to create MissileWeaponProjectile. Check if the prefab is set correctly in ProjectileFactory.");
            }
        }
    }

    public override void CoolDownReset(float cooldown)
    {
        _cooldownTimer = Mathf.Max(0f, cooldown);
    }
}
