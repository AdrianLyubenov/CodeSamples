using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class MeshUtils
{
    public static Vector3 Clamp(this Vector3 v, float min, float max)
    {
        var x = Mathf.Clamp(v.x, min, max);
        var y = Mathf.Clamp(v.y, min, max);
        var z = Mathf.Clamp(v.z, min, max);

        return new Vector3(x, y, z);
    }

    public static Vector2 toVector2xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 toVector2xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector2 toVector2zy(this Vector3 v)
    {
        return new Vector2(v.z, v.y);
    }

    public static Mesh Clone(this Mesh mesh, string suffix = "Clone", bool overrideIndexFormat = false, IndexFormat indexFormat = IndexFormat.UInt16)
    {
        Mesh newmesh = new Mesh();
        newmesh.indexFormat = overrideIndexFormat ? indexFormat : mesh.indexFormat;
        newmesh.name        = mesh.name + " - " + suffix;
        newmesh.vertices    = mesh.vertices;
        newmesh.triangles   = mesh.triangles;
        newmesh.uv          = mesh.uv;
        newmesh.normals     = mesh.normals;
        newmesh.colors      = mesh.colors;
        newmesh.tangents    = mesh.tangents;

        return newmesh;
    }

    public static void Save(this Mesh mesh, String path, bool optimizeMesh)
    {
        if (string.IsNullOrEmpty(path)) return;

        if (optimizeMesh)
            MeshUtility.Optimize(mesh);

        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }

    public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }

        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);
    }
    
    public static Mesh ScaleMesh(this Mesh mesh, Vector3 amount, bool overwrite = false)
    {
        var vertices = mesh.vertices;

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= amount.x;
            vertices[i].y *= amount.y;
            vertices[i].z *= amount.z;
        }

        if (overwrite)
        {
            mesh.vertices = vertices;
            return mesh;
        }

        Mesh newMesh = mesh.Clone(" - Scaled");
        newMesh.vertices = vertices;
        return newMesh;
    }


    public static void RecalculateNormals(this Mesh mesh, float angle)
    {
        var angleThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);
        var vertices       = mesh.vertices;
        var normals        = new Vector3[vertices.Length];

        var dict = new Dictionary<Vector3, List<TriVertPair>>(vertices.Length);

        var triangles   = mesh.triangles;
        var faceNormals = new Vector3[triangles.Length / 3];

        int index = 0;
        for (var i = 0; i < triangles.Length; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            Vector3 d1     = vertices[v2] - vertices[v1];
            Vector3 d2     = vertices[v3] - vertices[v1];
            Vector3 normal = Vector3.Cross(d1, d2).normalized;

            faceNormals[index] = normal;

            List<TriVertPair> entry;
            Vector3           key;

            if (!dict.TryGetValue(key = vertices[v1], out entry))
            {
                entry = new List<TriVertPair>(4);
                dict.Add(key, entry);
            }

            entry.Add(new TriVertPair(index, v1));

            if (!dict.TryGetValue(key = vertices[v2], out entry))
            {
                entry = new List<TriVertPair>();
                dict.Add(key, entry);
            }

            entry.Add(new TriVertPair(index, v2));

            if (!dict.TryGetValue(key = vertices[v3], out entry))
            {
                entry = new List<TriVertPair>();
                dict.Add(key, entry);
            }

            entry.Add(new TriVertPair(index, v3));

            index++;
        }

        foreach (var triVertPairs in dict.Values)
        {
            foreach (var p1 in triVertPairs)
            {
                var normal = Vector3.zero;

                foreach (var p2 in triVertPairs)
                {
                    if (p1.VertIndex == p2.VertIndex)
                    {
                        normal += faceNormals[p2.TriIndex];
                    }
                    else
                    {
                        var dot = Vector3.Dot(
                            faceNormals[p1.TriIndex],
                            faceNormals[p2.TriIndex]);
                        if (dot >= angleThreshold)
                        {
                            normal += faceNormals[p2.TriIndex];
                        }
                    }
                }

                normals[p1.VertIndex] = normal.normalized;
            }
        }

        mesh.normals = normals;
    }


    private readonly struct TriVertPair
    {
        public readonly int TriIndex;
        public readonly int VertIndex;

        public TriVertPair(int triIndex, int vertIndex)
        {
            this.TriIndex  = triIndex;
            this.VertIndex = vertIndex;
        }
    }

    public static class MeshHelper
    {
        static List<Vector3> vertices;
        static List<Vector3> normals;
        static List<Color>   colors;
        static List<Vector2> uv;
        static List<Vector2> uv2;
        static List<Vector2> uv3;

        static List<int>             indices;
        static Dictionary<uint, int> newVertices;

        static void InitArrays(Mesh mesh)
        {
            vertices = new List<Vector3>(mesh.vertices);
            normals  = new List<Vector3>(mesh.normals);
            colors   = new List<Color>(mesh.colors);
            uv       = new List<Vector2>(mesh.uv);
            uv2      = new List<Vector2>(mesh.uv2);
            uv3      = new List<Vector2>(mesh.uv3);
            indices  = new List<int>(mesh.vertices.Length * 10);
        }

        static void CleanUp()
        {
            vertices = null;
            normals  = null;
            colors   = null;
            uv       = null;
            uv2      = null;
            uv3      = null;
            indices  = null;
        }

        #region Subdivide4 (2x2)

        static int GetNewVertex4(int i1, int i2)
        {
            int  newIndex = vertices.Count;
            uint t1       = ((uint) i1 << 16) | (uint) i2;
            uint t2       = ((uint) i2 << 16) | (uint) i1;
            if (newVertices.ContainsKey(t2))
                return newVertices[t2];
            if (newVertices.ContainsKey(t1))
                return newVertices[t1];

            newVertices.Add(t1, newIndex);

            vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);
            if (normals.Count > 0)
                normals.Add((normals[i1] + normals[i2]).normalized);
            if (colors.Count > 0)
                colors.Add((colors[i1] + colors[i2]) * 0.5f);
            if (uv.Count > 0)
                uv.Add((uv[i1] + uv[i2]) * 0.5f);
            if (uv2.Count > 0)
                uv2.Add((uv2[i1] + uv2[i2]) * 0.5f);
            if (uv3.Count > 0)
                uv3.Add((uv3[i1] + uv3[i2]) * 0.5f);

            return newIndex;
        }

        public static Mesh Subdivide4(Mesh mesh)
        {
            newVertices = new Dictionary<uint, int>();


            InitArrays(mesh);
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int i1 = triangles[i + 0];
                int i2 = triangles[i + 1];
                int i3 = triangles[i + 2];

                int a = GetNewVertex4(i1, i2);
                int b = GetNewVertex4(i2, i3);
                int c = GetNewVertex4(i3, i1);
                indices.Add(i1);
                indices.Add(a);
                indices.Add(c);
                indices.Add(i2);
                indices.Add(b);
                indices.Add(a);
                indices.Add(i3);
                indices.Add(c);
                indices.Add(b);
                indices.Add(a);
                indices.Add(b);
                indices.Add(c); // center triangle
            }

            //  Debug.Log("Vertices length = " + vertices.Count);

            mesh.vertices = vertices.ToArray();
            if (normals.Count > 0)
                mesh.normals = normals.ToArray();
            if (colors.Count > 0)
                mesh.colors = colors.ToArray();
            if (uv.Count > 0)
                mesh.uv = uv.ToArray();
            if (uv2.Count > 0)
                mesh.uv2 = uv2.ToArray();
            if (uv3.Count > 0)
                mesh.uv3 = uv3.ToArray();

            mesh.triangles = indices.ToArray();

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();


            CleanUp();
            return mesh;
        }

        #endregion Subdivide4 (2x2)
    }

    #region Subdivide

    public static void Subdivide(this Mesh mesh, int level)
    {
        if (level < 2)
            return;
        while (level > 1)
        {
            while (level % 2 == 0)
            {
                if (mesh.vertexCount > 200000)
                {
                    Debug.LogWarning("Vertex count too high, cancelling subdivision");
                    return;
                }

                MeshHelper.Subdivide4(mesh);
                level /= 2;
            }
        }
    }

    #endregion Subdivide

    public static void StoreLocalYPosInColor(this Mesh mesh)
    {
        // Add local height info to colors
        var     vertices = mesh.vertices;
        Color[] colors   = new Color[vertices.Length];
        var     boundsY  = mesh.bounds.max.y;

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i].a = ((vertices[i].y + boundsY) / 2) / boundsY;
        }

        mesh.colors = colors;
    }
}