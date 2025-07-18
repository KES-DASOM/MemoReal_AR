using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

/// <summary>
/// AR 환경에서 캡슐의 생성 및 애니메이션을 테스트하기 위한 통합 컨트롤러입니다.
/// </summary>
public class CapsuleTestController : MonoBehaviour
{
    [Header("AR 관련 컴포넌트")]
    [Tooltip("씬에 있는 AR Raycast Manager를 할당해주세요.")]
    [SerializeField]
    private ARRaycastManager raycastManager;

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

    void Awake()
    {
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
        }
    }

    void Update()
    {
        if (Touchscreen.current == null || !Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return;
        }

        Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

        // 캡슐이 이미 생성된 상태에서 터치했을 때
        if (spawnedCapsule != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hitObject))
            {
                // 터치한 오브젝트가 생성된 캡슐이 맞는지 확인
                if (hitObject.transform.IsChildOf(spawnedCapsule.transform) || hitObject.transform == spawnedCapsule.transform)
                {
                    if (capsuleAnimator != null)
                    {
                        Debug.Log("캡슐 터치 감지! '열기' 애니메이션을 요청합니다.");
                        capsuleAnimator.PlayOpenAnimation(); // 캡슐에게 열기 애니메이션을 요청
                    }
                    return;
                }
            }
        }

        // 캡슐이 없는 상태에서 바닥을 터치했을 때
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
        }
    }
}