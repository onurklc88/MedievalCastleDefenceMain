using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DoubleVisionEffect : MonoBehaviour
{
    [SerializeField] private VolumeProfile volumeProfile;
    [SerializeField] private float effectDuration = 2f;
    [SerializeField] private float maxOffset = 0.05f;
    [SerializeField] private float maxIntensity = 0.7f;

    private Material doubleVisionMaterial;
    private bool isActive = false;
    private float timer = 0f;

    private void Start()
    {
        // Shader'dan material olu�tur
        Shader doubleVisionShader = Shader.Find("Custom/DoubleVision");
        if (doubleVisionShader != null)
        {
            doubleVisionMaterial = new Material(doubleVisionShader);
        }

        // Varsay�lan de�erler
        if (doubleVisionMaterial != null)
        {
            doubleVisionMaterial.SetFloat("_Offset", 0);
            doubleVisionMaterial.SetFloat("_Intensity", 0);
        }
    }

    public void ActivateEffect()
    {
        isActive = true;
        timer = 0f;
    }

    private void Update()
    {
        if (isActive)
        {
            timer += Time.deltaTime;

            if (timer <= effectDuration)
            {
                // Efektin �iddetini zamanla azalt
                float progress = timer / effectDuration;
                float currentOffset = Mathf.Lerp(maxOffset, 0, progress);
                float currentIntensity = Mathf.Lerp(maxIntensity, 0, progress);

                if (doubleVisionMaterial != null)
                {
                    doubleVisionMaterial.SetFloat("_Offset", currentOffset);
                    doubleVisionMaterial.SetFloat("_Intensity", currentIntensity);
                }
            }
            else
            {
                isActive = false;
            }
        }
    }

    // Full-screen efekt i�in render callback
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (isActive && doubleVisionMaterial != null)
        {
            Graphics.Blit(src, dest, doubleVisionMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}