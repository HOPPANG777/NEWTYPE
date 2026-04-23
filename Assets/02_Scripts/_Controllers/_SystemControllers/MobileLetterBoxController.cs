using UnityEngine;

public class MobileLetterBoxController : MonoBehaviour
{
    void Start()
    {
        LetterBox();
    }

    public void LetterBox()
    {
        Camera camera = Camera.main;

        // 타겟 비율 (세로형 게임이면 9/16, 가로형이면 16/9)
        float targetRatio = 9.0f / 16.0f;

        // 현재 기기 비율
        float currentRatio = (float)Screen.width / Screen.height;

        // 현재 비율을 타겟 비율로 나눈 값
        float scale = currentRatio / targetRatio;

        if (scale < 1.0f)
        {
            // 기기가 타겟보다 세로로 더 길 때 (위아래 여백 생성)
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scale;
            rect.x = 0;
            rect.y = (1.0f - scale) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            // 기기가 타겟보다 가로로 더 길 때 (좌우 여백 생성)
            float scaleWidth = 1.0f / scale;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }
}
