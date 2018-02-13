using System.Collections.Generic;

// A class representing a narrative about a data graph.

public class Narrative
{
    // The shapes in the graph for this narrative, in order of their presentation.
    public List<Shape> shapes;

    public List<NarrativeEvent> events;

    public Narrative()
    {
        shapes = new List<Shape>();
        events = new List<NarrativeEvent>();
    }//end constructor Narrative
}//end class Narrative