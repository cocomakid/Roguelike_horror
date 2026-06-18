using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class XpManager : MonoBehaviour
{
    public TMP_Text xpText;
    [SerializeField] GameObject UpgradPanel;

    private int xpCount = 0;
    private int maxXp = 100;

    private void Start()
    {
        // 📂 ⭐ [추가] 세이브 데이터가 있다면 최대 경험치 통도 불러오기
        SaveManager saveManager = FindAnyObjectByType<SaveManager>();
        if (saveManager != null)
        {
            PlayerSaveData loadedData = saveManager.LoadGame();
            if (loadedData != null)
            {
                maxXp = loadedData.maxXp; // 저장되었던 최대 경험치 통 적용!
            }
        }

        UpdateXpText();
    }
    public int GetMaxXp()
    {
        return maxXp;
    }

    public void AddXp()
    {
        xpCount += 10;
        UpdateXpText();

        if (xpCount >= maxXp)
        {
            UpgradPanel.SetActive(true);

            // ⭐ 마우스 락을 풀고, 확실하게 보이게 설정
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // 💡 [추가] 두 번째 열릴 때 유니티가 마우스 좌표를 놓치지 않도록 강제 새로고침
            OnApplicationFocus(true);
        }
    }

    // 💡 [추가] 마우스 포커스 꼬임 방지를 위한 안전장치 함수
    private void OnApplicationFocus(bool hasFocus)
    {
        if (UpgradPanel != null && UpgradPanel.activeSelf)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // ⭐ UpgradeManager가 버튼 클릭 후 호출할 함수입니다!
    public void LevelUpComplete()
    {
        xpCount -= maxXp;
        if (xpCount < 0) xpCount = 0;

        // 배터리 증가 버튼을 눌렀을 때만 maxXp를 늘리고 싶다면 UpgradeManager 쪽에서 조절하거나,
        // 보편적인 로그라이크처럼 레벨업할 때마다 무조건 통이 커지게 하려면 여기에 둡니다.
        maxXp += 10;

        Time.timeScale = 1f; // 다시 재생!
        UpdateXpText();
    }

    private void UpdateXpText()
    {
        xpText.text = "Xp : " + xpCount.ToString() + " / " + maxXp.ToString();
    }
}