using UnityEngine;

public class PlayerMobileMoveController : MonoBehaviour
{
    [SerializeField] private PlayerMoveController _moveController;
    [SerializeField] private PlayerMouseDirectionController _rotationController;

    private Vector2 _touchStartPos;
    private bool _isDragging = false;

    void Update()
    {
        if (AllManager.instance.GamemodeManager.GameMode != GameModeType.GAMEMODE_INGAME) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _touchStartPos = touch.position;
                _isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                Vector2 currentTouchPos = touch.position;
                Vector2 direction = (currentTouchPos - _touchStartPos).normalized;

                // 이동 스크립트에 방향 전달
                _moveController.MoveDirection = new Vector3(direction.x, direction.y, 0);

                // 회전 스크립트에 방향 전달 (기존 Rotate 로직 대신 방향만 갱신)
                _rotationController.RotationDirection = new Vector3(direction.x, direction.y, 0);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _isDragging = false;
                _moveController.MoveDirection = Vector3.zero; // 손을 떼면 멈춤
            }
        }
    }
}