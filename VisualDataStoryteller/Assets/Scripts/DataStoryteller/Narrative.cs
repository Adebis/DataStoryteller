using System.Collections.Generic;

// A class representing a narrative about a data graph.

public class Narrative
{
    // The shapes in the graph for this narrative, in order of their presentation.
    public List<Shape> shapes;

    public List<NarrativeEvent> events;

    public int current_event_index;

    public Narrative()
    {
        shapes = new List<Shape>();
        events = new List<NarrativeEvent>();
        current_event_index = 0;
    }//end constructor Narrative

    public void AddEvents(List<NarrativeEvent> events_to_add)
    {
        foreach (NarrativeEvent event_to_add in events_to_add)
            events.Add(event_to_add);
    }//end method AddEvents

    // Return the event at the current event index, then increment the index.
    // Returns null if the narrative has reached its end.
    public NarrativeEvent AdvanceNarrative()
    {
        if (current_event_index >= events.Count)
            return null;
        NarrativeEvent event_to_return = events[current_event_index];
        current_event_index += 1;
        return event_to_return;
    }//end method AdvanceNarrative
}//end class Narrative