using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    private int damage = 1;

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // 🛡️ [핵심 안전장치] 부딪히자마자 총알 자신의 콜라이더를 즉시 꺼버립니다!
            // 유니티가 Destroy를 처리하기 전 0.01초 찰나의 순간에 연사로 충돌하는 버그를 원천 차단합니다.
            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                myCollider.enabled = false;
            }

            // 1. 기존 에너미 스크립트(EnemyTraceController) 체크 및 대미지
            EnemyTraceController enemy1 = collision.GetComponent<EnemyTraceController>();
            if (enemy1 != null)
            {
                enemy1.TakeDamage(damage);
            }

            // 2. 새 에너미 스크립트(EnemyTrace2) 체크 및 대미지
            EnemyTrace2 enemy2 = collision.GetComponent<EnemyTrace2>();
            if (enemy2 != null)
            {
                enemy2.TakeDamage(damage);
            }

            // 총알 파괴
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}