using System.Collections.Generic;
using System;

// A shape object describes the shape that a section of a numerical graph makes.
abstract class Shape
{
    public bool shape_of_interest;
    protected string shape_name;

    // Whether or not we print debug information
    public bool verbose;

    // Dictionary mapping numerical reference values to their string representations.
    public Dictionary<double, string> reference_map;

    // Used for generating different types of description for different test cases.
    //  0 = default, all features included.
    //  1 = no point-of-interest hint.
    //  2 = no point-of-interest hint or point-of-interest information.
    public int description_type;
    // Generate and return a text description of this shape.
    public abstract string GenerateDescription(List<double> x_refs, List<double> y_refs, string x_label, string y_label, DataPoint point_of_interest, string site_name, string variable_name);
    // Match a list of segments to this shape's parts and critical points.
    public abstract void MatchSegmentsToShape(List<Segment> segments_in, List<double> critical_points_in);

    protected string FindNearestReference(double real_value, List<double> reference_list, bool y_refs = false)
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
        // Convert reference value (which is in days since 1980) into its corresponding string representation.
        string closest_label = "";
        if (!y_refs)
            closest_label = reference_map[closest_reference];
        else if(y_refs)
            closest_label = closest_reference.ToString();
        return closest_label;
    }//end method FindNearestReference
}//end class Shape