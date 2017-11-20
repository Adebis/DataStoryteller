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
    }//end constructor Segment
    public Segment(int id_in, double start_x, double start_y, double end_x, double end_y)
    {
        this.id = id_in;
        this.start_point = new DataPoint(start_x, start_y);
        this.end_point = new DataPoint(end_x, end_y);
        this.linked_segment_ids = new List<int>();
    }//end constructor Segment

    private void Initialize()
    {
        
    }//end method Initialize
    
}//end class Segment