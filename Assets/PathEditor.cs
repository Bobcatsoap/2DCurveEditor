using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor((typeof(PathCreator)))]
public class PathEditor : Editor
{
    private PathCreator creator;

    private Path Path
    {
        get { return creator.Path; }
    }

    const float MinDisOfPointMutli = 1f;

    private const float MinDisOfSegment = .1f;

    private int _highLightSegmentIndex = -1;

    private int drawSpacingPointsCount ;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create new"))
        {
            creator.CreatePath();
            SceneView.RepaintAll();
        }

        bool isClosed = GUILayout.Toggle(Path.IsClosed, "Closed Path");
        if (isClosed != Path.IsClosed)
        {
            Path.IsClosed = isClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != Path.AutoSetControlPoints)
        {
            Path.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event e = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin;
        if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
        {
            if (_highLightSegmentIndex != -1)
            {
                Path.SplitSegment(_highLightSegmentIndex, mousePos);
            }
            else
            {
                if (!Path.IsClosed)
                {
                    Path.AddSegment(mousePos);
                }
            }
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            Debug.LogWarning("right click");
            for (int i = 0; i < Path.NumPoints; i++)
            {
                if (Vector2.Distance(mousePos, Path[i]) <= MinDisOfPointMutli * creator.AnchorDiameter)
                {
                    Debug.LogWarning(i);
                    Path.DeleteSegment(i);
                    break;
                }
            }
        }

        if (e.type == EventType.MouseMove)
        {
            _highLightSegmentIndex = -1;

            for (int i = 0; i < Path.NumSegments; i++)
            {
                Vector2[] points = Path.GetPointsInSegment(i);
                float dis = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (dis < MinDisOfSegment)
                {
                    _highLightSegmentIndex = i;
                }
            }

            if (_highLightSegmentIndex != -1)
            {
                HandleUtility.Repaint();
            }

            /*
            if (newSelectedSegmentIndex != highLightSegmentIndex)
            {
                highLightSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
            */
        }
    }

    void Draw()
    {
        for (int i = 0; i < Path.NumSegments; i++)
        {
            Vector2[] points = Path.GetPointsInSegment(i);
            if (creator.DisplayControlPoint)
            {
                Handles.color = creator.AnchorControlConnectLineColor;
                Handles.DrawLine(points[0], points[1]);
                Handles.DrawLine(points[2], points[3]);
            }

            Color bezierColor = _highLightSegmentIndex == i && Event.current.shift
                ? creator.HightLightSegmentColor
                : creator.SegmentColor;
            Handles.DrawBezier(points[0], points[3], points[1], points[2], bezierColor, null, 2);
        }


        for (int i = 0; i < Path.NumPoints; i++)
        {
            if (i % 3 != 0 && !creator.DisplayControlPoint)
            {
                break;
            }

            Handles.color = i % 3 == 0 ? creator.AnchorColor : creator.ControlColor;
            float unitSize = i % 3 == 0 ? creator.AnchorDiameter : creator.ControlDiameter;
            Vector2 handlePosition = Handles.FreeMoveHandle(Path[i], Quaternion.identity, unitSize, Vector2.zero,
                Handles.CylinderHandleCap);
            if (handlePosition != Path[i])
            {
                //更新撤销指令指定的目标
                Undo.RecordObject(creator, "Move Point");
                Path.MovePoint(i, handlePosition);
            }
        }

      
    }

    private void OnEnable()
    {
        //获取 inspector 中选中的对象
        creator = (PathCreator) target;

        if (creator.Path == null)
        {
            creator.CreatePath();
        }
    }
}