using UnityEngine;

public class MobileLetterBoxController : MonoBehaviour
{
    public void LetterBox()
    {
        // 현재 메인 카메라
        Camera camera = Camera.main;

        // 타겟 비율
        float targetRatio = 9.0f / 16.0f;

        // 현재 비율
        float currentRatio = (float)Screen.width / (float)Screen.height;    

        // 비율 조정
        if (currentRatio < targetRatio)
        {
            // 화면이 더 좁을 때 (상하 여백이 남음)
            camera.orthographicSize = 5.0f / (currentRatio / targetRatio);
        }
        else
        {
            // 화면이 더 넓을 때 (좌우 여백이 남음)
            // orthographicsSize는 고정
        }
    }
}
