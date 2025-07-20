using UnityEngine;
using System.Collections;

/// <summary>
/// 캡슐 프리팹에 직접 추가되어, 셰이더의 ClipHeight 값을 조절하여 애니메이션을 관리합니다。
/// 또한, 기존 Animator 컴포넌트의 애니메이션도 함께 제어합니다。
/// </summary>
public class CapsuleAnimator : MonoBehaviour
{
    [Header("구성 요소 (각 파트의 Renderer)")]
    [Tooltip("불투명한 몸통의 Renderer")]
    [SerializeField]
    private Renderer bodyRenderer;

    [Tooltip("반투명한 머리의 Renderer")]
    [SerializeField]
    private Renderer headRenderer;

    [Tooltip("사진을 표시할 Quad의 Renderer")]
    [SerializeField]
    private Renderer photoQuadRenderer;

    [Header("애니메이션 설정")]
    [Tooltip("애니메이션이 재생되는 시간 (초)")]
    [SerializeField]
    private float animationDuration = 1.5f;

    [Tooltip("캡슐 프리팹의 총 높이 (피벗이 중앙에 있을 경우). 이 값을 정확히 설정해야 합니다.")]
    [SerializeField]
    private float capsuleTotalHeight = 1.5f; // 캡슐 프리팹의 실제 높이에 맞춰 조절해주세요。

    // 모든 셰이더가 공통으로 사용하는 프로퍼티의 ID
    private readonly int clipHeightPropertyID = Shader.PropertyToID("_ClipHeight");

    // 각 Renderer별로 독립적인 MaterialPropertyBlock을 사용
    private MaterialPropertyBlock bodyPropBlock;
    private MaterialPropertyBlock headPropBlock;
    private MaterialPropertyBlock photoPropBlock;

    // 캡슐의 현재 열림/닫힘 상태를 나타내는 변수
    private bool isOpened = false; // 초기 상태는 닫힘 (땅 속에 있음)

    // --- 기존 Animator 관련 필드 ---
    [Header("기존 Animator 설정")]
    [Tooltip("캡슐 프리팹에 있는 Animator 컴포넌트를 할당해주세요.")]
    [SerializeField]
    private Animator mainAnimator; // 캡슐 프리팹의 Animator 컴포넌트

    [Tooltip("저장 애니메이션을 위한 Animator의 트리거 파라미터 이름입니다.")]
    [SerializeField]
    private string saveTriggerName = "DoSave";

    [Tooltip("열기 애니메이션을 위한 Animator의 트리거 파라미터 이름입니다.")]
    [SerializeField]
    private string openTriggerName = "DoOpen";
    // --- 여기까지 추가 ---

    void Awake()
    {
        bodyPropBlock = new MaterialPropertyBlock();
        headPropBlock = new MaterialPropertyBlock();
        photoPropBlock = new MaterialPropertyBlock();

        // Animator가 할당되지 않았다면, 같은 게임 오브젝트에서 찾기 시도
        if (mainAnimator == null)
        {
            mainAnimator = GetComponent<Animator>();
        }

        // 캡슐이 생성되자마자 완전히 보이도록 ClipHeight 초기 설정
        // 캡슐의 가장 낮은 Y 좌표보다 더 아래로 클리핑 평면을 설정
        // 피벗이 중앙일 경우: transform.position.y - (capsuleTotalHeight / 2f) - 0.1f;
        // 피벗이 바닥에 있을 경우: transform.position.y - 0.1f;
        float initialVisibleClipHeight = transform.position.y - (capsuleTotalHeight / 2f) - 0.1f; 
        SetAllClipHeights(initialVisibleClipHeight);
    }

    /// <summary>
    /// 캡슐이 땅 속으로 사라지는 '저장' 애니메이션을 재생합니다。
    /// </summary>
    public void PlaySaveAnimation()
    {
        StopAllCoroutines(); // 이전에 실행중인 애니메이션이 있다면 중지

        // 클리핑 시작점: 캡슐의 가장 낮은 Y 좌표보다 더 아래 (완전히 보이는 상태)
        float startClipHeight = transform.position.y - (capsuleTotalHeight / 2f) - 0.1f;
        // 클리핑 끝점: 캡슐의 가장 높은 Y 좌표보다 더 위 (완전히 사라지는 상태)
        float endClipHeight = transform.position.y + (capsuleTotalHeight / 2f) + 0.1f;

        StartCoroutine(AnimateClipHeight(startClipHeight, endClipHeight));

        // --- 기존 Animator 애니메이션도 함께 재생 ---
        if (mainAnimator != null)
        {
            mainAnimator.SetTrigger(saveTriggerName);
        }
        // --- 여기까지 추가 ---
    }

    /// <summary>
    /// 캡슐이 땅에서 나타나는 '열기' 애니메이션을 재생합니다。
    /// </summary>
    public void PlayOpenAnimation()
    {
        StopAllCoroutines(); // 이전에 실행중인 애니메이션이 있다면 중지

        // 클리핑 시작점: 캡슐의 가장 높은 Y 좌표보다 더 위 (완전히 사라진 상태)
        float startClipHeight = transform.position.y + (capsuleTotalHeight / 2f) + 0.1f;
        // 클리핑 끝점: 캡슐의 가장 낮은 Y 좌표보다 더 아래 (완전히 보이는 상태)
        float endClipHeight = transform.position.y - (capsuleTotalHeight / 2f) - 0.1f;

        StartCoroutine(AnimateClipHeight(startClipHeight, endClipHeight));

        // --- 기존 Animator 애니메이션도 함께 재생 ---
        if (mainAnimator != null)
        {
            mainAnimator.SetTrigger(openTriggerName);
        }
        // --- 여기까지 추가 ---
    }

    // --- 여기까지 추가 ---

    /// <summary>
    /// 캡슐의 현재 상태에 따라 열기 또는 저장 애니메이션을 토글합니다.
    /// </summary>
    public void ToggleCapsuleAnimation()
    {
        if (isOpened)
        {
            PlaySaveAnimation();
        }
        else
        {
            PlayOpenAnimation();
        }
    }

    private IEnumerator AnimateClipHeight(float start, float end)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            float currentHeight = Mathf.Lerp(start, end, elapsedTime / animationDuration);
            SetAllClipHeights(currentHeight);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 애니메이션 종료 후 최종 값으로 확실하게 설정
        SetAllClipHeights(end);

        // 애니메이션 완료 후 콜백 호출
        if (end > start) // 저장 애니메이션이 끝났을 때 (높이가 증가)
        {
            OnSaveAnimationComplete();
        }
        else // 열기 애니메이션이 끝났을 때 (높이가 감소)
        {
            OnOpenAnimationComplete();
        }
    }

    private void SetAllClipHeights(float height)
    {
        SetClipHeight(bodyRenderer, bodyPropBlock, height);
        SetClipHeight(headRenderer, headPropBlock, height);
        SetClipHeight(photoQuadRenderer, photoPropBlock, height);
    }

    private void SetClipHeight(Renderer rend, MaterialPropertyBlock block, float height)
    {
        if (rend != null)
        {
            rend.GetPropertyBlock(block);
            block.SetFloat(clipHeightPropertyID, height);
            rend.SetPropertyBlock(block);
        }
    }

    // --- 애니메이션 완료 콜백 함수들 ---

    public void OnOpenAnimationComplete()
    {
        Debug.Log("열기 애니메이션 완료! 이제 콘텐츠를 표시합니다.");
        isOpened = true; // 애니메이션 완료 후 상태 업데이트
        // 예: 여기에 캡슐에 저장된 사진을 보여주는 코드를 추가합니다.
    }

    public void OnSaveAnimationComplete()
    {
        Debug.Log("저장 애니메이션 완료!");
        isOpened = false; // 애니메이션 완료 후 상태 업데이트
        // 예: RN <-> Unity 연동 시, 여기서 RN으로 '저장 완료' 메시지를 보냅니다.
    }
}