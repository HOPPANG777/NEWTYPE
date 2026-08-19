using UnityEngine;


//==============================================================================
// Singleton<T>
//==============================================================================
// Unity에서 MonoBehaviour 기반 싱글톤을 만들기 위한 제네릭 클래스
// 이 클래스를 상속받으면 해당 클래스는 자동으로 싱글톤 패턴을 가지게 된다.
//
// 핵심 특징:
// - static instance로 전역 접근 가능
// - 최초 접근 시 자동 생성
// - 씬 전환 시 파괴되지 않음 (DontDestroyOnLoad)
//==============================================================================


// 동일한 컴포넌트를 하나의 GameObject에 여러 개 붙이는 것을 방지
// 싱글톤은 반드시 "하나만 존재해야 하기 때문에" 필수적인 보호 장치
[DisallowMultipleComponent]
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // 실제 싱글톤 인스턴스를 저장하는 static 변수
    // static이기 때문에 씬과 상관없이 하나만 유지됨
    private static T _instance;

    // 외부에서 접근할 수 있는 싱글톤 프로퍼티
    public static T instance
    {
        get
        {
            // 아직 인스턴스가 생성되지 않았을 경우
            if (_instance == null)
            {
                // 새로운 GameObject를 생성
                // 이름을 지정하지 않으면 "GameObject"라는 기본 이름으로 생성됨
                GameObject singletonObject = new GameObject();

                // 해당 GameObject에 T 타입 컴포넌트를 추가하면서 인스턴스 생성
                _instance = singletonObject.AddComponent<T>();

                // 디버그 로그 출력 (어떤 싱글톤이 생성되었는지 확인)
                Debug.Log("singleton object has been created " + _instance.name);
            }

            // 이미 존재하거나 방금 생성된 인스턴스 반환
            return _instance;
        }
    }

    // Unity의 생명주기 함수
    // GameObject가 생성될 때 호출됨
    protected virtual void Awake()
    {
        // 아직 싱글톤 인스턴스가 등록되지 않은 경우
        if (_instance == null)
        {
            // 현재 컴포넌트를 싱글톤 인스턴스로 설정
            _instance = this as T;

            // 씬 전환 시에도 파괴되지 않도록 설정
            // 싱글톤은 전역으로 유지되어야 하기 때문에 필수
            DontDestroyOnLoad(this.gameObject);

            // 생성 로그 출력
            Debug.Log("singleton object has been created " + _instance.name);
        }
        else
        {
            // 이미 다른 인스턴스가 존재하는 경우
            // 중복된 싱글톤이므로 현재 오브젝트 제거
            Destroy(gameObject);
        }
    }

    // GameObject가 파괴될 때 호출되는 함수
    private void OnDestroy()
    {
        // 현재 인스턴스 이름 출력 (디버깅용)
        // ?? 주의: _instance가 null일 경우 NullReferenceException 발생 가능
        Debug.Log(_instance.name + "singleton object has been destroyed");
    }
}