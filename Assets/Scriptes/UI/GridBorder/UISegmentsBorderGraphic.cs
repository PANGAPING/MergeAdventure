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
        public Vector2 outwardNormal; // 单位向量，指向外侧（left/right/up/down）

        public Segment(Vector2 a, Vector2 b, Vector2 outwardNormal)
        {
            this.a = a;
            this.b = b;
            this.outwardNormal = outwardNormal;
        }
    }

    /// <summary>
    /// 外凸拐角的圆角补片：一个四分之一圆盘（扇形）
    /// center = 角点（可额外沿角平分线推一点 epsilon）
    /// radius = thickness（注意不是 half）
    /// start/end angle 用于定义扇区方向（通常 90 度）
    /// </summary>
    public struct Join
    {
        public Vector2 center;
        public float radius;
        public float startAngleDeg;
        public float endAngleDeg;

        public Join(Vector2 center, float radius, float startAngleDeg, float endAngleDeg)
        {
            this.center = center;
            this.radius = radius;
            this.startAngleDeg = startAngleDeg;
            this.endAngleDeg = endAngleDeg;
        }
    }

    [SerializeField] private float _thickness = 6f;
    [SerializeField, Range(2, 64)] private int _arcSteps = 12;

    private readonly List<Segment> _segments = new List<Segment>();
    private readonly List<Join> _joins = new List<Join>();

    public float Thickness
    {
        get => _thickness;
        set { _thickness = Mathf.Max(0.1f, value); SetVerticesDirty(); }
    }

    public int ArcSteps
    {
        get => _arcSteps;
        set { _arcSteps = Mathf.Clamp(value, 2, 64); SetVerticesDirty(); }
    }

    public void SetGeometry(IReadOnlyList<Segment> segs, IReadOnlyList<Join> joins)
    {
        _segments.Clear();
        _joins.Clear();
        if (segs != null) _segments.AddRange(segs);
        if (joins != null) _joins.AddRange(joins);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if ((_segments.Count == 0 && _joins.Count == 0) || _thickness <= 0f) return;

        float half = _thickness * 0.5f;

        // 1) 画所有暴露边：外侧偏移 half + 垂直方向挤出 half
        for (int i = 0; i < _segments.Count; i++)
        {
            var s = _segments[i];

            Vector2 dir = s.b - s.a;
            float len = dir.magnitude;
            if (len < 0.0001f) continue;
            dir /= len;

            // 线段垂直方向（用于厚度挤出）
            Vector2 side = new Vector2(-dir.y, dir.x);

            // 将“中心线”整体往外偏移 half，确保描边全在外侧
            Vector2 offset = s.outwardNormal.normalized * half;

            // 厚度挤出
            Vector2 n = side * half;

            Vector2 a = s.a + offset;
            Vector2 b = s.b + offset;

            Vector2 v0 = a - n;
            Vector2 v1 = a + n;
            Vector2 v2 = b + n;
            Vector2 v3 = b - n;

            AddQuad(vh, v0, v1, v2, v3, color);
        }

        // 2) 画外凸角的四分之一圆盘，半径=thickness
        for (int i = 0; i < _joins.Count; i++)
        {
            var j = _joins[i];
            AddArcFanShort(vh, j.center, j.radius, j.startAngleDeg, j.endAngleDeg, _arcSteps, color);
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

    /// <summary>
    /// 画“短弧”的扇形（避免走 270 度）
    /// </summary>
    private static void AddArcFanShort(VertexHelper vh, Vector2 center, float radius, float startDeg, float endDeg, int steps, Color32 col)
    {
        float delta = DeltaAnglePositive(startDeg, endDeg);
        if (delta > 180f)
        {
            // 交换方向，确保走短弧
            float tmp = startDeg; startDeg = endDeg; endDeg = tmp;
            delta = DeltaAnglePositive(startDeg, endDeg);
        }

        int start = vh.currentVertCount;

        UIVertex v = UIVertex.simpleVert;
        v.color = col;

        // 中心点
        v.position = center;
        vh.AddVert(v);

        int arcPoints = Mathf.Max(2, steps + 1);
        for (int i = 0; i < arcPoints; i++)
        {
            float t = i / (float)(arcPoints - 1);
            float ang = startDeg + delta * t;
            float rad = ang * Mathf.Deg2Rad;

            Vector2 p = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
            v.position = p;
            vh.AddVert(v);
        }

        // 三角扇
        for (int i = 0; i < arcPoints - 1; i++)
        {
            vh.AddTriangle(start, start + i + 1, start + i + 2);
        }
    }

    private static float DeltaAnglePositive(float from, float to)
    {
        float d = (to - from) % 360f;
        if (d < 0) d += 360f;
        return d;
    }
}
