using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("ESC를 누르면 켜고 닫을 UI 패널")]
    [SerializeField] private GameObject menuPanel;

    private void Start()
    {
        // 게임이 시작할 때는 설정해둔 패널을 확실하게 꺼둡니다.
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // ESC 키(Escape) 입력을 감지합니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // 메뉴의 상태를 반대로 뒤집는 함수 (켜져 있으면 끄고, 꺼져 있으면 켬)
    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            bool isActive = !menuPanel.activeSelf;
            menuPanel.SetActive(isActive);

            // ⏱️ [핵심] 메뉴 패널이 켜지면(true) 게임 시간을 멈추고, 꺼지면(false) 다시 흐르게 합니다!
            if (isActive)
            {
                Time.timeScale = 0f;  // 게임 일시정지 (배터리 소모, 몬스터 이동 모두 정지)
            }
            else
            {
                Time.timeScale = 1f;  // 게임 재개 (정상 속도)
            }
        }
    }
}