using System.Collections.Generic;
using System;

// A shape object describes the shape that a section of a numerical graph makes.
abstract class Shape
{
    public bool shape_of_interest;
    protected string shape_name;
    // Generate and return a text description of this shape.
    public abstract string GenerateDescription(List<double> x_refs, List<double> y_refs, string x_label, string y_label, DataPoint point_of_interest, string site_name, string variable_name);
    // Match a list of segments to this shape's parts and critical points.
    public abstract void MatchSegmentsToShape(List<Segment> segments_in, List<double> critical_points_in);

    protected double FindNearestReference(double real_value, List<double> reference_list)
    {
        double smallest_difference = double.MaxValue;
        double closest_reference = 0;
        foreach (double reference_value in reference_list)
        {
            double current_difference = Math.Abs(real_value - reference_value);
            if (current_difference < smallest_difference)
            {
                smallest_difference = current_difference;
                closest_reference = reference_value;
            }//end if
        }//end foreach
        return closest_reference;
    }//end method FindNearestReference
}//end class Shape