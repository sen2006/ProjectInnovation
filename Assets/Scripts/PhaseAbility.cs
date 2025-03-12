using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhaseAbility : MonoBehaviour
{
    [Header("Phase Ability Settings")]
    [SerializeField] private float phaseDuration = 2f;
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private string phaseableWallTag = "PhaseableWall";

    [Header("Materials")]
    [SerializeField] private Material normalWallMaterial;
    [SerializeField] private Material glitchWallMaterial;

    [Header("UI Elements")]
    [SerializeField] private Image cooldownImage; // Assign this in the Inspector

    private bool canUseAbility = true;

    private void Start()
    {
        if (cooldownImage != null)
            cooldownImage.fillAmount = 1f; // Start as fully ready
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
}
