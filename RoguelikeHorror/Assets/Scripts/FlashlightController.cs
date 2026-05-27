using UnityEngine;
using UnityEngine.Rendering.Universal; 

public class FlashlightController : MonoBehaviour
{
    [Header("조명 설정")]
    public Light2D flashlight;  
    public float maxRadius = 5f; 
    public float maxIntensity = 1.5f;  

    [Header("배터리 시스템")]
    public float maxBattery = 100f; 
    private float currentBattery;
    public float drainRate = 5f;  

    private bool isDead = false;

    void Start()
    {
        currentBattery = maxBattery;
    }

    void Update()
    {
        if (isDead) return;

        currentBattery -= drainRate * Time.deltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery); 

        float batteryRatio = currentBattery / maxBattery;

        if (flashlight != null)
        {
            flashlight.pointLightOuterRadius = maxRadius * batteryRatio;
            flashlight.intensity = maxIntensity * batteryRatio;
        }

        if (currentBattery <= 0)
        {
            Die();
        }
    }

    public void RechargeBattery(float amount)
    {
        if (isDead) return;

        currentBattery += amount;
        if (currentBattery > maxBattery) currentBattery = maxBattery;

        Debug.Log("배터리 충전 완료! 현재 배터리: " + currentBattery);
    }

    void Die()
    {
        isDead = true;
        Debug.Log("암흑 속에서 사망했습니다...");
    }
}