using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class GlowingCircle : MonoBehaviour
{
    [Header("Emission Settings")]
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField, Range(0, 10)] private float emissionIntensity = 2f;
    
    [Header("Light Source Settings")]
    [SerializeField] private bool createLight = true;
    [SerializeField, Range(0, 10)] private float lightIntensity = 1f;
    [SerializeField, Range(0, 20)] private float lightRange = 10f;
    
    private Material material;
    private Light pointLight;

    void Start()
    {
        // Create new material instance
        material = new Material(Shader.Find("Standard"));
        material.EnableKeyword("_EMISSION");
        
        // Set material properties
        material.SetFloat("_Glossiness", 0.5f);
        material.SetFloat("_Metallic", 0.0f);
        
        // Apply material to the mesh renderer
        GetComponent<MeshRenderer>().material = material;
        
        // Create point light if enabled
        if (createLight)
        {
            GameObject lightObj = new GameObject("GlowLight");
            lightObj.transform.parent = this.transform;
            lightObj.transform.localPosition = Vector3.zero;
            
            pointLight = lightObj.AddComponent<Light>();
            pointLight.type = LightType.Point;
            pointLight.shadows = LightShadows.Soft;
        }
        
        UpdateGlowEffect();
    }

    void Update()
    {
        UpdateGlowEffect();
    }

    void UpdateGlowEffect()
    {
        // Update emission
        material.SetColor("_EmissionColor", glowColor * emissionIntensity);
        
        // Update light source if enabled
        if (pointLight != null)
        {
            pointLight.color = glowColor;
            pointLight.intensity = lightIntensity;
            pointLight.range = lightRange;
        }
    }

    void OnValidate()
    {
        if (material != null)
        {
            UpdateGlowEffect();
        }
    }
}