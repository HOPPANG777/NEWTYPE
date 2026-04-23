using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

//==============================================================================
// PlayerEnemyDirectionController
//==============================================================================
// Player의 가장 가까운 몬스터를 향해 회전하는 Controller
//
//==============================================================================

public class PlayerEnemyDirectionController : MonoBehaviour, IRotationable, ISetStatus
{
    [TabGroup("PlayerDirection")][Tooltip("플레이어 방향")][SerializeField] private Vector3 _rotationDirection;
    public Vector3 RotationDirection { get { return _rotationDirection; } set { _rotationDirection = value; } }



    public void Rotate()
    {

    }

    public void SetStatus()
    {
        
    }
}