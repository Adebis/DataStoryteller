using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPointController : MonoBehaviour {

	// The internal data point that this script's game object represents on the graph.
	public DataPoint internal_data_point;

	void Awake()
	{
		internal_data_point = new DataPoint();
	}//end method Awake
	
	// Update is called once per frame
	void Update () 
	{
		
	}//end method Update

	public void SetDataPoint(DataPoint data_point_in)
	{
		internal_data_point = data_point_in;
	}//end method SetDataPoint
}
