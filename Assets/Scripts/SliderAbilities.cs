using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SliderAbilities : MonoBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] private float cameraShakeAbilityDuration = 15f; // Duration of Camera Shake
    [SerializeField] private float audioDistortionAbilityDuration = 15f; // Duration of Audio Distortion

    [Header("UI Elements")]
    [SerializeField] private Image cameraShakeCooldownImage; // Assign in Inspector
    [SerializeField] private Image audioDistortionCooldownImage; // Assign in Inspector

    private bool canUseCameraShake = true;
    private bool canUseAudioDistortion = true;

    private DJAbilities djAbilities;
    private void Start()
    {
        djAbilities = FindObjectOfType<DJAbilities>(); // Find DJAbilities in the scene

        if (djAbilities == null)
        {
            Debug.LogError("DJAbilities script not found! Ensure it exists in the scene.");
        }
    }

    private void Update()
    {
        // Input to trigger Camera Shake Ability
        if (Input.GetKeyDown(KeyCode.R) && canUseCameraShake)
        {
            StartCoroutine(ActivateCameraShake());
        }

        // Input to trigger Audio Distortion Ability
        if (Input.GetKeyDown(KeyCode.F) && canUseAudioDistortion)
        {
            StartCoroutine(ActivateAudioDistortion());
        }
    }

    // Camera Shake Ability
    private IEnumerator ActivateCameraShake()
    {
        canUseCameraShake = false;
        Debug.Log("Camera Shake Activated!");

        // Call the Networked RPC method
        if (djAbilities != null && djAbilities.IsOwner)
        {
            djAbilities.PickupCameraDistortionPowerupRpc();
        }

        // Start cooldown effect
        StartCoroutine(CooldownTimer(cameraShakeCooldownImage, cameraShakeAbilityDuration));

        yield return new WaitForSeconds(cameraShakeAbilityDuration);
        Debug.Log("Camera Shake Ended!");

        canUseCameraShake = true;
    }

    // Audio Distortion Ability
    private IEnumerator ActivateAudioDistortion()
    {
        canUseAudioDistortion = false;
        Debug.Log("Audio Distortion Activated!");

        // Call the Networked RPC method
        if (djAbilities != null && djAbilities.IsOwner)
        {
            djAbilities.PickupAudioDistortionPowerupRpc();
        }

        // Start cooldown effect
        StartCoroutine(CooldownTimer(audioDistortionCooldownImage, audioDistortionAbilityDuration));

        yield return new WaitForSeconds(audioDistortionAbilityDuration);
        Debug.Log("Audio Distortion Ended!");

        canUseAudioDistortion = true;
    }

    // Cooldown Timer (for ability cooldown)
    private IEnumerator CooldownTimer(Image cooldownImage, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = elapsed / duration; // Update UI fill
            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f; // Reset to full

    }

    // Pseudo Function to reset slider value on the phone (Placeholder)
    private void ResetSliderToZero(string abilityName)
    {
        // This function should communicate with the phone's slider system
        // Example: Send message to reset the corresponding slider to 0
        Debug.Log($"Resetting {abilityName} slider to 0 on phone.");
    }
}
