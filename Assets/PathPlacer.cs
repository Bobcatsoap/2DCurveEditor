using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPlacer : MonoBehaviour
{
    public float Spacing = .1f;
    public float Resolution = 1;

    // Use this for initialization
    void Start()
    {
        Vector2[] points = FindObjectOfType<PathCreator>().Path.CalculateEvenlySpacedPoints(Spacing, Resolution);
        foreach (Vector2 point in points)
        {
            Debug.LogError(point);

            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = point;
            g.transform.localScale = Vector3.one * Spacing * .5f;
        }
    }

   
}