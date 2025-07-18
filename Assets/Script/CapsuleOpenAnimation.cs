using UnityEngine;

public class CapsuleAnimationTest : MonoBehaviour
{
    public Transform mediaObject; // 미디어가 들어갈 오브젝트
    public Transform capsuleTargetPoint; // 캡슐 내부 위치
    public Animator capsuleLidAnimator;

    public float moveDuration = 1.5f;

    void Start()
    {
        // 시작 시 미디어 오브젝트를 캡슐로 이동
        StartCoroutine(PlaySequence());
    }

    System.Collections.IEnumerator PlaySequence()
    {
        Vector3 start = mediaObject.position;
        Vector3 end = capsuleTargetPoint.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            mediaObject.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // 뚜껑 닫기 애니메이션 재생
        capsuleLidAnimator.SetTrigger("Close");
    }
}
