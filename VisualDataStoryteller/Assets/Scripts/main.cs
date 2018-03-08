using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour {

	public GameObject data_point_prefab;

	private List<GameObject> all_data_point_objects;

	private Narrative current_narrative;

	// Pixel boundaries.
	private double width;
	private double height;

	// Conversion ratios of x and y values to pixel distances.
	private double pixels_per_x;
	private double pixels_per_y;

	// Global x and y boundaries from the data.
	private double global_left_x;
	private double global_bottom_y;

	// Camera variables
	public Vector3 target_position;
	public float camera_movement_time = 0.3f;
	private Vector3 velocity = Vector3.zero;

	void Awake ()
	{
		StartStoryteller();
	}//end method Awake

	private void StartStoryteller()
	{
		print("Hello World!");
		List<string> segment_file_names = new List<string>();
		List<string> info_file_names = new List<string>();
		List<string> data_file_names = new List<string>();
		// Calcium at Basin Bay (a.k.a. Chemical C at Site B), w. Index 0
		segment_file_names.Add("bb_ca/bb_ca_segments.csv");
		info_file_names.Add("bb_ca/bb_ca_meta.json");
		data_file_names.Add("bb_ca/bb_ca_csv.csv");
		// Chlorine at Basin Bay (a.k.a. Chemical H at Site C), w. Index 1
		segment_file_names.Add("bb_cl/bb_cl_segments.csv");
		info_file_names.Add("bb_cl/bb_cl_meta.json");
		data_file_names.Add("bb_cl/bb_cl_csv.csv");
		// Magnesium at Anthony's Nose (a.k.a. Chemical M at Site T), v. Index 2
		segment_file_names.Add("an_mg/an_mg_segments.csv");
		info_file_names.Add("an_mg/an_mg_meta.json");
		data_file_names.Add("an_mg/an_mg_csv.csv");
		// SO4 at French Point (a.k.a. Chemical S at Site F), line. Index 3
		segment_file_names.Add("f_so4/f_so4_segments.csv");
		info_file_names.Add("f_so4/f_so4_meta.json");
		data_file_names.Add("f_so4/f_so4_csv.csv");
		// Sodium at Northwest Bay (a.k.a. Chemical A at Site N), line. Index 4
		segment_file_names.Add("a10_na/a10_na_segments.csv");
		info_file_names.Add("a10_na/a10_na_meta.json");
		data_file_names.Add("a10_na/a10_na_csv.csv");
		// Silicon at Roger's Rock (a.k.a. Chemical L at Site G), w. Index 5
		segment_file_names.Add("r_si/r_si_segments.csv");
		info_file_names.Add("r_si/r_si_meta.json");
		data_file_names.Add("r_si/r_si_csv.csv");
		// Conductivity at Dome Island (a.k.a. Chemical O at Site I), line, w, and v. Index 6
		segment_file_names.Add("d_cond/d_cond_segments.csv");
		info_file_names.Add("d_cond/d_cond_meta.json");
		data_file_names.Add("d_cond/d_cond_csv.csv");
		// Calcium at Tea Island (a.k.a. Chemical E at Site K), line. Index 7
		segment_file_names.Add("t_ca/t_ca_segments.csv");
		info_file_names.Add("t_ca/t_ca_meta.json");
		data_file_names.Add("t_ca/t_ca_csv.csv");

		int data_index = 0;
		int description_type = 2;	// 1 is no hint, point of interest. 2 is no hint, no point of interest. 3 is full.
		NarrativeGenerator generator = new NarrativeGenerator();
		string segment_file_name = segment_file_names[data_index];
		string info_file_name = info_file_names[data_index];
		string data_file_name = data_file_names[data_index];
		double starting_year = 1980;

		//for (int i = 0; i < 3; i++)
		current_narrative = generator.GenerateNarrative(data_file_name, segment_file_name, info_file_name, starting_year, description_type);

		// Make a data point game object for each data point in the narrative generator.
		all_data_point_objects = new List<GameObject>();
		GameObject new_data_point_object = null;
		double left_x = double.MaxValue;
		double right_x = double.MinValue;
		double top_y = double.MinValue;
		double bottom_y = double.MaxValue;
		foreach (DataPoint data_point in generator.all_data_points)
		{
			// Find the borders of the graph.
			if (data_point.x < left_x)
				left_x = data_point.x;
			if (data_point.x > right_x)
				right_x = data_point.x;
			if (data_point.y < bottom_y)
				bottom_y = data_point.y;
			if (data_point.y > top_y)
				top_y = data_point.y;

			new_data_point_object = Instantiate(data_point_prefab, new Vector3(0, 0, 0), Quaternion.identity);
			new_data_point_object.GetComponent<DataPointController>().internal_data_point = data_point;
			all_data_point_objects.Add(new_data_point_object);
		}//end foreach

		//left_x = generator.x_refs[0];
		//right_x = generator.x_refs[generator.x_refs.Count - 1];
		//top_y = generator.y_refs[generator.y_refs.Count - 1];
		//bottom_y = generator.y_refs[0];

		global_left_x = left_x;
		global_bottom_y = bottom_y;

		// Pixel boundaries.
		this.width = 200;
		this.height = 200;

		// Conversion ratios of x and y values to pixel distances.
		this.pixels_per_x = width / (right_x - left_x);
		this.pixels_per_y = height / (top_y - bottom_y);

		// Place all data points according to pixel distances.
		DataPoint internal_data_point = null;
		foreach (GameObject data_point_object in all_data_point_objects)
		{
			internal_data_point = data_point_object.GetComponent<DataPointController>().internal_data_point;
			data_point_object.transform.position = new Vector3((float)(ToWorldCoordinate(internal_data_point.x - global_left_x, true)), (float)(ToWorldCoordinate(internal_data_point.y - global_bottom_y, false)), 0);
		}//end foreach

		// Move the camera so that it is centered on the graph.
		//Camera.main.transform.position = new Vector3((float)width / 2, (float)height / 2, Camera.main.transform.position.z);
		target_position = new Vector3((float)width / 2, (float)height / 2, Camera.main.transform.position.z);

	}//end method StartStoryteller
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKeyDown("space"))
		{
			AdvanceNarrative();
		}//end if
		if (Camera.main.transform.position != target_position)
			Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, target_position, ref velocity, camera_movement_time);
	} //end method Update

	private void AdvanceNarrative()
	{
		print("Advancing narrative. Current event index: " + current_narrative.current_event_index.ToString());
		NarrativeEvent current_event = current_narrative.AdvanceNarrative();
		if (current_event == null)
		{
			print ("Narrative complete.");
			return;
		}// end if

		// Place the camera so that the points for the event are in frame.
		float camera_x = Camera.main.transform.position.x;
		float camera_y = Camera.main.transform.position.y;
		float camera_z = Camera.main.transform.position.z;

		// If there is more than one point, place the camera in the middle of them.
		if (current_event.associated_points.Count > 1)
		{
			double leftmost_x = float.MaxValue;
			double rightmost_x = float.MinValue;
			double bottommost_y = float.MaxValue;
			double topmost_y = float.MinValue;

			foreach (DataPoint event_point in current_event.associated_points)
			{
				if (event_point.x < leftmost_x)
					leftmost_x = event_point.x;
				if (event_point.x > rightmost_x)
					rightmost_x = event_point.x;
				if (event_point.y < bottommost_y)
					bottommost_y = event_point.y;
				if (event_point.y > topmost_y)
					topmost_y = event_point.y;
			}//end foreach

			print ("left_x: " + leftmost_x.ToString() + " right_x: " + rightmost_x.ToString() + " top_y: " + topmost_y.ToString() + " bottom_y: " + bottommost_y.ToString());

			leftmost_x = ToWorldCoordinate(leftmost_x - global_left_x, true);
			rightmost_x = ToWorldCoordinate(rightmost_x - global_left_x, true);
			bottommost_y = ToWorldCoordinate(bottommost_y - global_bottom_y, false);
			topmost_y = ToWorldCoordinate(topmost_y - global_bottom_y, false);

			camera_x = (float)(leftmost_x + (rightmost_x - leftmost_x) / 2);
			camera_y = (float)(bottommost_y + (topmost_y - bottommost_y) / 2);
		}//end if
		// Otherwise, center the camera on the single point.
		else if (current_event.associated_points.Count == 1)
		{
			DataPoint event_point = current_event.associated_points[0];
			print ("x: " + event_point.x.ToString() + " y: " + event_point.y.ToString());
			camera_x = (float)ToWorldCoordinate(event_point.x - global_left_x, true);
			camera_y = (float)ToWorldCoordinate(event_point.y - global_bottom_y, true);
		}//end else if

		print ("Description: " + current_event.description);

		target_position = new Vector3(camera_x, camera_y, camera_z);
	}//end method AdvanceNarrative

	// Transforms a given graph value to its corresponding game world coordinate
	// depending on whether it is an x value or a y value.
	private double ToWorldCoordinate(double value_to_convert, bool is_x)
	{
		if (is_x)
			return value_to_convert * pixels_per_x;
		else
			return value_to_convert * pixels_per_y;
	}//end method ToWorldCoordinate
}
