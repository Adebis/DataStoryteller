using System;
using System.Collections;
using System.Collections.Generic;

public class Segment
{
    // ID corresponds to the segment's index in the list of all segments.
    public int id;
    public DataPoint start_point;
    public DataPoint end_point;

    // The numerical observations as an array of dictionaries.
    // There are 8 numerical observations:
    //  0: start_x
    //  1: end_x
    //  2: start_y
    //  3: end_y
    //  4: change_x
    //  5: change_y
    //  6: slope_mag
    //  7: slope_dir
    public Dictionary<string, string>[] observations;
    // The IDs of the segments for which the last occurence of each observation occured.
    // For two observations to match occurences, they have to have the same descriptor.
    public int[] last_occurences;
    // The IDs of the segments for which the next occurence of each observation will occur.
    public int[] next_occurences;

    // The style of how the x and y observations should be presented.
    //  0: start value and end values
    //  1: start value and change
    //  2: change and end value
    //  3: change
    //  4: none
    public int x_presentation;
    public int y_presentation;
    // The style of how the slope observations should be presented. 
    //  0: direction only
    //  1: direction and magnitude
    public int slope_presentation;
    // Whether or not to use numerical values for x and y presentation.
    // 0 is true, 1 is false.
    public int use_numerical_x;
    public int use_numerical_y;

    // Which segments this segment should link to, by ID.
    public List<int> linked_segment_ids;

    // A set of segments which are hierarchically below this segment.
    public List<Segment> subsegments;

    // A shape descriptor, if the segment forms one.
    public string shape;

    public Segment()
    {
        this.id = 0;
        this.start_point = new DataPoint();
        this.end_point = new DataPoint();
        this.linked_segment_ids = new List<int>();
        this.subsegments = new List<Segment>();
        shape = "";
        ResetObservations();
    }//end constructor Segment
    public Segment(int id_in, double start_x, double start_y, double end_x, double end_y)
    {
        this.id = id_in;
        this.start_point = new DataPoint(start_x, start_y);
        this.end_point = new DataPoint(end_x, end_y);
        this.linked_segment_ids = new List<int>();
        this.subsegments = new List<Segment>();
        shape = "";
        ResetObservations();
    }//end constructor Segment
    // Initialize this segment with a set of subsegments. The start and end points will be the
    // start of the first segment and the end of the last segment.
    public Segment(int id_in, List<Segment> subsegments_in)
    {
        this.id = id_in;
        this.SetSubsegments(subsegments_in);
        this.linked_segment_ids = new List<int>();
        shape = "";
        ResetObservations();
    }//end constructor Segment

    public void SetSubsegments(List<Segment> subsegments_in)
    {
        this.subsegments = subsegments_in;
        this.start_point = this.subsegments[0].start_point;
        this.end_point = this.subsegments[this.subsegments.Count - 1].end_point;
    }//end method SetSubsegments
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
    public string GetObservationDescription(int observation_id)
    {
        return this.observations[observation_id]["description"];
    }//end method GetObservationDescription
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

    // Get how many other segments, total, are under this segment.
    public int GetSubsegmentCount()
    {
        int subsegment_count = 0;
        foreach (Segment subsegment in this.subsegments)
            subsegment_count += 1 + subsegment.GetSubsegmentCount();
        return subsegment_count;
    }//end method GetSubsegmentCount

    // Return whether or not the given segment ID is a subsegment of the current segment.
    // Checks down the hierarchy; the ID appears in any segment lower in the hierarchy,
    // this will return true.
    public bool HasSubsegment(int segment_id)
    {
        foreach (Segment subsegment in this.subsegments)
            if (subsegment.id == segment_id || subsegment.HasSubsegment(segment_id))
                return true;
        return false;
    }//end method IsSubsegment

    // Returns whether or not this segment has subsegments under it.
    public bool IsSupersegment()
    {
        if (subsegments.Count > 0)
            return true;
        return false;
    }

    public void PrintSubsegments(int depth)
    {
        string padding = "  ";
        for (int i = 0; i < depth; i++)
            padding = padding + padding;
        Console.WriteLine(padding + "Segment " + this.id.ToString() + ". Subsegments: ");
        if (this.subsegments.Count > 0)
        {        
            foreach (Segment subsegment in this.subsegments)
            subsegment.PrintSubsegments(depth + 1);
        }//end if
        else
            Console.WriteLine(padding + "None");
    }//end method PrintSubsegments

    public void ResetObservations()
    {
        this.observations = new Dictionary<string, string>[8];
        for (int i = 0; i < observations.Length; i++)
            this.observations[i] = new Dictionary<string, string>();
    }//end method ResetObservations
    public void ResetOccurences()
    {
        this.last_occurences = new int[8];
        this.next_occurences = new int[8];
    }//end method ResetOccurences
    
}//end class Segment