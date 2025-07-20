using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPageManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button saveCapsuleButton;
    public Button openCapsuleButton;

    void Start()
    {
        // 버튼 클릭 이벤트 리스너 추가
        if (saveCapsuleButton != null)
        {
            saveCapsuleButton.onClick.AddListener(LoadARCoreSceneForSave);
        }
        if (openCapsuleButton != null)
        {
            openCapsuleButton.onClick.AddListener(LoadARCoreSceneForOpen);
        }
    }

    void OnDestroy()
    {
        // 씬이 파괴될 때 리스너 제거 (메모리 누수 방지)
        if (saveCapsuleButton != null)
        {
            saveCapsuleButton.onClick.RemoveListener(LoadARCoreSceneForSave);
        }
        if (openCapsuleButton != null)
        {
            openCapsuleButton.onClick.RemoveListener(LoadARCoreSceneForOpen);
        }
    }

    /// <summary>
    /// ARCoreScene을 로드하고 캡슐 저장 모드를 설정합니다.
    /// </summary>
    public void LoadARCoreSceneForSave()
    {
        Debug.Log("Loading ARCoreScene for Save Capsule mode...");
        SceneManager.LoadScene("ARCoreScene");
        // 씬 로드 후 다음 프레임에 RNBridgeManager를 통해 모드 설정
        StartCoroutine(SetModeAfterSceneLoad(ARSceneManager.ARMode.SaveCapsule.ToString(), "{}"));
    }

    /// <summary>
    /// ARCoreScene을 로드하고 캡슐 오픈 모드를 설정합니다.
    /// </summary>
    public void LoadARCoreSceneForOpen()
    {
        Debug.Log("Loading ARCoreScene for Open Capsule mode...");
        SceneManager.LoadScene("ARCoreScene");
        // 씬 로드 후 다음 프레임에 RNBridgeManager를 통해 모드 설정
        // TODO: 실제 RN에서는 캡슐 ID를 여기에 전달해야 합니다.
        StartCoroutine(SetModeAfterSceneLoad(ARSceneManager.ARMode.OpenCapsule.ToString(), "{\"capsuleId\":\"test_capsule_id\"}"));
    }

    private System.Collections.IEnumerator SetModeAfterSceneLoad(string mode, string data)
    {
        // 씬 로드가 완료될 때까지 기다립니다.
        yield return null; // 한 프레임 기다림
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "ARCoreScene");

        if (RNBridgeManager.Instance != null)
        {
            RNBridgeManager.Instance.SetARModeAndData(mode, data);
        }
        else
        {
            Debug.LogError("RNBridgeManager.Instance is null. Ensure RNBridgeManager is in the scene and DontDestroyOnLoad is set.");
        }
    }
}