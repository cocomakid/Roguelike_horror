using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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

    [Header("UI 연동")]
    public FlashlightManager flashlightManager;

    private bool isDead = false;

    void Start()
    {
        currentBattery = maxBattery;
    }

    void Update()
    {
        if (isDead) return;

        if (GameObject.Find("Upgrade_Panel") != null && GameObject.Find("Upgrade_Panel").activeSelf)
        {
            return;
        }

        currentBattery -= drainRate * Time.deltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery); 

        float batteryRatio = currentBattery / maxBattery;

        if (flashlight != null)
        {
            flashlight.pointLightOuterRadius = maxRadius * batteryRatio;
            flashlight.intensity = maxIntensity * batteryRatio;
        }

        // UI/점프스케어는 매니저에게 위임
        if (flashlightManager != null)
            flashlightManager.UpdateBattery(currentBattery, maxBattery);

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

        // 플레이어 체력이 다 닳았을 때처럼 시작 체력을 4로 리셋
        PlayerController.currentHp = 4;

        // 1.5초 뒤에 현재 씬을 재시작합니다. (PlayerController와 동일한 연출)
        Invoke(nameof(RestartScene), 1.5f);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IncreaseMaxBattery(float amount)
    {
        maxBattery += amount;
        currentBattery += amount; // 최대치가 늘어난 만큼 현재 배터리도 보너스로 채워줍니다.
        Debug.Log("최대 배터리 증가! 현재 최대치: " + maxBattery);
    }

}