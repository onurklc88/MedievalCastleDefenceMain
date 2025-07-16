using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DoubleVisionController : MonoBehaviour
{
    [SerializeField] private DoubleVisionFeature doubleVisionFeature;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float maxOffset = 0.05f;
    [SerializeField] private float maxIntensity = 0.8f;

    private float timer;
    private bool isActive;

   
    private void Update()
    {
        if (isActive)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);

            if (doubleVisionFeature != null)
            {
                // Zamanla efektin þiddetini azalt
                float currentIntensity = Mathf.Lerp(maxIntensity, 0, progress);
                float currentOffset = Mathf.Lerp(maxOffset, 0, progress);

                doubleVisionFeature.settings.intensity = currentIntensity;
                doubleVisionFeature.settings.offset = currentOffset;
            }

            if (timer >= duration)
            {
                isActive = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.P))
        {
            ActivateEffect();
        }
    }

    public void ActivateEffect()
    {
        Debug.Log("test1");
        if (doubleVisionFeature != null)
        {
            Debug.Log("test2");
            isActive = true;
            timer = 0f;
            doubleVisionFeature.settings.intensity = maxIntensity;
            doubleVisionFeature.settings.offset = maxOffset;
        }
    }

    // Test için
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Stun Effect"))
        {
            ActivateEffect();
        }
    }
}