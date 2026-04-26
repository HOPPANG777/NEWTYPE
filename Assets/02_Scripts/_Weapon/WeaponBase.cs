using NUnit.Framework;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponBase : MonoBehaviour, ITeamMember, ISpeakable, ISetStatus, ICollidable, IDead, IWeaponAttribute<WeaponAttribute>, IWeaponDisableMember, IWeaponLevel, IWeaponAttackFunction, IWeaponMember
{
    protected const float DefaultTargetConeHalfAngleDeg = 60f; // 120도
    [TabGroup("Weapon Base")][Tooltip("Weapon Data")][SerializeField] protected WeaponBaseSO _weaponBaseData;
    public WeaponBaseSO WeaponBaseData { get { return _weaponBaseData; } set { _weaponBaseData = value; } }

    [TabGroup("Weapon Base")][Tooltip("무기 이름")][SerializeField] protected string _weaponName;
    public string WeaponName { get { return _weaponName; } set { _weaponName = value; } }

    [TabGroup("Weapon Base")][Tooltip("팀 타입")][SerializeField] protected TeamType _teamType;
    public TeamType TeamType { get { return _teamType; } set { _teamType = value; } }

    [TabGroup("Unit Base")][Tooltip("무기 비활성화 타입")][SerializeField] protected WeaponDisableType _weaponDisableType;
    public WeaponDisableType WeaponDisableType { get { return _weaponDisableType; } set { _weaponDisableType = value; } }

    [TabGroup("Weapon Base")][Tooltip("무기 능력치")][SerializeField] protected WeaponAttribute _weaponAttribute;
    public WeaponAttribute Attributes { get { return _weaponAttribute; } set { _weaponAttribute = value; } }

    [TabGroup("Weapon Base")][Tooltip("무기 타입")][SerializeField] protected WeaponType _weaponType;
    public WeaponType WeaponType { get { return _weaponType; } set { _weaponType = value; } }

    [Header("Weapon Sound")]
    // 사용할 사운드 이름
    [TabGroup("Weapon Sound")][Tooltip("사운드 이름")][SerializeField] protected string _soundName;
    public string SoundName { get { return _soundName; } }

    // 사용할 사운드 타입
    [TabGroup("Weapon Sound")][Tooltip("사운드 타입")][SerializeField] protected SoundType _soundType;
    public SoundType SoundType { get { return _soundType; } }

    // 사용할 오디오 클립
    [TabGroup("Weapon Sound")][Tooltip("오디오 클립")][SerializeField] protected AudioClip _soundClip;
    public AudioClip SoundClip { get { return _soundClip; } }

    // 오디오를 출력할 주체
    [TabGroup("Weapon Sound")][Tooltip("오디오를 출력할 주체")][SerializeField] protected AudioSource _soundSource;
    public AudioSource SoundSource { get { return _soundSource; } }

    [Header("ICollidable")]
    [TabGroup("ICollidable")][Tooltip("객체의 포지션을 받는")][Serialize] protected Vector3 _position => this.transform.position;
    public Vector3 Position { get { return _position; } }

    [TabGroup("ICollidable")][Tooltip("객체의 포지션을 받는(그리고 100 분율로 보십시오 따라서 50일 0.5f가 됩니다.)")][SerializeField] protected float _collisionRadius;
    public float CollisionRadius { get { return _collisionRadius; } }

    [TabGroup("Weapon Dead")][Tooltip("죽었는지 확인")][SerializeField] private bool isDead = false;
   
    public bool IsDead { get { return isDead; } }

    public int CurrentLevel { get; set; }

    public int MaxLevel => _weaponBaseData.MaxLevel;

    public virtual void Update()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;
    }

    public virtual void Shoot()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;
    }

    public virtual void Melee()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;
    }

    public virtual void ReleaseProjectile()
    {
        
    }

    public bool LevelUp()
    {
        if (CurrentLevel >= MaxLevel - 1) return false;
        CurrentLevel++;
        ApplyLevelAttributes();
        return true;
    }

    public void ApplyLevelAttributes()
    {
        // 레벨에 맞는 속성표에서 가져와서 적용
        int idx = Mathf.Clamp(CurrentLevel, 0, _weaponBaseData.LevelAttributes.Count - 1);
        _weaponAttribute = _weaponBaseData.LevelAttributes[idx];
    }

    public virtual void SoundPlay()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;
        if (_soundSource != null && _soundClip != null)
        {
            _soundSource.clip = _soundClip;
            _soundSource.Play();
        }
    }

    public virtual void SoundStop()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;   
        if (_soundSource != null && _soundSource.isPlaying)
        {
            _soundSource.Stop();
        }
    }

    public virtual void SetStatus()
    {
        _weaponBaseData.SetData(WeaponBaseData);
        _weaponAttribute = _weaponBaseData.LevelAttributes[Mathf.Clamp(CurrentLevel, 0, _weaponBaseData.LevelAttributes.Count - 1)];
        _weaponName = _weaponBaseData.WeaponName;
        
        _collisionRadius = _weaponBaseData.CollisionRadius;
        
        _teamType = _weaponBaseData.TeamType;
        _weaponType = _weaponBaseData.WeaponType;
        _weaponDisableType = _weaponBaseData.WeaponDisableType;
        
        _soundName = _weaponBaseData.SoundName;
        _soundType = _weaponBaseData.SoundType;
        _soundClip = _weaponBaseData.SoundClip;
        _soundSource = this.gameObject.GetComponent<AudioSource>();
        _soundSource.clip = _soundClip;

        AllManager.instance.CollisionManager.RegisterCollidable(this);
        SoundManager.instance.RegisterSound(this.SoundName, this.SoundClip, this.SoundType);
        isDead = false;
    }

    public virtual void CheckDead()
    {
        if (AllManager.instance.GamemodeManager.GameMode == GameModeType.GAMEMODE_GAMECLEAR || AllManager.instance.GamemodeManager.GameMode == GameModeType.GAMEMODE_GAMEOVER)
        {
            Dead();
        }
    }

    public virtual void Dead()
    {
        isDead = true;
        AllManager.instance.CollisionManager.UnregisterCollidable(this);
        gameObject.SetActive(false);
    }

    protected Vector3 GetOwnerFacingDirection()
    {
        Transform owner = transform.parent != null ? transform.parent : transform;
        var dirCtrl = owner.GetComponent<PlayerMouseDirectionController>();
        if (dirCtrl != null && dirCtrl.RotationDirection.sqrMagnitude > 1e-6f)
        {
            return dirCtrl.RotationDirection.normalized;
        }

        Vector3 right = owner.right;
        if (right.sqrMagnitude > 1e-6f) return right.normalized;
        return Vector3.right;
    }

    protected bool TryGetAimDirectionInCone(float range, float halfAngleDeg, Vector3 origin, Vector3 facingDir, out Vector3 aimDir)
    {
        aimDir = Vector3.zero;
        if (TryFindClosestEnemyInCone(range, halfAngleDeg, origin, facingDir, out var target) && target != null)
        {
            Vector3 delta = target.transform.position - origin;
            if (delta.sqrMagnitude > 1e-6f)
            {
                aimDir = delta.normalized;
                return true;
            }
        }

        if (facingDir.sqrMagnitude > 1e-6f)
        {
            aimDir = facingDir.normalized;
            return false;
        }

        aimDir = Vector3.right;
        return false;
    }

    protected bool TryFindClosestEnemyInCone(float range, float halfAngleDeg, Vector3 origin, Vector3 facingDir, out UnitBase target)
    {
        target = null;

        if (facingDir.sqrMagnitude <= 1e-6f) return false;

        float sqrRange = range * range;
        Vector3 forward = facingDir.normalized;
        float cosThreshold = Mathf.Cos(Mathf.Deg2Rad * Mathf.Clamp(halfAngleDeg, 0f, 180f));

        float bestSqr = float.MaxValue;
        List<ICollidable> list = AllManager.instance.CollisionManager.GetAllCollidables();
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            if (list[i] is not UnitBase unit) continue;
            if (unit is PlayerUnit || unit is ContainerUnit) continue;
            if (unit.IsDead) continue;

            Vector3 delta = unit.transform.position - origin;
            float sqr = delta.sqrMagnitude;
            if (sqr > sqrRange) continue;

            float invMag = 1f / Mathf.Sqrt(Mathf.Max(sqr, 1e-6f));
            Vector3 to = delta * invMag;
            float dot = Vector3.Dot(forward, to);
            if (dot < cosThreshold) continue;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                target = unit;
            }
        }

        return target != null;
    }

    public void CoolDownReset()
    {
        CoolDownReset(0f);
    }

    public virtual void CoolDownReset(float cooldown)
    {
        
    }

}
