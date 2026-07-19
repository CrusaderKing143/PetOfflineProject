using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SweepingRobotRouteFollower))]
public sealed class SweepingRobotRouteFollowerEditor : Editor
{
    private SerializedProperty animatorProperty;
    private SerializedProperty bodyProperty;
    private SerializedProperty animationSetProperty;
    private SerializedProperty normalSpeedProperty;
    private SerializedProperty boostedSpeedProperty;
    private SerializedProperty arrivalThresholdProperty;
    private SerializedProperty routePointsProperty;
    private ReorderableList routeList;

    private SweepingRobotRouteFollower Robot => (SweepingRobotRouteFollower)target;

    private void OnEnable()
    {
        animatorProperty = serializedObject.FindProperty("animator");
        bodyProperty = serializedObject.FindProperty("body");
        animationSetProperty = serializedObject.FindProperty("animationSet");
        normalSpeedProperty = serializedObject.FindProperty("normalSpeed");
        boostedSpeedProperty = serializedObject.FindProperty("boostedSpeed");
        arrivalThresholdProperty = serializedObject.FindProperty("arrivalThreshold");
        routePointsProperty = serializedObject.FindProperty("routePoints");
        CreateRouteList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawMovementSettings();
        EditorGUILayout.Space();
        routeList.DoLayoutList();
        DrawRouteButtons();
        DrawRouteHint();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawMovementSettings()
    {
        EditorGUILayout.PropertyField(animatorProperty);
        EditorGUILayout.PropertyField(bodyProperty);
        EditorGUILayout.PropertyField(animationSetProperty, new GUIContent("场景动画组"));
        EditorGUILayout.PropertyField(normalSpeedProperty, new GUIContent("正常速度"));
        EditorGUILayout.PropertyField(boostedSpeedProperty, new GUIContent("加速速度"));
        EditorGUILayout.PropertyField(arrivalThresholdProperty, new GUIContent("到点误差"));
    }

    private void CreateRouteList()
    {
        routeList = new ReorderableList(serializedObject, routePointsProperty, true, true, false, true);
        routeList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "循环线路节点（世界坐标）");
        routeList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
        routeList.drawElementCallback = DrawRouteElement;
        routeList.onRemoveCallback = RemoveRoutePoint;
    }

    private void DrawRouteElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        rect.y += 2f;
        rect.height = EditorGUIUtility.singleLineHeight;
        SerializedProperty point = routePointsProperty.GetArrayElementAtIndex(index);
        EditorGUI.PropertyField(rect, point, new GUIContent("节点 " + (index + 1)));
    }

    private void DrawRouteButtons()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添加当前位置"))
        {
            AddRoutePoint(Robot.transform.position, "添加路线节点");
        }

        if (GUILayout.Button("追加节点"))
        {
            Vector2 point = GetAppendPosition();
            AddRoutePoint(point, "追加路线节点");
        }

        if (GUILayout.Button("清空线路"))
        {
            ClearRoute();
        }
        EditorGUILayout.EndHorizontal();
    }

    private Vector2 GetAppendPosition()
    {
        if (routePointsProperty.arraySize == 0)
        {
            return Robot.transform.position;
        }

        Vector2 lastPoint = routePointsProperty
            .GetArrayElementAtIndex(routePointsProperty.arraySize - 1)
            .vector2Value;
        return lastPoint + new Vector2(1f, 0.5f);
    }

    private void AddRoutePoint(Vector2 point, string undoName)
    {
        Undo.RecordObject(Robot, undoName);
        int index = routePointsProperty.arraySize;
        routePointsProperty.InsertArrayElementAtIndex(index);
        routePointsProperty.GetArrayElementAtIndex(index).vector2Value = point;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Robot);
        SceneView.RepaintAll();
    }

    private void RemoveRoutePoint(ReorderableList list)
    {
        if (list.index < 0)
        {
            return;
        }

        Undo.RecordObject(Robot, "删除路线节点");
        routePointsProperty.DeleteArrayElementAtIndex(list.index);
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Robot);
        SceneView.RepaintAll();
    }

    private void ClearRoute()
    {
        if (routePointsProperty.arraySize == 0)
        {
            return;
        }

        Undo.RecordObject(Robot, "清空扫地机线路");
        routePointsProperty.ClearArray();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(Robot);
        SceneView.RepaintAll();
    }

    private void DrawRouteHint()
    {
        string message = routePointsProperty.arraySize < 2
            ? "少于两个节点时不会循环移动。"
            : "机器人会按列表顺序移动，并从末节点返回首节点。";
        EditorGUILayout.HelpBox(message, MessageType.Info);
    }

    private void OnSceneGUI()
    {
        serializedObject.Update();
        DrawRouteLines();
        DrawRouteHandles();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRouteLines()
    {
        int count = routePointsProperty.arraySize;
        if (count < 2)
        {
            return;
        }

        Handles.color = Color.cyan;
        for (int index = 0; index < count; index++)
        {
            Vector3 start = ToScenePosition(GetRoutePoint(index));
            Vector3 end = ToScenePosition(GetRoutePoint((index + 1) % count));
            Handles.DrawAAPolyLine(3f, start, end);
        }
    }

    private void DrawRouteHandles()
    {
        for (int index = 0; index < routePointsProperty.arraySize; index++)
        {
            Vector3 position = ToScenePosition(GetRoutePoint(index));
            Handles.Label(position, "节点 " + (index + 1), EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            Vector3 movedPosition = Handles.PositionHandle(position, Quaternion.identity);
            if (!EditorGUI.EndChangeCheck())
            {
                continue;
            }

            Undo.RecordObject(Robot, "移动路线节点");
            routePointsProperty.GetArrayElementAtIndex(index).vector2Value = movedPosition;
            EditorUtility.SetDirty(Robot);
        }
    }

    private Vector2 GetRoutePoint(int index)
    {
        return routePointsProperty.GetArrayElementAtIndex(index).vector2Value;
    }

    private Vector3 ToScenePosition(Vector2 point)
    {
        return new Vector3(point.x, point.y, Robot.transform.position.z);
    }
}
