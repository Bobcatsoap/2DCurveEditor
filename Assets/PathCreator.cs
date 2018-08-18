using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
	[HideInInspector] public Path Path;

	public Color AnchorColor = Color.black;
	public Color ControlColor = Color.white;
	public Color SegmentColor = Color.green;
	public Color HightLightSegmentColor = Color.red;
	public Color AnchorControlConnectLineColor = Color.black;
	public float AnchorDiameter = .1f;
	public float ControlDiameter = .05f;
	public bool DisplayControlPoint = true;


	public void CreatePath()
	{
		Path=new Path(transform.position);
	}

	private void Reset()
	{
		CreatePath();	
	}
}
