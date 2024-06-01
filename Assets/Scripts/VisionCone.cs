using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    public Material VisionConeMaterial;
    public float VisionRange = 10f;
    public float VisionAngle = 45f;
    public LayerMask VisionObstructingLayer; // Layer with objects that obstruct the enemy view, like walls
    public int VisionConeResolution = 120; // The vision cone will be made up of triangles

    private Mesh visionConeMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = VisionConeMaterial;

        meshFilter = gameObject.AddComponent<MeshFilter>();
        visionConeMesh = new Mesh();
        meshFilter.mesh = visionConeMesh;

        VisionAngle *= Mathf.Deg2Rad;
    }

    void Update()
    {
        DrawVisionCone();
    }

    void DrawVisionCone()
    {
        int[] triangles = new int[(VisionConeResolution - 1) * 3];
        Vector3[] vertices = new Vector3[VisionConeResolution + 1];
        vertices[0] = Vector3.zero;

        float currentAngle = -VisionAngle / 2;
        float angleIncrement = VisionAngle / (VisionConeResolution - 1);

        for (int i = 0; i < VisionConeResolution; i++)
        {
            float sine = Mathf.Sin(currentAngle);
            float cosine = Mathf.Cos(currentAngle);
            Vector3 raycastDirection = (transform.forward * cosine) + (transform.right * sine);
            Vector3 vertexDirection = (Vector3.forward * cosine) + (Vector3.right * sine);

            if (Physics.Raycast(transform.position, raycastDirection, out RaycastHit hit, VisionRange, VisionObstructingLayer))
            {
                vertices[i + 1] = vertexDirection * hit.distance;
            }
            else
            {
                vertices[i + 1] = vertexDirection * VisionRange;
            }

            currentAngle += angleIncrement;
        }

        for (int i = 0, j = 0; i < triangles.Length; i += 3, j++)
        {
            triangles[i] = 0;
            triangles[i + 1] = j + 1;
            triangles[i + 2] = j + 2;
        }

        visionConeMesh.Clear();
        visionConeMesh.vertices = vertices;
        visionConeMesh.triangles = triangles;
        visionConeMesh.RecalculateNormals();
    }
}
