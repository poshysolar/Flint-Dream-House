using UnityEngine;

public class GemGlowSpin : MonoBehaviour
{
    private Vector3 lockedPosition = new Vector3(37.14f, 8.805387f, 5.53f);
    private Vector3 lockedScale = new Vector3(7.45985222f, 7.45985222f, 7.45985222f);
    private float spinSpeed = 30f; // Degrees per second

    void Start()
    {
        // Lock position and scale
        transform.position = lockedPosition;
        transform.localScale = lockedScale;

        // Get the renderer of the Gem model
        Renderer gemRenderer = GetComponent<Renderer>();
        if (gemRenderer != null)
        {
            // Create and assign glowing yellow material
            Material glowMaterial = new Material(Shader.Find("Standard"));
            glowMaterial.SetColor("_EmissionColor", Color.yellow * 2f); // Bright yellow glow
            glowMaterial.EnableKeyword("_EMISSION");
            glowMaterial.SetColor("_Color", Color.white); // Base color white to preserve model texture
            gemRenderer.material = glowMaterial;
        }
        else
        {
            Debug.LogWarning("No Renderer found on Gem model. Please ensure the Gem has a MeshRenderer.");
        }
    }

    void Update()
    {
        // Keep position and scale locked
        transform.position = lockedPosition;
        transform.localScale = lockedScale;

        // Spin slowly around Y axis
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }
}