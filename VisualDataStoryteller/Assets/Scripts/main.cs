using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour {

	public GameObject data_point_prefab;

	private List<GameObject> all_data_point_objects;

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
		int description_type = 0;
		NarrativeGenerator generator = new NarrativeGenerator();
		string segment_file_name = segment_file_names[data_index];
		string info_file_name = info_file_names[data_index];
		string data_file_name = data_file_names[data_index];
		double starting_year = 1980;

		for (int i = 0; i < 3; i++)
			generator.GenerateNarrative(data_file_name, segment_file_name, info_file_name, starting_year, i);

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

		// Pixel boundaries.
		double width = 200;
		double height = 200;

		// Conversion ratios of x and y values to pixel distances.
		double pixels_per_x = width / (right_x - left_x);
		double pixels_per_y = height / (top_y - bottom_y);

		// Place all data points according to pixel distances.
		DataPoint internal_data_point = null;
		foreach (GameObject data_point_object in all_data_point_objects)
		{
			internal_data_point = data_point_object.GetComponent<DataPointController>().internal_data_point;
			data_point_object.transform.position = new Vector3((float)((internal_data_point.x - left_x) * pixels_per_x), (float)((internal_data_point.y - bottom_y) * pixels_per_y), 0);
		}//end foreach

		// Place the camera so that it is centered on the graph.
		Camera.main.transform.position = new Vector3((float)width / 2, (float)height / 2, Camera.main.transform.position.z);
	}//end method StartStoryteller
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
