using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    /*
     *三次贝塞尔曲线，每条曲线由四个点构造，其中两个点是锚点，即起始点、终结点，两个是控制点
     * 
     */
    [SerializeField, HideInInspector] List<Vector2> points;
    [SerializeField, HideInInspector] bool isClosed;
    [SerializeField, HideInInspector] private bool autoSetControlPoints;

    public bool AutoSetControlPoints
    {
        get { return autoSetControlPoints; }
        set
        {
            autoSetControlPoints = value;
            if (autoSetControlPoints)
            {
                AutoSetAllControlPoint();
            }
        }
    }


    public Path(Vector2 center)
    {
        points = new List<Vector2>
        {
            //以给定点为中心初始化四个点
            center + Vector2.left,
            center + (Vector2.left + Vector2.up) * .5f,
            center + (Vector2.right + Vector2.down) * .5f,
            center + Vector2.right
        };
    }

    //索引器
    public Vector2 this[int i]
    {
        get { return points[i]; }
    }

    public void AddSegment(Vector2 anchorPos)
    {
        //每次添加新的锚点都会产生两个新的控制点，一共三个点，这三个点与之前的锚点共同构成产生曲线的四个点
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) / 2);
        points.Add(anchorPos);
        if (autoSetControlPoints)
        {
            AutoSetAllEffectPoint(points.Count - 1);
        }
    }

    public void SplitSegment(int segmentIndex, Vector2 anchorPosition)
    {
        int anchorIndex = segmentIndex * 3 + 3;
        Vector2[] insertPoints = new Vector2[]
        {
            anchorPosition,
            anchorPosition,
            anchorPosition
        };
        points.InsertRange(anchorIndex - 1, insertPoints);
        if (autoSetControlPoints)
        {
            AutoSetAllEffectPoint(anchorIndex);
        }
        else
        {
            AutoSetAnchorControlPoint(anchorIndex);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || !isClosed && NumSegments > 1)
        {
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    //获取最后一个点的位置
                    Vector2 lastPointPos = points[2];
                    points.RemoveRange(0, 3);
                    points[points.Count - 1] = lastPointPos;
                }
                else
                {
                    points.RemoveRange(0, 3);
                }
            }
            else if (anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(points.Count - 3, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector2[] GetPointsInSegment(int segmentIndex)
    {
        //每个曲线的开始锚点都是上个曲线的结束锚点（除了起始点）
        //锚点实例：0，1，2，3		3，4，5，6
        return new Vector2[]
        {
            points[segmentIndex * 3],
            points[segmentIndex * 3 + 1],
            points[segmentIndex * 3 + 2],
            points[LoopIndex(segmentIndex * 3 + 3)]
        };
    }

    public int NumSegments
    {
        //除去最开始创建的四个点每次点击都会产生三个点和一条新曲线
        //所以曲线数=最开始的一条+除去四个起始点的所有锚点/3

        get { return (points.Count - 4) / 3 + 1 + (isClosed ? 1 : 0); }

        //  get { return points.Count / 3 }
    }

    public int NumPoints
    {
        get { return points.Count; }
    }

    public void MovePoint(int pointIndex, Vector2 point)
    {
        Vector2 oldPoint = points[pointIndex];
        points[pointIndex] = point;

        //如果是锚点，同步位移其控制点
        if (pointIndex % 3 == 0)
        {
            if (pointIndex + 1 <= points.Count - 1 || isClosed)
            {
                points[LoopIndex(pointIndex + 1)] += point - oldPoint;
            }

            if (pointIndex - 1 >= 0 || isClosed)
            {
                points[LoopIndex(pointIndex - 1)] += point - oldPoint;
            }

            if (autoSetControlPoints)
            {
                AutoSetAllEffectPoint(pointIndex);
            }
        }
        //如果是控制点，使另一个控制点与此点保持水平
        else
        {
            //下一个点是否是控制点
            bool nextIsAnchor = (pointIndex + 1) % 3 == 0;
            //计算锚点索引值
            int anchorIndex = nextIsAnchor ? pointIndex + 1 : pointIndex - 1;
            //计算另一个控制点索引值
            int anthorControlPointIndex = nextIsAnchor ? pointIndex + 2 : pointIndex - 2;
            //控制点必定在 list 之中
            if (anthorControlPointIndex >= 0 && anthorControlPointIndex <= points.Count - 1 || isClosed)
            {
                float distance = (points[LoopIndex(anthorControlPointIndex)] - points[LoopIndex(anchorIndex)])
                    .magnitude;
                Vector2 direction = (points[LoopIndex(anchorIndex)] - point).normalized;
                points[LoopIndex(anthorControlPointIndex)] = points[LoopIndex(anchorIndex)] + distance * direction;
            }
        }
    }

    public bool IsClosed
    {
        get { return isClosed; }
        set
        {
            if (isClosed != value)
            {
                isClosed = value;
                if (isClosed)
                {
                    //添加结尾和开始锚点的另一控制点
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);
                    if (autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoint(0);
                        AutoSetAnchorControlPoint(points.Count - 3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                    if (autoSetControlPoints)
                    {
                        AutoSetStartAndEndControlPoint();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 计算平均放置点的位置
    /// </summary>
    /// <param name="spacing"></param>
    /// <param name="resolution"></param>
    /// <returns></returns>
    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2> {points[0]};
        //前一个点
        Vector2 prePoint = points[0];
        //这个点到上个平均放置点的距离
        float sinceLastEvenPointDistance = 0;
      
        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector2[] segmentPoints = GetPointsInSegment(segmentIndex);
            //锚点和控制点构成的三条线段的长度
            float controlPointsLineLength = Vector2.Distance(segmentPoints[0], segmentPoints[1]) +
                                            Vector2.Distance(segmentPoints[1], segmentPoints[2]) + Vector2.Distance(segmentPoints[2], segmentPoints[3]);
            //估算的贝塞尔曲线长度:锚点间距+锚点和控制点构成的三条线段的一半
            float estimatedCurveLength = Vector2.Distance(segmentPoints[0], segmentPoints[3]) + controlPointsLineLength / 2f;
            int divisons = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisons;
                //采样点
                Vector2 pointOnCurve = Bezier.CalculateCubic(segmentPoints[0], segmentPoints[1], segmentPoints[2],
                    segmentPoints[3], t);
                sinceLastEvenPointDistance += Vector2.Distance(prePoint, pointOnCurve);
                //如果距离超过了期望的平均点位置
                while (sinceLastEvenPointDistance >= spacing)
                {
                    //记录期望平均点位置
                    float offset = sinceLastEvenPointDistance - spacing;
                    Vector2 direction = (prePoint - pointOnCurve).normalized;
                    Vector2 addEvenlyPoint = pointOnCurve + direction * offset;
                    evenlySpacedPoints.Add(addEvenlyPoint);
                    sinceLastEvenPointDistance = offset;
                    prePoint = addEvenlyPoint;
                }

                prePoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }


    /// <summary>
    /// 自动调整所有被影响的控制点的位置
    /// </summary>
    /// <param name="moveAnchorIndex"></param>
    void AutoSetAllEffectPoint(int moveAnchorIndex)
    {
        for (int i = moveAnchorIndex - 3; i <= moveAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < points.Count || isClosed)
            {
                AutoSetAnchorControlPoint(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControlPoint();
    }


    /// <summary>
    /// 自动调整所有控制点位置
    /// </summary>
    void AutoSetAllControlPoint()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoint(i);
        }

        AutoSetStartAndEndControlPoint();
    }

    /// <summary>
    /// 自动调整控制点位置
    /// </summary>
    /// <param name="anchorIndex"></param>
    void AutoSetAnchorControlPoint(int anchorIndex)
    {
        if (NumSegments < 2)
        {
            return;
        }

        Vector2 anchor = points[anchorIndex];
        Vector2 preDir = Vector2.zero;
        Vector2 nextDir = Vector2.zero;
        float[] dis = new float[2];
        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector2 pre = points[LoopIndex(anchorIndex - 3)];
            //获取到前一个控制点的方向
            preDir = (pre - anchor).normalized;
            //获取到前一个控制点的距离
            dis[0] = (pre - anchor).magnitude;
        }

        if (anchorIndex + 3 < points.Count || isClosed)
        {
            Vector2 next = points[LoopIndex(anchorIndex + 3)];
            //获取到后一个控制点的方向
            nextDir = (next - anchor).normalized;
            //获取到后一个控制点的方向
            dis[1] = (next - anchor).magnitude;
        }

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
            {
                //0 代表前一个控制点，1 代表后一个控制点
                points[LoopIndex(controlIndex)] =
                    anchor + (i == 0
                        ? dis[i] * .5f * (preDir - nextDir).normalized
                        : dis[i] * .5f * (nextDir - preDir).normalized);
            }
        }
    }


    /// <summary>
    /// 自动调整开始结束控制点的位置
    /// </summary>
    void AutoSetStartAndEndControlPoint()
    {
        if (!isClosed && NumSegments > 1)
        {
            points[1] = (points[2] + points[0]) / 2;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) / 2;
        }
    }

    int LoopIndex(int currentIndex)
    {
        /*
         *    假如数列长度为 12 ，那么
         *
         * 
         */
        return (currentIndex + points.Count) % points.Count;
    }
}