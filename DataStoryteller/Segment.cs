using System;
using System.Collections;
using System.Collections.Generic;

public class Segment
{
    // ID corresponds to the segment's index in the list of all segments.
    public int id;
    public DataPoint start_point;
    public DataPoint end_point;

    // Numerical observations
    public Dictionary<string, string>[] observations;
    // There are 8 numerical observations:
    //  0: start_x
    //  1: end_x
    //  2: start_y
    //  3: end_y
    //  4: change_x
    //  5: change_y
    //  6: slope_mag
    //  7: slope_dir

    // Start and end point values
    public double start_value;
    public double start_value_reference;
    public string start_value_descriptor; //ID 0
    public double end_value;
    public double end_value_reference;
    public string end_value_descriptor; // ID 0
    // Start and end point dates
    public double start_date;
    public double start_date_reference;
    public string start_date_descriptor;
    public double end_date;
    public double end_date_reference;
    public string end_date_descriptor;
    // Changes
    public double magnitude_of_change;
    public string magnitude_descriptor; // ID 1
    public double duration_of_change;
    public string duration_descriptor; // ID 2
    // Slope
    public double slope;
    public string direction_descriptor; // ID 3
    public string rate_descriptor; // ID 4

    // Which segments this segment should link to, by ID.
    public List<int> linked_segment_ids;

    public Segment()
    {
        this.id = 0;
        this.start_point = new DataPoint();
        this.end_point = new DataPoint();
        this.linked_segment_ids = new List<int>();
        ResetObservations();
    }//end constructor Segment
    public Segment(int id_in, double start_x, double start_y, double end_x, double end_y)
    {
        this.id = id_in;
        this.start_point = new DataPoint(start_x, start_y);
        this.end_point = new DataPoint(end_x, end_y);
        this.linked_segment_ids = new List<int>();
        ResetObservations();
    }//end constructor Segment

    private void Initialize()
    {
        
    }//end method Initialize

    public void AddObservation(int observation_id, string name, string value_as_string, string description)
    {
        this.AddObservationField(observation_id, "name", name);
        this.AddObservationField(observation_id, "value", value_as_string);
        this.AddObservationField(observation_id, "description", description);
    }//end method AddObservation
    // Add or change a single field for the given observation id.
    public void AddObservationField(int observation_id, string field_name, string field_value)
    {
        if (this.observations[observation_id].ContainsKey(field_name))
            this.observations[observation_id][field_name] = field_value;
        else
            this.observations[observation_id].Add(field_name, field_value);
    }//end method AddObservationField
    // Return min value if parse did not succeed.
    public double GetObservationValue(int observation_id)
    {
        double return_value = 0;
        bool parse_success = Double.TryParse(this.observations[observation_id]["value"], out return_value);
        if (parse_success)
            return return_value;
        else
            return double.MinValue;
    }//end method GetObservation
    public void ResetObservations()
    {
        this.observations = new Dictionary<string, string>[8];
        for (int i = 0; i < observations.Length; i++)
            this.observations[i] = new Dictionary<string, string>();
    }//end method ResetObservations
    
}//end class Segment