using UnityEngine;
using UnityEngine.SceneManagement;

public class RNBridgeManager : MonoBehaviour
{
    public static RNBridgeManager Instance { get; private set; }

    private ARSceneManager arSceneManager; // ARSceneManager 참조

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 이 오브젝트가 파괴되지 않도록
        }
    }

    void Start()
    {
        // ARSceneManager는 ARCoreScene에 있을 것이므로, 해당 씬이 로드된 후 찾습니다.
        // 하지만 RNBridgeManager는 DontDestroyOnLoad이므로, ARCoreScene이 로드될 때까지 기다려야 합니다.
        // 여기서는 간단히 FindObjectOfType으로 찾지만, 실제 앱에서는 씬 로드 완료 콜백 등을 사용하는 것이 좋습니다.
        arSceneManager = FindObjectOfType<ARSceneManager>();
        if (arSceneManager == null)
        {
            Debug.LogWarning("ARSceneManager not found in the current scene. Ensure it's in ARCoreScene.");
        }
    }

    /// <summary>
    /// RN에서 호출하여 AR 모드를 설정하고 필요한 데이터를 전달합니다.
    /// </summary>
    /// <param name="modeString">설정할 모드 문자열 (예: "SaveCapsule", "OpenCapsule")</param>
    /// <param name="dataJson">모드에 따른 데이터 (JSON 문자열)</param>
    public void SetARModeAndData(string modeString, string dataJson)
    {
        if (arSceneManager == null)
        {
            arSceneManager = FindObjectOfType<ARSceneManager>(); // 다시 시도
        }

        if (arSceneManager != null)
        {
            arSceneManager.SetARMode(modeString);
            // TODO: dataJson을 파싱하여 ARSceneManager의 적절한 메서드에 전달
            Debug.Log($"RNBridgeManager: Received mode: {modeString}, data: {dataJson}");
        }
        else
        {
            Debug.LogError("ARSceneManager is null. Cannot set AR mode or data.");
        }
    }

    // Unity에서 RN으로 데이터를 보낼 때 사용할 메서드 (플랫폼별 구현 필요)
    public void SendMessageToRN(string messageType, string messageData)
    {
        // 이 부분은 Unity를 라이브러리로 임베드할 때 사용하는 플러그인이나
        // 직접 구현하는 네이티브 브릿지에 따라 달라집니다.
        // Android 예시: UnityPlayer.UnitySendMessage("RNBridgeReceiver", "OnUnityMessage", messageData);
        // iOS 예시: UnityFramework.sendMessageToRN(messageType, messageData);
        Debug.Log($"RNBridgeManager: Sending message to RN - Type: {messageType}, Data: {messageData}");
    }
}