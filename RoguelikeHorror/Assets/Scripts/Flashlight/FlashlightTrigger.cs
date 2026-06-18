using UnityEngine;

public class FlashlightTrigger : MonoBehaviour
{
    // 이 비어있는 함수가 여기에 적혀있는 것만으로도, 
    // 손전등(Triangle)에 닿는 적의 충돌 신호는 부모(Player)에게 전달되지 않고 여기서 흡수되어 소멸합니다!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 비워두셔도 됩니다.
    }
}