
using UnityEngine;

public class RailMovement : MonoBehaviour
{
    public float speed = 5f;
    public float segmentLength = 10f;

    public Transform segment1;
    public Transform segment2;

    void Update()
    {
        MoveSegment(segment1);
        MoveSegment(segment2);
    }

    void MoveSegment(Transform segment)
    {
        segment.position += Vector3.left * speed * Time.deltaTime;

        if (segment.position.x <= -segmentLength)
        {
            segment.position += Vector3.right * segmentLength * 2f;
        }
    }
}
