using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI; // UnityEngine.UI 추가

public class ARSceneManager : MonoBehaviour
{
    public enum ARMode
    {
        None,           // 아무 동작도 하지 않음
        SaveCapsule,    // 캡슐 저장 (생성) 모드
        OpenCapsule     // 캡슐 오픈 (상호작용) 모드
    }

    [Header("AR 관련 컴포넌트")]
    [Tooltip("씬에 있는 AR Raycast Manager를 할당해주세요.")]
    [SerializeField]
    private ARRaycastManager raycastManager;

    [Tooltip("씬에 있는 AR Plane Manager를 할 할당해주세요.")]
    [SerializeField]
    private ARPlaneManager planeManager;

    [Header("캡슐 설정")]
    [Tooltip("생성할 캡슐의 프리팹을 할당해주세요. 이 프리팹에는 CapsuleAnimator.cs 스크립트가 있어야 합니다.")]
    [SerializeField]
    private GameObject capsulePrefab;

    [Header("캡슐 생성 위치 조정")]
    [Tooltip("캡슐이 생성될 때 적용될 오프셋입니다.\nY: 땅으로부터의 높이 조절 (음수면 더 낮게)\nZ: 평면으로부터의 거리 조절 (양수면 더 멀리, 음수면 더 카메라에 가깝게)")]
    [SerializeField]
    private Vector3 spawnOffset = new Vector3(0f, -10f, 0.2f); // Y 값을 -0.5f로 변경하여 더 낮게 생성

    private GameObject spawnedCapsule; // 현재 씬에 생성된 캡슐 오브젝트
    private CapsuleAnimator capsuleAnimator;  // 생성된 캡슐의 애니메이션 컨트롤러

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [Header("디버그 UI")]
    [Tooltip("AR Session 상태를 표시할 TextMeshProUGUI 컴포넌트")]
    [SerializeField]
    private TextMeshProUGUI arSessionStateText;

    [Tooltip("현재 AR 모드를 표시할 TextMeshProUGUI 컴포넌트")]
    [SerializeField]
    private TextMeshProUGUI arModeText;



    [Header("모드 선택 버튼")]
    [SerializeField]
    private Button saveModeButton;
    [SerializeField]
    private Button openModeButton;

    private ARMode currentMode = ARMode.None;

    void Awake()
    {
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
        }
        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
        }

        // 씬 로드 시 평면 감지 활성화 (초기 상태)
        SetPlaneDetectionActive(true);

        // 버튼 클릭 이벤트 리스너 추가
        if (saveModeButton != null)
        {
            saveModeButton.onClick.AddListener(OnSaveModeButtonClicked);
        }
        if (openModeButton != null)
        {
            openModeButton.onClick.AddListener(OnOpenModeButtonClicked);
        }
    }

    void Update()
    {
        // AR Session 상태 표시
        if (arSessionStateText != null)
        {
            arSessionStateText.text = $"AR Session State: {ARSession.state}";
        }
        // 현재 AR 모드 표시
        if (arModeText != null)
        {
            arModeText.text = $"AR Mode: {currentMode}";
        }

        if (Touchscreen.current == null || !Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return;
        }

        Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

        switch (currentMode)
        {
            case ARMode.SaveCapsule:
                HandleSaveCapsuleMode(touchPosition);
                break;
            case ARMode.OpenCapsule:
                HandleOpenCapsuleMode(touchPosition);
                break;
            case ARMode.None:
            default:
                // 아무것도 하지 않음
                break;
        }
    }

    /// <summary>
    /// RN으로부터 AR 모드를 설정합니다.
    /// </summary>
    /// <param name="modeString">설정할 모드 문자열 (예: "SaveCapsule", "OpenCapsule")</param>
    public void SetARMode(string modeString)
    {
        if (System.Enum.TryParse(modeString, out ARMode newMode))
        {
            currentMode = newMode;
            Debug.Log($"AR Mode set to: {currentMode}");

            // 모드 변경 시 초기화
            if (spawnedCapsule != null)
            {
                Destroy(spawnedCapsule);
                spawnedCapsule = null;
                capsuleAnimator = null;
            }

            // 새로운 모드에 따라 평면 감지 활성화/비활성화
            if (currentMode == ARMode.SaveCapsule || currentMode == ARMode.OpenCapsule)
            {
                SetPlaneDetectionActive(true);
            }
            else
            {
                SetPlaneDetectionActive(false);
            }
        }
        else
        {
            Debug.LogError($"Invalid AR Mode string: {modeString}");
        }
    }

    private void HandleSaveCapsuleMode(Vector2 touchPosition)
    {
        // 캡슐이 없는 상태에서 바닥을 터치했을 때만 생성
        if (spawnedCapsule == null && raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            var hitPose = hits[0].pose;

            // hitPose.position에 spawnOffset을 더하여 최종 위치 계산
            Vector3 finalSpawnPosition = hitPose.position + spawnOffset;

            spawnedCapsule = Instantiate(capsulePrefab, finalSpawnPosition, hitPose.rotation);
            Debug.Log($"캡슐 생성 위치: {finalSpawnPosition}");

            capsuleAnimator = spawnedCapsule.GetComponent<CapsuleAnimator>();

            if (capsuleAnimator != null)
            {
                Debug.Log("'저장' 애니메이션을 요청합니다.");
                capsuleAnimator.PlaySaveAnimation(); // 캡슐에게 저장 애니메이션을 요청
            }
            else
            {
                Debug.LogError($"오류: {capsulePrefab.name} 프리팹에 CapsuleAnimator 컴포넌트가 없습니다!");
            }

            // 터치 감지를 위해 콜라이더가 없으면 자동으로 추가
            if (spawnedCapsule.GetComponentInChildren<Collider>() == null)
            {
                var newCollider = spawnedCapsule.AddComponent<MeshCollider>();
                newCollider.convex = true; // 동적인 오브젝트와의 충돌을 위해 Convex 설정
                Debug.Log("터치 감지를 위해 MeshCollider를 자동으로 추가했습니다.");
            }

            // 캡슐 생성 후 평면 감지 비활성화
            SetPlaneDetectionActive(false);

            // TODO: RN으로 캡슐 저장 완료 메시지 및 데이터 전송
            // ReturnToReactNative("CapsuleSaved", "{}");
        }
    }

    private void HandleOpenCapsuleMode(Vector2 touchPosition)
    {
        // 캡슐이 아직 배치되지 않았고, 평면 감지가 활성화된 경우에만 터치로 배치 시도
        if (spawnedCapsule == null && planeManager.enabled)
        {
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;

                // hitPose.position에 spawnOffset을 더하여 최종 위치 계산
                Vector3 finalSpawnPosition = hitPose.position + spawnOffset;

                spawnedCapsule = Instantiate(capsulePrefab, finalSpawnPosition, hitPose.rotation);
                Debug.Log($"캡슐 배치 위치: {finalSpawnPosition}");

                capsuleAnimator = spawnedCapsule.GetComponent<CapsuleAnimator>();

                if (capsuleAnimator != null)
                {
                    Debug.Log("캡슐 오픈 씬: '열기' 애니메이션을 요청합니다.");
                    capsuleAnimator.PlayOpenAnimation(); // 캡슐에게 열기 애니메이션을 요청
                }
                else
                {
                    Debug.LogError($"오류: {capsulePrefab.name} 프리팹에 CapsuleAnimator 컴포넌트가 없습니다!");
                }

                // 터치 감지를 위해 콜라이더가 없으면 자동으로 추가
                if (spawnedCapsule.GetComponentInChildren<Collider>() == null)
                {
                    var newCollider = spawnedCapsule.AddComponent<MeshCollider>();
                    newCollider.convex = true; // 동적인 오브젝트와의 충돌을 위해 Convex 설정
                    Debug.Log("터치 감지를 위해 MeshCollider를 자동으로 추가했습니다.");
                }

                // 캡슐 배치 후 평면 감지 비활성화
                SetPlaneDetectionActive(false);
            }
        }
        // 캡슐이 배치된 상태에서 터치했을 때 토글 애니메이션
        else if (spawnedCapsule != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hitObject))
            {
                if (hitObject.transform.IsChildOf(spawnedCapsule.transform) || hitObject.transform == spawnedCapsule.transform)
                {
                    if (capsuleAnimator != null)
                    {
                        Debug.Log("캡슐 터치 감지! '열기/닫기' 애니메이션을 요청합니다.");
                        capsuleAnimator.ToggleCapsuleAnimation();
                    }
                }
            }
        }
    }

    /// <summary>
    /// AR 평면 감지 활성화/비활성화
    /// </summary>
    /// <param name="enable">활성화 여부</param>
    public void SetPlaneDetectionActive(bool enable)
    {
        if (planeManager != null)
        {
            planeManager.enabled = enable;
            // 평면 시각화도 함께 제어 (AR Default Plane 프리팹이 할당되어 있다면)
            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(enable);
            }
            Debug.Log($"AR Plane Manager {(enable ? "활성화" : "비활성화")}.");
        }
    }

    /// <summary>
    /// RN으로 돌아가는 메서드 (플랫폼별 구현 필요)
    /// </summary>
    /// <param name="messageType">메시지 타입</param>
    /// <param name="messageData">메시지 데이터 (JSON 문자열)</param>
    public void ReturnToReactNative(string messageType, string messageData)
    {
        Debug.Log($"Returning to React Native with message: {messageType}, Data: {messageData}");
        // 이 부분은 Unity를 라이브러리로 임베드할 때 사용하는 플러그인이나
        // 직접 구현하는 네이티브 브릿지에 따라 달라집니다.
        // 예시:
        // RNBridgeManager.Instance.SendMessageToRN(messageType, messageData);
        // Application.Quit(); // Unity 앱을 종료하고 RN으로 돌아가는 방식 (플랫폼에 따라 다름)

        // Unity 앱을 종료하는 대신, RN으로 메시지를 보내고 RN에서 Unity 뷰를 닫도록 하는 것이 일반적입니다.
        // 여기서는 예시로 Debug.Log만 남겨둡니다.
    }

    /// <summary>
    /// "캡슐 저장 모드" 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnSaveModeButtonClicked()
    {
        SetARMode(ARMode.SaveCapsule.ToString());
    }

    /// <summary>
    /// "캡슐 오픈 모드" 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnOpenModeButtonClicked()
    {
        SetARMode(ARMode.OpenCapsule.ToString());
    }
}