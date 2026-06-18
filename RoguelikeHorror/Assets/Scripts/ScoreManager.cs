using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    public TMP_Text keyText;
    [SerializeField] GameObject nextPanel;
    [SerializeField] GameObject jumpScarePanel; // 점프스케어 패널 드래그앤드롭
    private int keyCount = 0;

    public void AddCoin()
    {
        keyCount++;
        keyText.text = "Key : " + keyCount.ToString() + " / 5";

        if (keyCount == 4)
        {
            StartCoroutine(JumpScare());
        }

        if (keyCount >= 5)
        {
            nextPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            //FindAnyObjectByType<StarterAssets.ThirdPersonController>().enabled = false;
        }
    }

    IEnumerator JumpScare()
    {
        jumpScarePanel.SetActive(true);  // 점프스케어 사진 표시
        FindAnyObjectByType<AudioManager>().PlaySFX(FindAnyObjectByType<AudioManager>().Jumpscare);
        yield return new WaitForSeconds(2f); // 2초 후 사라짐
        jumpScarePanel.SetActive(false);
    }
}