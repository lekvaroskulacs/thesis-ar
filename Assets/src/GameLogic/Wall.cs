using UnityEngine;

class Wall : MonoBehaviour
{
    public GameObject start { get; set; }
    public GameObject end { get; set; }

    void Update()
    {
        if (start == null || end == null)
        {
            return;
        }

        Vector3 startPos = start.transform.position;
        Vector3 endPos = end.transform.position;
        Vector3 direction = endPos - startPos;

        transform.position = (startPos + endPos) / 2f;

        transform.rotation = Quaternion.LookRotation(direction);

        Vector3 scale = transform.localScale;
        scale.z = direction.magnitude;
        transform.localScale = scale;
    }
}