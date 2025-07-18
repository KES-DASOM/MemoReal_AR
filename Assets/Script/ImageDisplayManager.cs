using UnityEngine;
using System.IO;

// 이 스크립트는 사진을 표시할 Quad 오브젝트에만 적용합니다.
public class ImageDisplayManager : MonoBehaviour
{
    [Tooltip("사진을 표시할 Quad의 Mesh Renderer를 여기에 연결하세요.")]
    [SerializeField]
    private Renderer targetRenderer;

    // Opaque_Photo_Shader의 MainTexture 프로퍼티 Reference 이름
    private readonly int mainTexturePropertyID = Shader.PropertyToID("_MainTexture");

    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
    }

    /// <summary>
    /// 이미지 바이트 데이터를 받아 텍스처로 변환하고 머티리얼에 적용합니다.
    /// </summary>
    public void SetImage(byte[] imageData)
    {
        if (targetRenderer == null)
        {
            Debug.LogError("Target Renderer가 할당되지 않았습니다!", this);
            return;
        }

        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageData))
        {
            targetRenderer.GetPropertyBlock(propBlock);
            propBlock.SetTexture(mainTexturePropertyID, texture);
            targetRenderer.SetPropertyBlock(propBlock);
        }
        else
        {
            Debug.LogError("이미지 데이터 로드에 실패했습니다.", this);
        }
    }

    [ContextMenu("Test: Load Image from File")]
    public void TestSetImageFromFile()
    {
        try
        {
            string path = Path.Combine(Application.dataPath, "..", "TestImage.png");
            byte[] testImageData = File.ReadAllBytes(path);
            SetImage(testImageData);
            Debug.Log("테스트 이미지 적용 완료!", this);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"테스트 이미지 로드 실패: {e.Message}", this);
        }
    }
}