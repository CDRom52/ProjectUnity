using UnityEngine;

public class FloatingOrigin : MonoBehaviour
{
    public float threshold = 1000f;
    public GameObject worldContainer;

    private CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void LateUpdate()
    {
        Vector3 playerPos = transform.position;

        if (playerPos.magnitude > threshold)
        {
            ShiftWorld(playerPos);
        }
    }

    void ShiftWorld(Vector3 offset)
    {
        foreach (Transform child in worldContainer.transform)
        {
            child.position -= offset;
        }

        cc.enabled = false;
        transform.position = Vector3.zero;
        cc.enabled = true;

        Debug.Log("World shifted by: " + offset);
    }
}