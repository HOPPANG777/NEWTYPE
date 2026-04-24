using UnityEngine;

public class MobileLetterBoxController : MonoBehaviour
{
    public void LetterBox()
    {
        Camera camera = Camera.main;
        float targetRatio = 9.0f / 16.0f;
        float currentRatio = (float)Screen.width / Screen.height;
        float scale = currentRatio / targetRatio;

        if (scale < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scale;
            rect.x = 0;
            rect.y = (1.0f - scale) / 2.0f;
            camera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scale;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }

    public void CreateMaskCamera()
    {
        // 배경을 검은색으로 채울 보조 카메라 생성
        GameObject maskObj = new GameObject("LetterBoxMaskCamera");
        Camera maskCam = maskObj.AddComponent<Camera>();

        // 메인 카메라보다 먼저 렌더링되도록 설정 (낮은 Depth)
        maskCam.depth = -100;
        maskCam.backgroundColor = Color.black;
        maskCam.clearFlags = CameraClearFlags.SolidColor;

        // 화면 전체를 덮도록 설정
        maskCam.rect = new Rect(0, 0, 1, 1);

        // 아무것도 렌더링하지 않고 오직 배경색만 출력
        maskCam.cullingMask = 0;
    }
}
