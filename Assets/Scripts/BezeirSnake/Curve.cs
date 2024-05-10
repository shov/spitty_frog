using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve : MonoBehaviour
{
    public delegate void OnMeetFiredBallHandler(Collider ball);

    public event OnMeetFiredBallHandler OnMeetFiredBall;

    protected GameObject generatedColliderMesh;

    private void Start()
    {
        Mesh mesh = GenerateMeshAlongCurve();
        generatedColliderMesh = new GameObject("MeshColliderObject");
        MeshCollider meshCollider = generatedColliderMesh.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;
        //meshCollider.isTrigger = true; n need, ball has trigger
        generatedColliderMesh.transform.SetParent(transform);
        generatedColliderMesh.transform.localPosition = Vector3.zero;
        generatedColliderMesh.transform.localRotation = Quaternion.identity;
        Rigidbody rb = generatedColliderMesh.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        CurveGenMeshCollider cgmCollider = generatedColliderMesh.AddComponent<CurveGenMeshCollider>();
        cgmCollider.OnMeetFiredBall += PassTriggerEnter;
    }

    public void PassTriggerEnter(Collider other)
    {
        // Fire the event
        OnMeetFiredBall?.Invoke(other);
    }

    // Gen Mesh along the curve

    public int resolution = 8; // Number of vertices per circle
    public float radius = 0.5f; // Radius of the tube

    private Mesh GenerateMeshAlongCurve()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        Vector3[] points = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(points);

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 forward = (points[i + 1] - points[i]).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            for (int j = 0; j < resolution; j++)
            {
                float angle = j * Mathf.PI * 2 / resolution;
                Vector3 vertex = points[i] + right * Mathf.Cos(angle) * radius + up * Mathf.Sin(angle) * radius;
                vertices.Add(vertex);
            }
        }

        for (int i = 0; i < points.Length - 2; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int current = j + i * resolution;
                int next = (j + 1) % resolution + i * resolution;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + resolution);

                triangles.Add(current + resolution);
                triangles.Add(next);
                triangles.Add(next + resolution);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); // To ensure the mesh is shaded correctly

        return mesh;
    }
}
