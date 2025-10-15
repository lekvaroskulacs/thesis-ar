using System;
using UnityEngine;

public class FloatyEffect : MonoBehaviour
{
    [SerializeField][Range(0.0f, 1.0f)] private float displacementAmplitude;
    [SerializeField] private float displacementSpeed;

    void Update()
    {
        var displacement = Mathf.Sin(Time.time * displacementSpeed) * displacementAmplitude * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, transform.position.y + displacement, transform.position.z);
    }
}
