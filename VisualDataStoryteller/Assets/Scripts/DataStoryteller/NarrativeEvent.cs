using System.Collections.Generic;

// A class representing a single event in a narrative about a graph.
public class NarrativeEvent
{
    public List<DataPoint> associated_points;
    public string description;

    public NarrativeEvent()
    {
        associated_points = new List<DataPoint>();
        description = "";
    }//end constructor
    public NarrativeEvent(string description_in, DataPoint associated_point_in)
    {
        associated_points = new List<DataPoint>();
        associated_points.Add(associated_point_in);
        description = description_in;
    }//end constructor
    public NarrativeEvent(string description_in, List<DataPoint> associated_points_in)
    {
        associated_points = associated_points_in;
        description = description_in;
    }//end constructor
}//end class NarrativeEvent