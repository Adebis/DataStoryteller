using System.Collections.Generic;
using System;

// The shape of a line, going either up or down.
class ShapeLine : Shape
{
    // Shape parts
    // The parts of the shape that will be stored will be abnormal sections that go against
    // the up/down trend.
    private List<ShapePart> shape_parts;

    // Critical Points
    // The critical points of a line shape are the starting point
    // and ending point.
    // Indices:
    //  0 = starting point
    //  1 = ending point
    // Additionally, past the first two, every pair of indices will be the
    // start and end point to a section of the line that defies the overall trend.
    private List<DataPoint> critical_points;
    // The last index that denotes a boundary points.
    //private int last_boundary_index;

    // An ordered list of all the segments in this shape.
    private List<Segment> segments;

    // A list of descriptors for this shape.
    // Each descriptor is a set of string descriptions for some part
    // of the shape, as well as the points and segments related to
    // the description.
    private List<Descriptor> abnormal_descriptors;

    // Which way is this line going, up or down?
    private string direction;

    public ShapeLine()
    {
        Initialize();
    }//end constructor ShapeLine

    private void Initialize()
    {
        shape_name = "line";
        direction = "";
        //last_boundary_index = -1;
        shape_of_interest = false;
        shape_parts = new List<ShapePart>();

        critical_points = new List<DataPoint>();

        abnormal_descriptors = new List<Descriptor>();
    }//end method Initialize

    // Match a list of segments to this shape's parts and critical points. Critical points in are x values.
    public override void MatchSegmentsToShape(List<Segment> segments_in, List<double> critical_points_in)
    {
        this.segments = segments_in;
        // First, find data points for all critical points.
        bool match_found = false;
        foreach (double critical_point_x in critical_points_in)
        {
            match_found = false;
            foreach (Segment segment_in in segments_in)
            {
                if (segment_in == segments_in[segments_in.Count - 1])
                    Console.WriteLine("Checking last segment");
                if (!SignificantlyDifferent(segment_in.start_point.x, critical_point_x, 1))
                {
                    if (!critical_points.Contains(segment_in.start_point))
                    {
                        critical_points.Add(segment_in.start_point);
                        match_found = true;
                        break;
                    }//end if
                }//end if
                if (!SignificantlyDifferent(segment_in.end_point.x, critical_point_x, 1))
                {
                    if (!critical_points.Contains(segment_in.end_point))
                    {
                        critical_points.Add(segment_in.end_point);
                        match_found = true;
                        break;
                    }//end if
                }//end else if
            }//end foreach
            if (!match_found)
                Console.WriteLine("No matching datapoint for " + critical_point_x.ToString());
        }//end foreach

        // Check if this is an upward line or a downward trending line.
        if (critical_points[0].y < critical_points[1].y)
            direction = "up";
        else if (critical_points[0].y > critical_points[1].y)
            direction = "down";

        // Create new critical points by looking for groups of segments that go against the trend.
        // Every pair of critical points, past the first, will be the start and end of an abnormal section.
        int critical_point_index = 0;
        string current_direction = "";
        List<Segment> current_abnormal_section = new List<Segment>();
        DataPoint current_starting_point = new DataPoint();
        DataPoint current_ending_point = new DataPoint();
        foreach (Segment segment_in in segments_in)
        {
            // Is this segment going down or up?
            if (segment_in.start_point.y < segment_in.end_point.y)
                current_direction = "up";
            else
                current_direction = "down";

            // If the segment is not going the same way as the line as a whole
            if (!current_direction.Equals(direction))
            {
                // Add it to any previous ongoing abnormal section.
                current_abnormal_section.Add(segment_in);
            }//end if
            // If the segment IS going the same way as the line as a whole
            if (current_direction.Equals(direction))
            {
                // Check if there was an ongoing abnormal section.
                if (current_abnormal_section.Count > 0)
                {
                    // If so, note its start and end points. These will become new critical points.
                    current_starting_point = current_abnormal_section[0].start_point;
                    current_ending_point = current_abnormal_section[current_abnormal_section.Count - 1].end_point;
                    // Make new critical points out of them. 
                    //critical_points.Add(current_starting_point);
                    //critical_points.Add(current_ending_point);

                    List<DataPoint> new_critical_points = new List<DataPoint>();
                    new_critical_points.Add(current_starting_point);
                    new_critical_points.Add(current_ending_point);
                    // Make a shape part out of these segments.
                    ShapePart new_part = new ShapePart(current_abnormal_section, new_critical_points);
                    shape_parts.Add(new_part);

                    // Reset the current abnormal section.
                    current_abnormal_section = new List<Segment>();
                }//end if
            }//end if
        }//end foreach
        // At the end of the loop, there may still be an ongoing abnormal section that needs to be handled.
        // Check if there was an ongoing abnormal section.
        if (current_abnormal_section.Count > 0)
        {
            // If so, note its start and end points. These will become new critical points.
            current_starting_point = current_abnormal_section[0].start_point;
            current_ending_point = current_abnormal_section[current_abnormal_section.Count - 1].end_point;
            // Make new critical points out of them. 
            //critical_points.Add(current_starting_point);
            //critical_points.Add(current_ending_point);

            List<DataPoint> new_critical_points = new List<DataPoint>();
            new_critical_points.Add(current_starting_point);
            new_critical_points.Add(current_ending_point);
            // Make a shape part out of these segments.
            ShapePart new_part = new ShapePart(current_abnormal_section, new_critical_points);
            shape_parts.Add(new_part);

            // Reset the current abnormal section.
            current_abnormal_section = new List<Segment>();
        }//end if
        // Note the index of the last critical point that denotes a section boundary.
        //last_boundary_index = critical_points.Count - 1;
    }//end method MatchSegmentsToShape
    
    // For the "regular" values of a Line:
    //  1. Define the start and end (x values) of the shape as the first and last critical points.
    //  2. Define the top and bottom of the shape:
    //      a. Look at the start and end points.
    //      b. The one with the higher y value is the top, and the one with the lower y value is the bottom.
    //  Check the regularity of the points.
    //  3. Choose abnormal sections close to the point of interest and comment on them.
    // Generate and return a text description of this shape.
    public override string GenerateDescription(List<double> x_refs, List<double> y_refs, string x_label, string y_label, DataPoint point_of_interest, string site_name, string variable_name)
    {
        //string description = "Description for shape 'w': ";
        string description = "";
        if (this.shape_of_interest)
        {
            description = GenerateDetailedDescription(x_refs, y_refs, x_label, y_label, point_of_interest, site_name, variable_name);
        }//end if
        else
        {
            description = GenerateShallowDescription();
        }//end else

        return description;
    }//end method GenerateDescription

    // Create a more detailed description for this shape, including how it relates to the point of interest.
    private string GenerateDetailedDescription(List<double> x_refs, List<double> y_refs, string x_label, string y_label, DataPoint point_of_interest, string site_name, string variable_name)
    {
        string description = "";

        // Calculate value (y-axis) range
        double highest_value = double.MinValue;
        double lowest_value = double.MaxValue;
        foreach (double y_value in y_refs)
        {
            if (y_value > highest_value)
                highest_value = y_value;
            if (y_value < lowest_value)
                lowest_value = y_value;
        }//end foreach
        double value_range = highest_value - lowest_value;
        // Calculate date (x-axis) range
        double latest_date = double.MinValue;
        double earliest_date = double.MaxValue;
        foreach (double x_value in x_refs)
        {
            if (x_value > latest_date)
                latest_date = x_value;
            if (x_value < earliest_date)
                earliest_date = x_value;
        }//end foreach
        double date_range = latest_date - earliest_date;

        // What, roughly, a 1-1 slope should be.
        // y-per-x
        double ideal_y_per_x = value_range / date_range;
        double y_per_x_threshold = 0.3;

        // Look for irregularities in the shape, so we can comment on them.
        // The ideal line is has a 1-1 slope with the graph as a whole.
        // Check for slope of the entire line.        
        DataPoint start_point = critical_points[0];
        DataPoint end_point = critical_points[1];
        double shape_y_per_x = Math.Abs((end_point.y - start_point.y) / (end_point.x - start_point.x));
        string temp_description = "";
        // See if the actual slope is significantly different from the ideal slope.
        if (SignificantlyDifferent(shape_y_per_x, ideal_y_per_x, y_per_x_threshold))
        {
            // Check for a shallow slope
            if (shape_y_per_x < ideal_y_per_x)
            {
                temp_description = "it's overall shallower than you'd expect";
            }//end if
            // Check for steep slope
            else if (shape_y_per_x > ideal_y_per_x)
            {
                temp_description = "it's overall steeper than you'd expect";
            }//end else if
            Descriptor new_descriptor = new Descriptor();
            new_descriptor.descriptions.Add(temp_description);
            // Consider this relating to the end point value.
            new_descriptor.points.Add(end_point);
            abnormal_descriptors.Add(new_descriptor);
        }//end if

        // Reference points at start, 1/4, 1/3, 1/2, 2/3, 3/4, and end.
        // Map x values to fractions that the descriptions will reference.
        Dictionary<double, string> reference_map = new Dictionary<double, string>();
        double line_x_range = end_point.x - start_point.x;
        reference_map.Add(start_point.x, "near the start");
        reference_map.Add(start_point.x + line_x_range * (1f/4f), "about a quarter of the way through");
        reference_map.Add(start_point.x + line_x_range * (1f/3f), "about a third of the way through");
        reference_map.Add(start_point.x + line_x_range * (1f/2f), "about halfway through");
        reference_map.Add(start_point.x + line_x_range * (2f/3f), "about two-thirds of the way through");
        reference_map.Add(start_point.x + line_x_range * (3f/4f), "about three-quarters of the way through");
        reference_map.Add(end_point.x, "near the end");
        
        // Look for inflection points.
        // For each segment, check if they're going in the same direction.
        // If so, check if their slopes are significantly different.
        // If so, check if the magnitude of the second slope is greater than or less than the first.
        Segment current_segment = new Segment();
        Segment next_segment = new Segment();
        // Either 1 (for up) or -1 (for down)
        double current_direction = -1;
        double next_direction = -1;
        double current_slope = 0;
        double next_slope = 0;
        temp_description = "";
        for (int i = 0; i < this.segments.Count - 1; i++)
        {
            current_segment = segments[i];
            next_segment = segments[i + 1];
            // Check direction
            current_direction = (current_segment.end_point.x - current_segment.start_point.x) / Math.Abs(current_segment.end_point.x - current_segment.start_point.x);
            next_direction = (next_segment.end_point.x - next_segment.start_point.x) / Math.Abs(next_segment.end_point.x - next_segment.start_point.x);
            // First, check that it is the same direction as the shape.
            // Positive direction is up, negative is down.
            if (current_direction > 0 && !direction.Equals("up"))
                continue;
            else if (current_direction < 0 && !direction.Equals("down"))
                continue;
            // If they are going in the same direction, this is a possible inflection point.
            if (current_direction == next_direction)
            {
                // Check the slopes.
                current_slope = (current_segment.end_point.y - current_segment.start_point.y) / (current_segment.end_point.x - current_segment.start_point.x);
                next_slope = (next_segment.end_point.y - next_segment.start_point.y) / (next_segment.end_point.x - next_segment.start_point.x);
                // See if they are significantly different.
                if (SignificantlyDifferent(current_slope, next_slope, y_per_x_threshold))
                {
                    temp_description = "";
                    // If they are significantly different, then this is an inflection point.
                    // See if the slope is speeding up (next slope has greater magnitude)
                    if (Math.Abs(next_slope) > Math.Abs(current_slope))
                    {
                        if (direction.Equals("up"))
                            temp_description += " it starts growing faster";
                        else if (direction.Equals("down"))
                            temp_description += " the decline gets faster";
                    }//end if
                    else if (Math.Abs(next_slope) < Math.Abs(current_slope))
                    {
                        if (direction.Equals("up"))
                            temp_description += " it starts slowing down";
                        else if (direction.Equals("down"))
                            temp_description += " it starts falling less quickly";
                    }//end else if
                    string inflection_point_reference = ClosestReference(current_segment.end_point.x, reference_map);
                    //temp_description += " " + inflection_point_reference + " at around " + Math.Round(current_segment.end_point.x).ToString();
                    temp_description += " at around " + Math.Round(current_segment.end_point.x).ToString();

                    Descriptor new_descriptor = new Descriptor();
                    new_descriptor.descriptions.Add(temp_description);
                    new_descriptor.points.Add(current_segment.end_point);
                    abnormal_descriptors.Add(new_descriptor);
                }//end if
            }//end if
        }//end for

        // Make descriptions out of each abnormal shape part.
        string reference_text = "";
        int number_of_deviations = 1;
        foreach (ShapePart abnormal_shape_part in shape_parts)
        {
            Descriptor new_descriptor = new Descriptor();
            reference_text = ClosestReference(abnormal_shape_part.part_segments[0].start_point.x, reference_map);
            temp_description = reference_text;
            temp_description += ", it goes ";
            if (direction.Equals("up"))
                temp_description += "down instead of up";
            else if (direction.Equals("down"))
                temp_description += "up instead of down";
            
            temp_description += " for the " + NumberToCountWord(number_of_deviations) + " time";

            new_descriptor.descriptions.Add(temp_description);
            new_descriptor.points.Add(abnormal_shape_part.part_critical_points[0]);
            new_descriptor.points.Add(abnormal_shape_part.part_critical_points[1]);
            new_descriptor.included_parts.Add(abnormal_shape_part);
            abnormal_descriptors.Add(new_descriptor);
        }//end foreach

        // What we call each critical point.
        List<string> critical_point_names = new List<string>();
        critical_point_names.Add("start of the line");
        critical_point_names.Add("end of the line");

        // Which reference fraction (of the line) the point of interest is closest to.
        string point_of_interest_reference = ClosestReference(point_of_interest.x, reference_map);
        string point_of_interest_hint = "But " + point_of_interest_reference + ", something interesting happens. ";
        // A list of points we select further descriptors around.
        // For a line, just add the point of interest itself.
        List<DataPoint> related_points = new List<DataPoint>();
        related_points.Add(point_of_interest);

        // Gather a limited set of abnormal descriptors that are either about the point of interest 
        // or are about points close to the point of interest.
        List<Descriptor> relevant_abnormal_descriptors = new List<Descriptor>();
        List<Descriptor> unassigned_abnormal_descriptors = new List<Descriptor>();
        foreach (Descriptor abnormal_descriptor in abnormal_descriptors)
        {
            unassigned_abnormal_descriptors.Add(abnormal_descriptor);   
        }//end foreach
        // Grab the descriptor whose point is closest to a point of interest until we have enough.
        int abnormal_descriptor_max = 2;
        int abnormal_descriptor_count = 0;
        int point_counter = 0;
        while(abnormal_descriptor_count < abnormal_descriptor_max)
        {
            // If we are out of unassigned abnormal descriptors, stop the loop.
            if (unassigned_abnormal_descriptors.Count == 0)
                break;
            // Find the unassigned abnormal descriptor that is closest to one of the related points.
            double closest_distance = double.MaxValue;
            double current_distance = 0;
            Descriptor closest_descriptor = new Descriptor();
            foreach (Descriptor current_descriptor in unassigned_abnormal_descriptors)
            {
                current_distance = Math.Abs(current_descriptor.points[0].x - related_points[point_counter].x);
                if (current_distance < closest_distance)
                {
                    closest_distance = current_distance;
                    closest_descriptor = current_descriptor;
                }//end if
            }//end foreach
            // Now that we have the closest abnormal descriptor, add it to the set of relevant descriptors and remove it
            // from the set of unassigned descriptors.
            relevant_abnormal_descriptors.Add(closest_descriptor);
            abnormal_descriptor_count += 1;
            unassigned_abnormal_descriptors.Remove(closest_descriptor);

            // Cycle which related point we look at.
            point_counter += 1;
            if (point_counter >= related_points.Count)
                point_counter = 0;
        }//end while

        // Now that we've gathered which abnormal points we're going to talk about, add them to the
        // set of related points so that critical points can be chosen appropriately. 
        foreach (Descriptor temp_descriptor in relevant_abnormal_descriptors)
        {
            related_points.Add(temp_descriptor.points[0]);
        }//end foreach

        string abnormal_description_text = "";

        int side_counter = 0;
        Descriptor next_descriptor = new Descriptor();
        string first_transition = "";
        string second_transition = "";
        int transition_count = 0;
        // Different transitions between each part (left, middle, or right) 
        // depending on how many parts we're going to talk about.
        first_transition = ", and ";
        second_transition = ". Also, ";

        for (int i = 0; i < relevant_abnormal_descriptors.Count; i++)
        {
            if (i == 0)
                abnormal_description_text += "However, ";
            else if (i == relevant_abnormal_descriptors.Count - 1)
                abnormal_description_text += ", and ";
            else if (side_counter == 0 && transition_count == 1)
                abnormal_description_text += first_transition;
            else if (side_counter == 0 && transition_count == 2)
                abnormal_description_text += second_transition;
            else
                abnormal_description_text += ", ";
            next_descriptor = relevant_abnormal_descriptors[i];
            abnormal_description_text += next_descriptor.descriptions[0];
        }//end for

        if (relevant_abnormal_descriptors.Count > 0)
            abnormal_description_text += ". ";

        // Now that we know what abnormal descriptors are being used, we can choose
        // what critical points we have to use to describe the shape.
        // Use critical point indicies for this.
        List<int> relevant_critical_points = new List<int>();
        List<int> unassigned_critical_points = new List<int>();
        for (int i = 0; i < critical_points.Count; i++)
            unassigned_critical_points.Add(i);
        // Choose the closest critical points to the points relevant to the point of interest.
        int critical_point_limit = 1;
        int critical_point_count = 0;
        point_counter = 0;
        while(critical_point_count < critical_point_limit)
        {
            // If we are out of unassigned critical points, stop the loop.
            if (unassigned_critical_points.Count == 0)
                break;
            // Find the unassigned critical point that is closest to the current related point.
            double closest_distance = double.MaxValue;
            double current_distance = 0;
            int closest_index = -1;
            DataPoint closest_point = new DataPoint();
            DataPoint current_point = new DataPoint();
            foreach (int current_point_index in unassigned_critical_points)
            {
                current_point = critical_points[current_point_index];
                current_distance = Math.Abs(current_point.x - related_points[point_counter].x);
                if (current_distance < closest_distance)
                {
                    closest_distance = current_distance;
                    closest_point = current_point;
                    closest_index = current_point_index;
                }//end if
            }//end foreach
            // Now that we have the closest critical point, add it to the set of relevant critical points and
            // remove it from the set of unassigned critical points.
            relevant_critical_points.Add(closest_index);
            critical_point_count += 1;
            unassigned_critical_points.Remove(closest_index);

            // Cycle what related point we're looking at.
            point_counter += 1;
            if (point_counter >= related_points.Count)
                point_counter = 0;
        }//end while

        string critical_point_text = "";
        // Describe the relevant critical points.
        List<int> points_mentioned = new List<int>();
        // First, look for pairs.
        // Start and end points are a pair.
        if (relevant_critical_points.Contains(0) && relevant_critical_points.Contains(1))
        {
            critical_point_text += "You can see where it starts and ends at ";
            critical_point_text += Math.Round(critical_points[0].x).ToString();
            critical_point_text += " and ";
            critical_point_text += Math.Round(critical_points[4].x).ToString();
            points_mentioned.Add(0);
            points_mentioned.Add(1);
        }//end if

        // Don't re-state any points that were in a pair above.
        foreach (int index in points_mentioned)
            relevant_critical_points.Remove(index);

        for (int i = 0; i < relevant_critical_points.Count; i++)
        {
            if (i == 0 && critical_point_text.Equals(""))
                critical_point_text += "You can see the ";
            else if (i == relevant_critical_points.Count - 1)
                critical_point_text += ", and the ";
            else
                critical_point_text += ", the ";
            critical_point_text += critical_point_names[relevant_critical_points[i]]
            + " at " + Math.Round(critical_points[relevant_critical_points[i]].x).ToString();
            // Note that these points were mentioned.
            points_mentioned.Add(relevant_critical_points[i]);
        }//end for
        critical_point_text += ". ";

        // Finally, generate the description of the shape, overall, using the start and end points.
        string overall_description = "";
        // Describe the shape, overall
        overall_description = "it goes " + direction + " in a line "; //from ";
        //overall_description += Math.Round(critical_points[0].x).ToString();
        //overall_description += " to ";
        //overall_description += Math.Round(critical_points[1].x).ToString();        
        //overall_description += ". ";
        // Try not to state a point if it will be mentioned later.
        // If neither the start nor end point are mentioned later.
        if (!points_mentioned.Contains(0) && !points_mentioned.Contains(1))
        {
            overall_description += " from ";
            overall_description += Math.Round(critical_points[0].x).ToString();
            overall_description += " to ";
            overall_description += Math.Round(critical_points[1].x).ToString();
            overall_description += ". ";
        }//end if
        // If both points are mentioned later.
        else if (points_mentioned.Contains(0) && points_mentioned.Contains(1))
        {
            overall_description += ". ";
        }//end else if
        // If just the start point is mentioned later.
        else if (points_mentioned.Contains(0) && !points_mentioned.Contains(1))
        {
            overall_description += "up until ";
            overall_description += Math.Round(critical_points[1].x).ToString();
            overall_description += ". ";
        }//end else if
        // If just the end point is mentioned later.
        else if (!points_mentioned.Contains(0) && points_mentioned.Contains(1))
        {
            overall_description += "starting from ";
            overall_description += Math.Round(critical_points[0].x).ToString();
            overall_description += ". ";
        }//end else if
        
        // Make the full description.
        description = overall_description;
        // Hint at the point of interest.
        description += point_of_interest_hint;

        // Add critical point descriptors, so the shape is described.
        description += critical_point_text;

        // Add the descriptions of the shape's abnormalities.
        description += abnormal_description_text;

        //description += point_of_interest_hint;
        // === IMPORTANT ===
        // Finally, reveal the information for the point of interest.
        description += "It's " + point_of_interest_reference;
        description += ", at around " + Math.Round(point_of_interest.x).ToString() 
        + ", that the average value for " + variable_name + " at " + site_name 
        + " peaks, reaching " + Math.Round(point_of_interest.y, 4) + " " + y_label;

        description += ". ";

        return description;
    }// end method GenerateDetailedDescription

    // Generate a shallow description of this shape.
    private string GenerateShallowDescription()
    {
        string description = "";

        // Describe the critical points of the shape.
        // The critical points for a W are:
        //  Start point (index 0)
        //  End point (index 1)

        // Describe the shape, overall
        description = "it goes " + direction + " in a line from ";
        description += Math.Round(critical_points[0].x).ToString();
        description += " to ";
        description += Math.Round(critical_points[1].x).ToString();        
        description += ". ";

        return description;
    }//end method GenerateLightDescription

    // Helper Functions

    private string ClosestReference(double value_in, Dictionary<double, string> reference_map)
    {
        string closest_reference = "";
        double closest_distance = double.MaxValue;
        double closest_value = -1;
        foreach (KeyValuePair<double, string> reference_entry in reference_map)
        {
            if (Math.Abs(value_in - reference_entry.Key) < closest_distance)
            {
                closest_distance = Math.Abs(value_in - reference_entry.Key);
                closest_value = value_in;
                closest_reference = reference_entry.Value;
            }//end if
        }//end foreach
        return closest_reference;
    }//end method ClosestReference

    private string NumberToCountWord(double number_in)
    {
        if (number_in == 1)
            return "first";
        else if (number_in == 2)
            return "second";
        else if (number_in == 3)
            return "third";
        else if (number_in == 4)
            return "fourth";
        else if (number_in == 5)
            return "fifth";
        else if (number_in == 6)
            return "sixth";
        else if (number_in == 7)
            return "seventh";
        else if (number_in == 8)
            return "eigth";
        else if (number_in == 9)
            return "ninth";
        else if (number_in == 10)
            return "tenth";
        else
            return "";
    }//end method NumberToCountWord

    // Returns true when the difference between value 1 and value 2 is above the given threshold
    private bool SignificantlyDifferent(double value_1, double value_2, double threshold)
    {
        if (Math.Abs(value_2 - value_1) > threshold)
            return true;
        else
            return false;
    }//end method SignificantlyDifferent
}//end class ShapeW