
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(MeshFilter))]
public class BreakableTargetController : MonoBehaviour, IAttackTarget
{
    public bool IsValid => this != null && enabled;

    [SerializeField] float _explosionForce = 1.0f;

    [SerializeField] float _explosionRadius = 1.0f;


    Rigidbody _rigidbody;
    MeshFilter _meshFilter;
    Renderer _renderer;

    void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _rigidbody = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();
    }

    void IAttackTarget.OnAttackHit(int damage)
    {
        if(!IsValid)
        {
            return;
        }
        BreakIntoPieces();
        Destroy(gameObject);
    }

    void BreakIntoPieces()
    {
        var mesh = _meshFilter.mesh;
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var triangles = mesh.triangles;
        Vector2[] uvs = mesh.uv;
        int index = 0;

        // get each face
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var averageNormal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]).normalized;
            var s = _renderer.bounds.size;
            var extrudeSize = ((s.x + s.y + s.z) / 3) * 0.3f;
            CreateMeshPiece(extrudeSize, index, averageNormal, vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]], uvs[triangles[i]], uvs[triangles[i + 1]], uvs[triangles[i + 2]]);
            index++;
        }
    }

    GameObject CreateMeshPiece(float extrudeSize, int index, Vector3 faceNormal, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        var go = new GameObject($"{name} {index}");

        Mesh mesh = go.AddComponent<MeshFilter>().mesh;
        go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = _renderer.material;
        go.transform.position = transform.position;
        go.layer = gameObject.layer;

        var vertices = new Vector3[3 * 4];
        var triangles = new int[3 * 4];
        var uvs = new Vector2[3 * 4];

        // get centroid
        var v4 = (v1 + v2 + v3) / 3;
        // extend to backwards
        v4 += -faceNormal * extrudeSize;

        // not shared vertices
        // orig face
        vertices[0] = (v1);
        vertices[1] = (v2);
        vertices[2] = (v3);
        // right face
        vertices[3] = (v1);
        vertices[4] = (v2);
        vertices[5] = (v4);
        // left face
        vertices[6] = (v1);
        vertices[7] = (v3);
        vertices[8] = (v4);
        // bottom face
        vertices[9] = (v2);
        vertices[10] = (v3);
        vertices[11] = (v4);
        // orig face
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        // right face
        triangles[3] = 5;
        triangles[4] = 4;
        triangles[5] = 3;
        // left face
        triangles[6] = 6;
        triangles[7] = 7;
        triangles[8] = 8;
        // bottom face
        triangles[9] = 11;
        triangles[10] = 10;
        triangles[11] = 9;
        // orig face
        uvs[0] = uv1;
        uvs[1] = uv2;
        uvs[2] = uv3;
        // right face
        uvs[3] = uv1;
        uvs[4] = uv2;
        uvs[5] = uv3; 
        // left face
        uvs[6] = uv1;
        uvs[7] = uv3;
        uvs[8] = uv3;
        uvs[9] = uv1;
        uvs[10] = uv2;
        uvs[11] = uv1;

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        CalculateMeshTangents(mesh);

        var rb = go.AddComponent<Rigidbody>();
        rb.AddExplosionForce(_explosionForce, v4, _explosionRadius);

        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        mc.convex = true;

        return go;
    }

    // source: http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html
    void CalculateMeshTangents(Mesh mesh)
    {
        //speed up math by copying the mesh arrays
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;
        var uv = mesh.uv;
        var normals = mesh.normals;

        //variable definitions
        var triangleCount = triangles.Length;
        var vertexCount = vertices.Length;

        var tan1 = new Vector3[vertexCount];
        var tan2 = new Vector3[vertexCount];

        var tangents = new Vector4[vertexCount];

        for (var a = 0; a < triangleCount; a += 3)
        {
            var i1 = triangles[a + 0];
            var i2 = triangles[a + 1];
            var i3 = triangles[a + 2];

            var v1 = vertices[i1];
            var v2 = vertices[i2];
            var v3 = vertices[i3];

            var w1 = uv[i1];
            var w2 = uv[i2];
            var w3 = uv[i3];

            var x1 = v2.x - v1.x;
            var x2 = v3.x - v1.x;
            var y1 = v2.y - v1.y;
            var y2 = v3.y - v1.y;
            var z1 = v2.z - v1.z;
            var z2 = v3.z - v1.z;

            var s1 = w2.x - w1.x;
            var s2 = w3.x - w1.x;
            var t1 = w2.y - w1.y;
            var t2 = w3.y - w1.y;

            var r = 1.0f / (s1 * t2 - s2 * t1);

            var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;

            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
        }

        for (int a = 0; a < vertexCount; ++a)
        {
            var n = normals[a];
            var t = tan1[a];
            Vector3.OrthoNormalize(ref n, ref t);
            tangents[a] = new Vector4(t.x, t.y, t.z, (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f);
        }
        mesh.tangents = tangents;
    }
}
