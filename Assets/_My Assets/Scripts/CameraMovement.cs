using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform follow;
    private readonly float lerpSpeed = 3.5f;

    void Start()
    {
        transform.position = new Vector3(follow.position.x, follow.position.y, transform.position.z);
    }

    void LateUpdate()
    {
        float x = Mathf.Lerp(transform.position.x, follow.position.x, lerpSpeed * Time.deltaTime);
        float y = Mathf.Lerp(transform.position.y, follow.position.y, lerpSpeed * Time.deltaTime);
        float z = transform.position.z;
        transform.position = new Vector3(x, y, z);
    }
}
