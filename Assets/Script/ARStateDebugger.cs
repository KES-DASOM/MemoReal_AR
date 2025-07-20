using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class ARStateDebugger : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI arSessionStateText;

    void Update()
    {
        if (arSessionStateText != null)
        {
            arSessionStateText.text = $"AR Session State: {ARSession.state}";
        }
    }
}
