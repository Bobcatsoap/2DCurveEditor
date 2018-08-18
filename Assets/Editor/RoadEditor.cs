using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(RoadCreator))]
public class NewBehaviourScript : Editor
{
    private RoadCreator _roadCreator;
    
    private void OnEnable()
    {
        _roadCreator = (RoadCreator) target;
    }

    private void OnSceneGUI()
    {
        if (_roadCreator.autoUpdate&&Event.current.type==EventType.Repaint)
        {
            _roadCreator.UpdateRoad();
        }
    }
}
