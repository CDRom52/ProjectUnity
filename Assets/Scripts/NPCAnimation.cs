using UnityEngine;
using UnityEngine.AI;

public class NPCAnimation : MonoBehaviour
{
    private Animator anim;
    private Vector3 velocity;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        
    }
}