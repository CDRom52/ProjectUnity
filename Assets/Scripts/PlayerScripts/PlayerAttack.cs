using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    private PlayerController player;
    private Animator animator;

    [Header("Punch Settings")]
    public float punchRange = 2.5f;
    public float knockbackForce = 25f;
    public LayerMask npcLayerMask;
    private int upperBodyLayerIndex;
    public string punchTriggerName = "Punch";
    public string upperBodyLayerName = "UpperBody";
    public string rightPunchStateName = "PunchR";
    public string leftPunchStateName = "PunchL";

    [Header("Timing")]
    public float hitDelay = 0.2f;

    void Start()
    {
        player = GetComponent<PlayerController>();
        animator = GetComponentInChildren<Animator>();
        upperBodyLayerIndex = animator.GetLayerIndex("UpperBody");
    }

    public void Attack()
    {
        if (animator.IsInTransition(upperBodyLayerIndex))
            return;

        animator.SetTrigger("Punch");
        player.punchLeft = !player.punchLeft;

        StartCoroutine(ExecutePunchHit(hitDelay));
    }

    private IEnumerator ExecutePunchHit(float delay)
    {
        yield return new WaitForSeconds(delay);

        Vector3 punchOrigin = transform.position + transform.forward * (punchRange * 0.5f);
        Collider[] hitColliders = Physics.OverlapSphere(punchOrigin, punchRange, npcLayerMask);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<NPCController>(out NPCController npc))
            {
                Vector3 knockbackDir = (hitCollider.transform.position - transform.position).normalized;
                npc.Impact(knockbackDir, knockbackForce);
            }
        }
    }
}
