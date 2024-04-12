using UnityEngine;

public class Knight : MonoBehaviour
{
    public GameObject questionMark;
    private readonly float amplitude = 0.06f;
    private readonly float frequency = 1.7f;
    private float yStart;
    private float step;

    void Start()
    {
        yStart = questionMark.transform.position.y;
        step = 0.0f;
    }

    void Update()
    {
        step += frequency * Time.deltaTime;
        float x = questionMark.transform.position.x;
        float y = yStart + Mathf.Sin(step) * amplitude;
        float z = questionMark.transform.position.z;
        questionMark.transform.position = new Vector3(x, y, z);
    }
}
