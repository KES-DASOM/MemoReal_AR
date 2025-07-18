
using UnityEngine;
using System.Collections;

// 이 스크립트는 캡슐 프리팹의 최상위 오브젝트에 적용합니다.
public class CapsuleController : MonoBehaviour
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

    [Tooltip("캡슐이 완전히 보이는 상태의 ClipHeight 값")]
    [SerializeField]
    private float visibleHeight = 0f;

    [Tooltip("캡슐이 완전히 숨겨진 상태의 ClipHeight 값")]
    [SerializeField]
    private float hiddenHeight = -1.5f;

    // 모든 셰이더가 공통으로 사용하는 프로퍼티의 ID
    private readonly int clipHeightPropertyID = Shader.PropertyToID("_ClipHeight");

    // 각 Renderer별로 독립적인 MaterialPropertyBlock을 사용
    private MaterialPropertyBlock bodyPropBlock;
    private MaterialPropertyBlock headPropBlock;
    private MaterialPropertyBlock photoPropBlock;

    void Awake()
    {
        bodyPropBlock = new MaterialPropertyBlock();
        headPropBlock = new MaterialPropertyBlock();
        photoPropBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// 캡슐이 땅 속으로 사라지는 '저장' 애니메이션을 재생합니다.
    /// </summary>
    public void PlaySaveAnimation()
    {
        StopAllCoroutines(); // 이전에 실행중인 애니메이션이 있다면 중지
        StartCoroutine(AnimateClipHeight(visibleHeight, hiddenHeight));
    }

    /// <summary>
    /// 캡슐이 땅에서 나타나는 '열기' 애니메이션을 재생합니다.
    /// </summary>
    public void PlayOpenAnimation()
    {
        StopAllCoroutines(); // 이전에 실행중인 애니메이션이 있다면 중지
        StartCoroutine(AnimateClipHeight(hiddenHeight, visibleHeight));
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

    // --- 테스트용 컨텍스트 메뉴 ---
    [ContextMenu("Test: Play Save Animation")]
    private void TestSave() => PlaySaveAnimation();

    [ContextMenu("Test: Play Open Animation")]
    private void TestOpen() => PlayOpenAnimation();
}
