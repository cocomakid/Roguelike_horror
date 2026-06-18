using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Image 컴포넌트를 쓰기 위해 반드시 필요합니다.
using UnityEngine;
using TMPro;

public class FlashlightManager : MonoBehaviour
{
    public TMP_Text batteryText;

    // GameObject 대신 캔버스의 Image 컴포넌트를 직접 연결합니다.
    [SerializeField] private Image jumpScareImage;
 
    public void UpdateBattery(float current, float max)
    {
        // 1. 배터리 텍스트 업데이트
        batteryText.text = "Battery : " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);

        // 2. 배터리가 20 이하일 때 선명도 조절 로직
        if (current <= 20f)
        {
            // 배터리가 20일 때 투명도 0 (안 보임), 0일 때 투명도 1 (완전 선명)
            // 역으로 계산하기 위해 (20 - 현재배터리) / 20 공식을 사용합니다.
            float alpha = (20f - current) / 20f;

            // 안전하게 0~1 사이로 값을 제한 (Clamp)
            alpha = Mathf.Clamp01(alpha);

            // 이미지 오브젝트가 꺼져있다면 켜줍니다.
            if (!jumpScareImage.gameObject.activeSelf)
            {
                jumpScareImage.gameObject.SetActive(true);

                AudioManager audioManager = FindAnyObjectByType<AudioManager>();
                if (audioManager != null)
                {
                    // ⚠️ AudioManager에 등록된 변수명 대소문자(Jumpscare2 또는 jumpscare2)를 꼭 맞춰주세요!
                    audioManager.PlaySFX(audioManager.Jumpscare2);
                }
            }

            // 이미지의 컬러(알파값) 변경
            Color color = jumpScareImage.color;
            color.a = alpha;
            jumpScareImage.color = color;
        }
        else
        {
            // 배터리가 20보다 많으면 이미지를 완전히 숨깁니다.
            if (jumpScareImage.gameObject.activeSelf)
            {
                jumpScareImage.gameObject.SetActive(false);
            }
        }
    }
}