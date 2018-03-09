using System.Collections.Generic;

// A class representing a single event in a narrative about a graph.
public class NarrativeEvent
{
    // Old Vars
    public List<DataPoint> associated_points;
    public string description;

    // New Vars
    // What type of information this narrative event carries.
    // 0 = Critical Point
    // 1 = Abnormality
    public int information_type;

    // The piece of information from the graph to be presented in
    // this narrative event.
    public GraphInfo event_info;

    // What related pieces of information need to have been mentioned
    // for this narrative event's information to be grounded.
    public List<GraphInfo> grounding_info;

    // How much the tension changes as a result of this narrative event.
    public float tension_change;

    public NarrativeEvent()
    {
        associated_points = new List<DataPoint>();
        description = "";

        information_type = -1;
        event_info = new GraphInfo();
        grounding_info = new List<GraphInfo>();
        tension_change = 0;
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