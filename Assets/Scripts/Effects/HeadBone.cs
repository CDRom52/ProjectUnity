using UnityEngine;

public class HeadBone : MonoBehaviour
{
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log($"Head collided with {collision.gameObject.name} on layer {collision.gameObject.layer}");
        // Debug.Log($"Head isKinematic: {rb.isKinematic}");
    }
}
