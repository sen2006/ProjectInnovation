using UnityEngine;

public class CameraGlitchEffect : MonoBehaviour
{
    [SerializeField] private GameObject glitchQuad; // Assign the Quad in the Inspector
    [SerializeField] private Material glitchMaterial; // Assign the glitch material in the Inspector
    [SerializeField] private Transform cameraTransform; // Assign Main Camera in the Inspector

    private bool isGlitchActive = false;

    private void Start()
    {
        if (glitchQuad != null)
            glitchQuad.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha2))
        {
            ActivateGlitch(true);
        }
        else
        {
            ActivateGlitch(false);
        }

        // Make sure the Quad always follows the camera
        if (glitchQuad != null && cameraTransform != null)
        {
            glitchQuad.transform.position = cameraTransform.position + cameraTransform.forward * 0.5f;
            glitchQuad.transform.rotation = cameraTransform.rotation;
        }
    }

    public void ActivateGlitch(bool state)
    {
        if (glitchQuad != null)
        {
            glitchQuad.SetActive(state);
            isGlitchActive = state;

            if (glitchMaterial != null)
                glitchMaterial.SetFloat("_GlitchStrength", state ? 1.0f : 0f);
        }
    }
}
