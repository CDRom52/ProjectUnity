using UnityEngine;
using UnityEngine.AI;

public class RagdollBone : MonoBehaviour
{
    private NPCController npc;

    void Start()
    {
        npc = GetComponentInParent<NPCController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (npc == null) return;
        npc.OnBoneCollision(collision);
    }
}