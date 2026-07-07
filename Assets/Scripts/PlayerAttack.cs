using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Universal melee attack that works with any player controller.
/// Uses the "Attack" action from the Input System Player action map
/// and damages nearby SimpleEnemy instances via OverlapSphere.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 1;
    public float attackRange = 2f;
    public float attackOffset = 1f;
    public float attackCooldown = 0.5f;

    PlayerInput playerInput;
    InputAction attackAction;
    Camera playerCamera;
    float lastAttackTime = -999f;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerCamera = GetComponentInChildren<Camera>();
        BindAttackAction();
    }

    void BindAttackAction()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        var map = playerInput.actions.FindActionMap("Player");
        if (map != null)
        {
            attackAction = map.FindAction("Attack");
            attackAction?.Enable();
        }
        else
        {
            attackAction = playerInput.actions.FindAction("Attack");
            attackAction?.Enable();
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (attackAction != null && attackAction.WasPressedThisFrame())
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                PerformAttack();
            }
        }
    }

    void PerformAttack()
    {
        Vector3 attackOrigin;
        if (playerCamera != null)
            attackOrigin = playerCamera.transform.position + playerCamera.transform.forward * attackOffset;
        else
            attackOrigin = transform.position + transform.forward * attackOffset;

        Collider[] hits = Physics.OverlapSphere(attackOrigin, attackRange);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<SimpleEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }
}
