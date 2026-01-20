using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/UI Segments Border Graphic")]
public class UISegmentsBorderGraphic : MaskableGraphic
{
    public struct Segment
    {
        public Vector2 a;
        public Vector2 b;
        public Vector2 outwardNormal; // 单位向量，指向“外侧”
        public Segment(Vector2 a, Vector2 b, Vector2 outwardNormal)
        {
            this.a = a;
            this.b = b;
            this.outwardNormal = outwardNormal;
        }
    }

    [SerializeField] private float _thickness = 6f;

    private readonly List<Segment> _segments = new List<Segment>();

    public float Thickness
    {
        get => _thickness;
        set { _thickness = Mathf.Max(0.1f, value); SetVerticesDirty(); }
    }

    public void SetSegments(IReadOnlyList<Segment> segs)
    {
        _segments.Clear();
        if (segs != null) _segments.AddRange(segs);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_segments.Count == 0 || _thickness <= 0f) return;

        float half = _thickness * 0.5f;

        for (int i = 0; i < _segments.Count; i++)
        {
            var s = _segments[i];

            Vector2 dir = s.b - s.a;
            float len = dir.magnitude;
            if (len < 0.0001f) continue;
            dir /= len;

            // 线段的“左右法线”（用于挤出厚度）
            Vector2 side = new Vector2(-dir.y, dir.x);

            // 把整条边段沿“外法线”偏移 half：保证描边在外侧
            Vector2 offset = s.outwardNormal.normalized * half;

            // 再沿side方向挤出 half：形成厚度
            Vector2 n = side * half;

            Vector2 a = s.a + offset;
            Vector2 b = s.b + offset;

            Vector2 v0 = a - n;
            Vector2 v1 = a + n;
            Vector2 v2 = b + n;
            Vector2 v3 = b - n;

            AddQuad(vh, v0, v1, v2, v3, color);
        }
    }

    private static void AddQuad(VertexHelper vh, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color32 col)
    {
        int start = vh.currentVertCount;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = col;

        vert.position = v0; vh.AddVert(vert);
        vert.position = v1; vh.AddVert(vert);
        vert.position = v2; vh.AddVert(vert);
        vert.position = v3; vh.AddVert(vert);

        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 0, start + 2, start + 3);
    }
}
