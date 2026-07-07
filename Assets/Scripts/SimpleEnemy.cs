using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy that chases the player and damages them on contact.
/// Has its own HP so the player can fight back and destroy it.
/// </summary>
public class SimpleEnemy : MonoBehaviour
{
    [Header("Contact Damage")]
    public bool defeatPlayerOnTouch = true;
    public float contactRadius = 1.1f;
    public float hitCooldown = 1.5f;

    [Header("Chase")]
    public bool enableChase = true;
    public float chaseSpeed = 3.5f;
    public float chaseRange = 15f;

    [Header("Enemy Health")]
    public int enemyMaxHP = 3;

    int enemyCurrentHP;
    Transform playerRoot;
    float lastHitTime = -999f;
    Renderer cachedRenderer;
    Color originalColor;
    Coroutine flashCoroutine;
    bool isDead;

    public AudioSource Yell; // Reference to the AudioSource component for the hit sound

    void Start()
    {
        enemyCurrentHP = enemyMaxHP;
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer != null)
            originalColor = cachedRenderer.material.color;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            playerRoot = p.transform;
    }

    void Update()
    {   
        if (isDead || GameManager.Instance == null || GameManager.Instance.IsGameOver || GameManager.Instance.GameNotStarted)
            return;

        if (playerRoot == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                playerRoot = p.transform;
            if (playerRoot == null)
                return;
        }

        Vector3 offset = playerRoot.position - transform.position;
        float sqrDist = offset.sqrMagnitude;

        if (enableChase && sqrDist < chaseRange * chaseRange && sqrDist > contactRadius * contactRadius)
        {
            Vector3 dir = offset;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                dir.Normalize();
                transform.position += dir * chaseSpeed * Time.deltaTime;
                transform.forward = dir;
            }
        }

        if (defeatPlayerOnTouch && sqrDist < contactRadius * contactRadius)
        {
            if (Time.time - lastHitTime >= hitCooldown)
            {
                lastHitTime = Time.time;
                GameManager.Instance.PlayerHitByEnemy();
                if (Yell != null)
                    Yell.Play();
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        enemyCurrentHP -= amount;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlash());

        if (enemyCurrentHP <= 0)
        {
            enemyCurrentHP = 0;
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Destroy(gameObject, 0.1f);
    }

    IEnumerator HitFlash()
    {
        if (cachedRenderer == null) yield break;

        cachedRenderer.material.color = Color.white;
        yield return new WaitForSeconds(0.12f);
        if (cachedRenderer != null)
            cachedRenderer.material.color = originalColor;
        flashCoroutine = null;
    }
}
