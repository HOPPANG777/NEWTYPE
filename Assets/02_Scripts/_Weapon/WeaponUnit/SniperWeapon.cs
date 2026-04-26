using UnityEngine;

//==============================================================================
// SniperWeapon
//==============================================================================
// 마우스가 가리키는 방향으로 강력한 탄환을 발사하는 스나이퍼 무기.
// 발사 시 반동으로 플레이어가 뒤로 밀려난다. 나머지 동작은 PistolWeapon과 유사하다.
//==============================================================================

public class SniperWeapon : WeaponBase
{
    // 발사에 사용될 투사체 데이터
    [SerializeField]
    private ProjectileBaseSO _projectileData;

    // 공격 스탯 계산용 프로바이더
    private AttackStatsProvider _stats;

    // 발사 간격 관리를 위한 쿨다운 타이머
    private float _cooldownTimer;

    // 플레이어 방향 계산용 컨트롤러
    private PlayerMouseDirectionController _directionCtrl;

    // 발사 반동 거리
    [SerializeField]
    private float _recoilDistance = 0.5f;

    // 투사체 경로 미리보기 오브젝트
    private GameObject _pathPreviewObj;
    private IProjectilePathPreviewer _pathPreview;

    //--------------------------------------------------------------------------
    // 무기 초기 설정
    //--------------------------------------------------------------------------
    public override void SetStatus()
    {
        base.SetStatus();

        if (_stats == null) _stats = AllManager.instance.GamemodeManager.Player.GetComponent<AttackStatsProvider>();

        _stats.SetTargetWeapon(this);

        if (_projectileData == null) _projectileData = AllManager.instance.ProjectileFactory.GetProjectileData(WeaponType);

        if (_directionCtrl == null && transform.parent != null) _directionCtrl = transform.parent.GetComponent<PlayerMouseDirectionController>();

        _cooldownTimer = 0f;

        // 경로 미리보기 초기화 및 표시
        if (_pathPreviewObj == null)
        {
            _pathPreviewObj = AllManager.instance.ParticleFactory.Create(ParticleType.PARTICLE_SNIPER_PATH,
                transform.position, Quaternion.identity);
        }
        if (_pathPreviewObj != null)
        {
            _pathPreview = _pathPreviewObj.GetComponent<SniperPathPreview>() as IProjectilePathPreviewer;
            if (_pathPreview != null)
            {
                _pathPreview.Initialize(transform);
                float range = _stats != null ? _stats.GetAttackRange(this) : 0f;
                _pathPreview.UpdatePath(transform.right, range);
                _pathPreview.Show();
            }
        }
    }

    //--------------------------------------------------------------------------
    // 매 프레임 호출되는 업데이트 로직
    //--------------------------------------------------------------------------
    public override void Update()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;

        base.Update();

        if (_stats == null) return;
        _stats.SetTargetWeapon(this);

        // 현재 방향을 이용해 미리보기 갱신
        Vector3 previewDir = transform.right;
        if (_directionCtrl != null)
        {
            _directionCtrl.Rotate();
            if (_directionCtrl.RotationDirection.sqrMagnitude > 0f)
            {
                previewDir = _directionCtrl.RotationDirection;
            }
        }
        if (_pathPreview != null)
        {
            float range = _stats.GetAttackRange(this);
            _pathPreview.UpdatePath(previewDir, range);
        }

        float cooldown = _stats.GetAttackCooldown(this);
        if (_cooldownTimer == 0f && _pathPreview != null)
        {
            _pathPreview.Show();
            _pathPreview.SetChargeProgress(0f);
        }

        _cooldownTimer += Time.deltaTime;
        if (_pathPreview != null)
        {
            float ratio = Mathf.Clamp01(_cooldownTimer / cooldown);
            _pathPreview.SetChargeProgress(ratio);
        }

        if (_cooldownTimer >= cooldown)
        {
            if (TryShoot())
            {
                _cooldownTimer = 0f;
                if (_pathPreview != null) _pathPreview.Hide();
            }
            else
            {
                // 타겟이 없으면 쿨다운은 "찬 상태"로 유지하고 대기한다
                _cooldownTimer = cooldown;
            }
        }
    }

    //--------------------------------------------------------------------------
    // 투사체 발사 및 반동 적용
    //--------------------------------------------------------------------------
    public override void Shoot()
    {
        base.Shoot();
        TryShoot();
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
            var proj = AllManager.instance.ProjectileFactory.Create(_projectileData, this.transform.position, Quaternion.identity, this) as SniperWeaponProjectile;
            if (proj != null)
            {
                proj.Initialize(dir, _stats);
            }
            else
            {
                Debug.LogError("Failed to create SniperWeaponProjectile. Check if the prefab is set correctly in ProjectileFactory.");
            }
        }

        if (this.transform.parent != null)
        {
            this.transform.parent.position -= dir.normalized * _recoilDistance;
        }

        return true;
    }

    public override void CoolDownReset(float cooldown)
    {
        // 스나이퍼는 "게이지가 다 찼을 때만 발사" 구조라서,
        // 리셋 시 즉시 발사 가능 상태로 만들어준다.
        float ready = _stats != null ? _stats.GetAttackCooldown(this) : 0f;
        _cooldownTimer = Mathf.Max(ready, cooldown);

        if (_pathPreview != null) _pathPreview.Hide();
    }
}
