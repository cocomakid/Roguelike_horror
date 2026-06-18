using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;

    private PlayerController player;
    private FlashlightController flashlight;
    private XpManager xpManager;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        flashlight = FindAnyObjectByType<FlashlightController>();
        xpManager = FindAnyObjectByType<XpManager>();
    }

    public void UpgradeAttack()
    {
        player.damage += 1;

        Debug.Log("현재 공격력 : " + player.damage);
        Debug.Log("버튼 클릭 성공");
        xpManager.LevelUpComplete();

        ClosePanel();
    }

    public void UpgradeBattery()
    {
        flashlight.IncreaseMaxBattery(10);

        xpManager.LevelUpComplete();

        ClosePanel();
    }

    private void ClosePanel()
    {
        upgradePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}