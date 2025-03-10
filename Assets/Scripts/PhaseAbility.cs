using System.Collections;
using UnityEngine;

public class PhaseAbility : MonoBehaviour
{
    [Header("Phase Ability Settings")]
    [SerializeField] private float phaseDuration = 2f;
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private string phaseableWallTag = "PhaseableWall";

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material glitchMaterial; // Glitch effect material

    private bool canUseAbility = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canUseAbility)
        {
            StartCoroutine(ActivatePhaseMode());
        }
    }

    private IEnumerator ActivatePhaseMode()
    {
        canUseAbility = false;
        Debug.Log("Phase Ability Activated!");

        SetWallsTriggerState(true);
        ApplyGlitchMaterial(true);

        yield return new WaitForSeconds(phaseDuration);

        SetWallsTriggerState(false);
        ApplyGlitchMaterial(false);

        Debug.Log("Phase Ability Ended!");
        yield return new WaitForSeconds(cooldownDuration);
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

    private void ApplyGlitchMaterial(bool enableGlitch)
    {
        GameObject[] walls = GameObject.FindGameObjectsWithTag(phaseableWallTag);
        foreach (GameObject wall in walls)
        {
            if (wall.TryGetComponent(out Renderer renderer))
            {
                renderer.material = enableGlitch ? glitchMaterial : normalMaterial;
            }
        }
    }
}
