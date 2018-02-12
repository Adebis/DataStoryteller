using System.Collections.Generic;

// Represents an observation or description that can be made.
// Contains the Points and Segments relevant to the description.
class Descriptor
{
    public int type;
    public List<string> descriptions;
    public List<DataPoint> points;
    public List<Segment> segments;

    // The actual shape part objects that this descriptor is about
    public List<ShapePart> included_parts;

    // The parts of the shape this descriptor's should be grouped into.
    public List<string> shape_part;

    public Descriptor()
    {
        Initialize();
    }//end constructor ShapeDescriptor

    private void Initialize()
    {
        // -1 is untyped
        type = -1;
        descriptions = new List<string>();
        points = new List<DataPoint>();
        segments = new List<Segment>();
        shape_part = new List<string>();
        included_parts = new List<ShapePart>();
    }//end method Initialize

    // Switch the descriptions at the two given indices.
    public void SwitchDescriptions(int first_index, int second_index)
    {
        string temp_string = descriptions[first_index];
        descriptions[first_index] = descriptions[second_index];
        descriptions[second_index] = temp_string;
    }//end method SwitchDescriptors

}//end class Descriptor