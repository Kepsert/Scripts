using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    private Mesh mesh;
    private Vector3 origin;
    private float startingAngle;

    [SerializeField] float fov = 60;
    [SerializeField] int rayCount = 2;
    [SerializeField] float viewDistance = 6;

    private void Start()
    {
        this.transform.SetParent(null);

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Update()
    {
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex = origin + UtilsClass.GetVectorFromAngle(angle) * viewDistance;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, UtilsClass.GetVectorFromAngle(angle), viewDistance, layerMask);
			//If the ray does not hit an object, continue until view distance and set a vertex there.
            if (raycastHit2D.collider == null)
            {
                vertex = origin + UtilsClass.GetVectorFromAngle(angle) * viewDistance;
            }
			//If the ray hits an object, the vertex of the custom mesh is the hitpoint.
            else
            {
                vertex = raycastHit2D.point;
            }

			//Add the vertex to the vertices array.
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;

            angle -= angleIncrease;
        }

		//Draw a custom mesh based on the raycast-deduced vertices.
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection)
    {
        startingAngle = UtilsClass.GetAngleFromVectorFloat(aimDirection) - fov / 2f;
    }
}
