using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(PathCreator))]
public class RoadCreator : MonoBehaviour
{
    [Range(.05f, 1.5f)] public float spacing = 1;
    public float RoadWidth = 1;
    public bool autoUpdate;
    private bool isClosed;

    public void UpdateRoad()
    {
        Path path = FindObjectOfType<PathCreator>().Path;
        Vector2[] points = path.CalculateEvenlySpacedPoints(spacing);
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);
        //贴图修正
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale=new Vector2(1,points.Length/30);
    }

    Mesh CreateRoadMesh(Vector2[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        int[] tris = new int[2 * (points.Length - 1) * 3];
        Vector2[] uvs = new Vector2[verts.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 dir = Vector3.zero;
            //除了最后一个点每个点都有一个朝向下个点的方向
            if (i < points.Length - 1)
            {
                dir += points[i + 1] - points[i];
            }

            //除了最后一个点每个点都有一个上个点朝向这个点的方向
            if (i > 0)
            {
                dir += points[i] - points[i - 1];
            }

            //当点为中间点（除了开始和结束）时朝向应该为前后向量的中间量
            dir.Normalize();

            //B(x,y)，A 垂直于 B ，A（-y,x）
            Vector2 left = new Vector2(-dir.y, dir.x);
            Vector2 right = -left;
            verts[i * 2] = points[i] + left * RoadWidth * .5f;
            verts[i * 2 + 1] = points[i] + right * RoadWidth * .5f;
            uvs[i * 2] = new Vector2(0, i / (float) points.Length - 1);
            uvs[i * 2 + 1] = new Vector2(1, i / (float) points.Length - 1);
            if (i < points.Length - 1)
            {
                //每个点两个三角形
                tris[i * 6] = i * 2;
                tris[i * 6 + 1] = i * 2 + 2;
                tris[i * 6 + 2] = i * 2 + 1;

                tris[i * 6 + 3] = i * 2 + 1;
                tris[i * 6 + 4] = i * 2 + 2;
                tris[i * 6 + 5] = i * 2 + 3;
            }
        }

        //闭合时添加方块进行弥合
        if (isClosed)
        {
            int[] bridge = new int[9];

            bridge[0] = verts.Length - 2;
            bridge[1] = 0;
            bridge[2] = verts.Length - 1;

            bridge[3] = verts.Length - 1;
            bridge[4] = 0;
            bridge[5] = 1;

            int[] newTris = new int[bridge.Length + tris.Length];
            for (int i = 0; i < newTris.Length; i++)
            {
                if (i < tris.Length)
                {
                    newTris[i] = tris[i];
                    continue;
                }

                newTris[i] = bridge[i - tris.Length];
            }

            tris = newTris;
        }
        
        
        
        
        Mesh mesh = new Mesh
        {
            vertices = verts,
            triangles = tris,
            uv=uvs
        };
        
        

        return mesh;
    }
}