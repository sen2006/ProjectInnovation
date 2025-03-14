using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhaseAbility : MonoBehaviour
{
    [Header("Phase Ability Settings")]
    [SerializeField] private float phaseDuration = 2f;
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private string phaseableWallTag = "PhaseableWallSpawn";

    [Header("Materials")]
    [SerializeField] private Material normalWallMaterial;
    [SerializeField] private Material glitchWallMaterial;

    [Header("UI & Effects")]
    [SerializeField] private Image cooldownImage; // Assign in Inspector
    [SerializeField] private Image phaseOverlay; // Flash effect (assign in Inspector)
    [SerializeField] private AudioSource phaseSound; // Assign an AudioSource with a phase sound
    [SerializeField] private Transform cameraTransform; // Assign Main Camera for shake effect

    private bool canUseAbility = true;
    private Vector3 originalCamPosition;

    private void Start()
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f; // Start as fully ready

        if (phaseOverlay != null)
            phaseOverlay.enabled = false; // Hide overlay initially

        if (cameraTransform != null)
            originalCamPosition = cameraTransform.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canUseAbility)
        {
            StartCoroutine(ActivatePhaseMode());
        }
    }

    private IEnumerator ActivatePhaseMode()
    {
        cooldownImage.fillAmount = 0;
        canUseAbility = false;
        Debug.Log("Phase Ability Activated!");

        if (phaseSound != null) phaseSound.Play(); // Play sound effect
        if (phaseOverlay != null) StartCoroutine(FlashOverlay()); // Flash UI overlay
        if (cameraTransform != null) StartCoroutine(CameraShake(0.15f, 0.1f)); // Shake camera

        SetWallsTriggerState(true);
        ApplyWallGlitch(true);

        yield return new WaitForSeconds(phaseDuration);

        SetWallsTriggerState(false);
        ApplyWallGlitch(false);

        Debug.Log("Phase Ability Ended!");

        // Start Cooldown UI Effect
        StartCoroutine(CooldownTimer());
    }

    private IEnumerator CooldownTimer()
    {
        float elapsed = 0f;
        while (elapsed < cooldownDuration)
        {
            elapsed += Time.deltaTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = elapsed / cooldownDuration; // Update UI fill
            yield return null;
        }

        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f; // Fully ready

        canUseAbility = true;
        Debug.Log("Phase Ability Ready Again!");
    }

    private void SetWallsTriggerState(bool state)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag(phaseableWallTag);
        foreach (GameObject wall in walls)
        {
            if (wall.TryGetComponent(out Collider wallCollider))
            {
                wallCollider.isTrigger = state;
            }
        }
    }

    private void ApplyWallGlitch(bool enableGlitch)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag(phaseableWallTag);
        foreach (GameObject wall in walls)
        {
            if (wall.TryGetComponent(out Renderer renderer))
            {
                renderer.material = enableGlitch ? glitchWallMaterial : normalWallMaterial;
            }
        }
    }

    // 🔹 UI Flash Effect
    private IEnumerator FlashOverlay()
    {
        phaseOverlay.enabled = true;
        phaseOverlay.color = new Color(1, 1, 1, 0.3f); // Semi-transparent white

        float fadeTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            phaseOverlay.color = new Color(1, 1, 1, Mathf.Lerp(0.3f, 0f, elapsed / fadeTime));
            yield return null;
        }
        phaseOverlay.enabled = false;
    }

    // 🔹 Camera Shake Effect
    private IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 randomOffset = Random.insideUnitSphere * magnitude;
            cameraTransform.localPosition = originalCamPosition + randomOffset;
            yield return null;
        }
        cameraTransform.localPosition = originalCamPosition; // Reset position
    }
}
