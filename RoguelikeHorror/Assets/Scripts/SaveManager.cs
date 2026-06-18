using System.IO;
using UnityEngine;

// 1. JSON으로 변환할 데이터 상자 (유니티 인스펙터 및 직렬화를 위해 Serializable 필수!)
[System.Serializable]
public class PlayerSaveData
{
    public int damage = 1;          // 강화된 공격력
    public float maxBattery = 100f; // 강화된 최대 배터리
    public int maxXp = 100;         // 늘어난 다음 레벨업 경험치
}

public class SaveManager : MonoBehaviour
{
    private string saveFilePath;

    private void Awake()
    {
        // 각 OS(PC, 모바일 등)에 맞는 안전한 저장 경로 설정 (파일 이름: savefile.json)
        saveFilePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    // 💾 데이터를 JSON 파일로 저장하는 함수
    public void SaveGame(int currentDamage, float currentMaxBattery, int currentMaxXp)
    {
        PlayerSaveData data = new PlayerSaveData();
        data.damage = currentDamage;
        data.maxBattery = currentMaxBattery;
        data.maxXp = currentMaxXp;

        // 클래스 객체를 이쁜 JSON 문자열로 변환 (true를 넣으면 가독성 좋게 정렬됨)
        string json = JsonUtility.ToJson(data, true);

        // 파일로 쓰기
        File.WriteAllText(saveFilePath, json);
        Debug.Log("게임 데이터가 JSON으로 저장되었습니다! 경로: " + saveFilePath);
    }

    // 📂 JSON 파일을 읽어서 데이터를 불러오는 함수
    public PlayerSaveData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            // 파일이 존재하면 내용을 읽어옴
            string json = File.ReadAllText(saveFilePath);

            // JSON 문자열을 다시 원래 클래스 형태로 역직렬화
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);
            Debug.Log("게임 데이터를 JSON으로부터 불러왔습니다.");
            return data;
        }
        else
        {
            Debug.Log("저장된 파일이 없습니다. 기본값으로 시작합니다.");
            return null; // 저장 파일이 없으면 null 반환
        }
    }

    // 🧹 죽거나 아예 초기화하고 싶을 때 세이브 파일을 지우는 함수
    public void ClearSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("세이브 파일이 삭제되었습니다.");
        }
    }

    [ContextMenu("Reset Save Data")]
    public void ResetSaveData()
    {
        ClearSaveFile();
        Debug.Log("세이브 파일이 삭제되었습니다. 게임을 다시 시작하면 초기화됩니다.");
    }
}