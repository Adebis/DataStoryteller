using System.Collections.Generic;

// One part of a shape. Consists of a set of segments, and a set of critical points.
class ShapePart
{
    public List<Segment> part_segments;
    public List<DataPoint> part_critical_points;

    public ShapePart()
    {
        Initialize();
    }//end constructor ShapePart
    public ShapePart(List<Segment> segments_in)
    {
        Initialize();
        AddSegments(segments_in);
    }//end constructor ShapePart
    public ShapePart(List<Segment> segments_in, List<DataPoint> critical_points_in)
    {
        Initialize();
        AddSegments(segments_in);
        part_critical_points = critical_points_in;
    }//end constructor ShapePart

    // Add a segment to the end of this shape part.
    public void AddSegment(Segment segment_in)
    {
        part_segments.Add(segment_in);
    }//end method AddSegment
    // Add a group of segments to this shape part.
    public void AddSegments(List<Segment> segments_in)
    {
        foreach (Segment segment_in in segments_in)
            AddSegment(segment_in);
    }//end method AddSegments
    
    // Add a critical point to the shape.
    public void AddCriticalPoint(DataPoint point_in)
    {
        part_critical_points.Add(point_in);
    }//end method AddCriticalPoint
    public void AddCriticalPoints(List<DataPoint> points_in)
    {
        foreach (DataPoint point_in in points_in)
            AddCriticalPoint(point_in);
    }//end method AddCriticalPoints

    private void Initialize()
    {
        part_segments = new List<Segment>();
        part_critical_points = new List<DataPoint>();
    }//end method Initialize

}//end class ShapePart
