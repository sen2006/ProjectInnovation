using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 originalLocalPosition;
    private Quaternion originalRotation;
    private float distortionAmount = 0f;
    private float slideOffset = 0f;
    private float slideLerpSpeed = 5f;
    private float slideTiltAmount = -20f;
    private bool isSliding = false;

    private void Start()
    {
        cameraTransform = GetComponent<Transform>();
        originalLocalPosition = cameraTransform.localPosition;
        originalRotation = cameraTransform.localRotation;
    }

    public void ApplyDistortion(float amount)
    {
        distortionAmount = amount;
    }

    public void ApplySlideEffect(float slideAmount, bool sliding)
    {
        slideOffset = slideAmount;
        isSliding = sliding;
    }

    private void Update()
    {
        Vector3 targetPosition = originalLocalPosition + new Vector3(0, -slideOffset, 0);
        Quaternion targetRotation = isSliding ? Quaternion.Euler(slideTiltAmount, 0, 0) : originalRotation;

        if (distortionAmount > 0)
        {
            float shakeAmount = distortionAmount * 0.6f;
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0
            );
            targetPosition += shakeOffset;
        }

        // Smoothly transition the camera position and rotation
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetPosition, Time.deltaTime * slideLerpSpeed);
        cameraTransform.localRotation = Quaternion.Lerp(cameraTransform.localRotation, targetRotation, Time.deltaTime * slideLerpSpeed);
    }
}
