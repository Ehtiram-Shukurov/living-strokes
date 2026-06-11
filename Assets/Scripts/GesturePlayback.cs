using System.Collections.Generic;
using UnityEngine;

public class GesturePlayback : MonoBehaviour
{
    public void Init(List<Vector3> positions, bool waveMode)
    {
        if (positions == null || positions.Count < 2) return;

        Vector3 startPos = positions[0];
        Vector3 endPos = positions[positions.Count - 1];
        Vector3[] offsets = new Vector3[positions.Count];

        float maxDist = 0.01f;
        foreach (var p in positions)
        {
            float d = Vector3.Distance(startPos, p);
            if (d > maxDist) maxDist = d;
        }

        for (int i = 0; i < positions.Count; i++)
        {
            float t = (float)i / (positions.Count - 1);
            Vector3 baseline = Vector3.Lerp(startPos, endPos, t);

            Vector3 pureGesture = (positions[i] - baseline) / maxDist;

            offsets[i] = transform.InverseTransformVector(pureGesture);
        }

        Texture2D motionTex = new Texture2D(positions.Count, 1, TextureFormat.RGBAFloat, false);
        motionTex.wrapMode = TextureWrapMode.Repeat;
        motionTex.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < positions.Count; i++)
        {
            motionTex.SetPixel(i, 0, new Color(offsets[i].x, offsets[i].y, offsets[i].z, 1f));
        }
        motionTex.Apply();

        var filters = GetComponentsInChildren<MeshFilter>();
        foreach (var filter in filters)
        {
            Mesh mesh = filter.mesh;
            Vector3[] verts = mesh.vertices;
            Vector2[] uvs = new Vector2[verts.Length];
            int pairCount = verts.Length / 2;
            for (int i = 0; i < verts.Length; i++)
            {
                float t = (float)(i / 2) / (pairCount > 0 ? pairCount : 1);
                uvs[i] = new Vector2(t, 0f);
            }
            mesh.uv = uvs;
        }

        var renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            Material mat = new Material(r.material);
            mat.SetTexture("_MotionTex", motionTex);
            mat.SetFloat("_Speed", 0.2f);
            mat.SetFloat("_WaveDensity", waveMode ? 1.0f : 0.0f);
            mat.SetFloat("_Amplitude", 0.1f);
            r.material = mat;
        }
    }
    public void SetSpeed(float speed)
    {
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
            r.material.SetFloat("_Speed", speed);
    }
}