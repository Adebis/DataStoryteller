using System.Collections.Generic;
using System;

// The shape of the letter "V"
class ShapeV : Shape
{
    // Shape parts
    // The parts of a V shape are the downward leg and the upward leg
    // Indices:
    //  0 = downward leg
    //  1 = upward leg
    private List<ShapePart> shape_parts;

    // Critical Points
    // The critical points of a V shape are the starting peak, the
    // middle trough, and the ending peak.
    // Indices:
    //  0 = starting peak
    //  1 = middle trough
    //  2 = ending peak
    private List<DataPoint> critical_points;

    // A list of descriptors for this shape.
    // Each descriptor is a set of string descriptions for some part
    // of the shape, as well as the points and segments related to
    // the description.
    private List<Descriptor> abnormal_descriptors;

    public ShapeV()
    {
        Initialize();
    }//end constructor ShapeV

    private void Initialize()
    {
        description_type = 0;
        verbose = false;
        shape_name = "v";
        shape_of_interest = false;
        shape_parts = new List<ShapePart>();
        // A V shape has 2 parts.
        // The downward leg, and the upward leg.
        int number_of_shape_parts = 2;
        for (int i = 0; i < number_of_shape_parts; i++)
        {
            shape_parts.Add(new ShapePart());
        }//end for

        critical_points = new List<DataPoint>();

        abnormal_descriptors = new List<Descriptor>();
    }//end method Initialize

    // Match a list of segments to this shape's parts and critical points. Critical points in are x values.
    public override void MatchSegmentsToShape(List<Segment> segments_in, List<double> critical_points_in)
    {
        // First, find data points for all critical points.
        bool match_found = false;
        foreach (double critical_point_x in critical_points_in)
        {
            match_found = false;
            foreach (Segment segment_in in segments_in)
            {
                if (segment_in == segments_in[segments_in.Count - 1])
                    if (verbose)
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
                else if (!SignificantlyDifferent(segment_in.end_point.x, critical_point_x, 1))
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
                if (verbose)
                    Console.WriteLine("No matching datapoint for " + critical_point_x.ToString());
        }//end foreach

        // Divide up the segments by the critical points to place them into shape parts.
        // Go through each segment, adding them to a shape part. When a segment ends at a 
        // critical point, move on to the next shape part and add segments to that.
        //critical_points.Add(segments_in[0].start_point);
        int critical_point_index = 0;
        foreach (Segment segment_in in segments_in)
        {
            // If the end point of the current segment is before the next critical point,
            // add it to the appropriate shape part.
            if (segment_in.end_point.x < critical_points_in[critical_point_index])
            {
                // If it's still at the first critical point index, then this segment isn't even
                // in the shape. Disregard it.
                if (critical_point_index == 0)
                    continue;
                shape_parts[critical_point_index - 1].AddSegment(segment_in);
            }//end if
            else if (segment_in.end_point.x == critical_points_in[critical_point_index])
            {
                // If it's still at the first critical point index, then this segment isn't even
                // in the shape. Disregard it.
                if (critical_point_index == 0)
                    continue;
                // If the end point is at the next critical point, then add it to the appropriate
                // shape part AND note the end point as the critical point.
                shape_parts[critical_point_index - 1].AddSegment(segment_in);
                //critical_points.Add(segment_in.end_point);
                // Increment the critical point index.
                critical_point_index += 1;
            }//end else if
            else if (segment_in.start_point.x == critical_points_in[critical_point_index])
            {
                //critical_points.Add(segment_in.start_point);
                critical_point_index += 1;
                shape_parts[critical_point_index - 1].AddSegment(segment_in);
            }//end else
        }//end foreach
    }//end method MatchSegmentsToShape

    // For the "regular" values of a V:
    //  1. Define the start and end (x values) of the shape as the first and last critical points.
    //  2. Define the top and bottom of the shape:
    //      a. Look at the start and end peaks and the middle trough.
    //      b. Choose the the peak (as the top) that makes the vertical difference of 
    //      the shape most closely match the horizontal difference (is most square).
    //  Check the regularity of the Critical Points.
    //  3. Check if the start and end peak y values are the same.
    //      a. If the start and end peak y values are different, then the odd one out is the one that does not
    //          match the top of the shape. We can then comment that:
    //          * "the first/last part's too tall/short" + "it stretches/only comes up to (y)"
    //  4. Check if the middle trough y value is too high or too low.
    //      a. Calculate the desired middle trough height: the one that makes the vertical difference
    //          of the shape match the horizontal difference (makes the shape square).
    //      b. See if the middle trough y values is within some y-tolerance of this value.
    //      c. If it is not, check if it is above the value or below it. We can then comment that:
    //          * "the middle's too low/high" + "it stretches all the way down/only goes down to (y)"
    //  5. Check if the middle trough x value is halfway between the start and end x values.
    //      a. If the middle trough x value is before the midway point, we can comment that:
    //          * "it's all skewed to the left" + "the middle's at (date)"
    //      b. If the middle trough x value is after the midway point, we can comment that:
    //          * "it's all skewed to the right" + "the middle's at (date)"
    // Generate and return a text description of this shape.
    public override string GenerateDescription(List<double> x_refs, List<double> y_refs, string x_label, string y_label, DataPoint point_of_interest, string site_name, string variable_name)
    {
        string description = "Description for shape 'v': ";
        if (this.shape_of_interest)
        {
            description = GenerateDetailedDescription(x_refs, y_refs, x_label, y_label, point_of_interest, site_name, variable_name);
        }//end if
        else
        {
            description = GenerateShallowDescription(x_refs);
        }//end else

        return description;
    }//end method GenerateDescription

    // Create a more detailed description for this shape, including how it relates to the point of interest.
    private string GenerateDetailedDescription(List<double> x_refs, List<double> y_refs, string x_label, string y_label, DataPoint point_of_interest, string site_name, string variable_name)
    {
        //string description = "Description for shape 'v': ";
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
        double y_per_x = value_range / date_range;

        // Look for irregularities in the shape, so we can commment on them.
        //  1. Define the start and end (x values) of the shape as the first and last critical points.
        DataPoint start_peak = critical_points[0];
        DataPoint end_peak = critical_points[2];
        double start_x = start_peak.x;
        double end_x = end_peak.x;
        //  2. Define the top and bottom of the shape:
        //      a. Look at the start and end peaks and the middle trough.
        //      b. Choose the the peak (as the top) that makes the vertical difference of 
        //      the shape most closely match the horizontal difference (is most square).
        DataPoint top_point = null;
        DataPoint bottom_point = null;
        double top_y = 0;
        double bottom_y = 0;
        DataPoint middle_trough = critical_points[1];
        List<DataPoint> peaks = new List<DataPoint>();
        peaks.Add(start_peak);
        peaks.Add(end_peak);
        List<DataPoint> troughs = new List<DataPoint>();
        troughs.Add(middle_trough);

        // The desired y difference is the one that matches the converted x difference.
        // This allows us to match the ratio of y to x for the graph as a whole.
        double x_difference = end_x - start_x;
        double desired_y_difference = x_difference * y_per_x;

        double closest_y_difference = double.MinValue;
        double current_y_difference = 0;

        foreach (DataPoint peak in peaks)
        {
            foreach (DataPoint trough in troughs)
            {
                current_y_difference = peak.y - trough.y;
                // See if the current y difference is closer to the desired y 
                // difference than the current closest y difference.
                if (Math.Abs(desired_y_difference - current_y_difference) < Math.Abs(desired_y_difference - closest_y_difference))
                {
                    closest_y_difference = current_y_difference;
                    top_point = peak;
                    bottom_point = trough;
                }//end if
            }//end foreach
        }//end foreach
        top_y = top_point.y;
        bottom_y = bottom_point.y;

        //  Check the regularity of the Critical Points.
        //  3. Check if the start and end peak y values are the same.
        //      a. If the start and end peak y values are different, then the odd one out is the one that does not
        //          match the top of the shape. We can then comment that:
        //          * "the first/last part's too tall/short" + "it stretches/only comes up to (y)"

        // If the difference between peak y values is above this value, then they are considered different.
        double y_difference_threshold = 0.1;
        // Check start peak.
        string temp_description = "";
        string temp_numerical_description = "";
        // Check if it's too far from the top point (where both peaks should be)
        if (SignificantlyDifferent(top_point.y, start_peak.y, y_difference_threshold))
        {
            //temp_description = "the first part's ";
            temp_description = "";
            temp_numerical_description = "";
            // Check if it's below the top point.
            if (start_peak.y < top_point.y)
            {
                // If so, then it's too short.
                temp_description += "the shape's not tall enough";
                temp_numerical_description = "only comes up to";
            }//end if
            // Check if it's above the top point.
            else if (start_peak.y > top_point.y)
            {
                // If so, then it's too tall.
                temp_description += "the shape's too tall";
                temp_numerical_description = "goes all the way up to";
            }//end else if
            //temp_description += "on the left";
            temp_numerical_description += " " + FindNearestReference(start_peak.y, y_refs, true) + " " + y_label;
            Descriptor new_descriptor = new Descriptor();
            new_descriptor.descriptions.Add(temp_description);
            new_descriptor.descriptions.Add(temp_numerical_description);
            new_descriptor.points.Add(start_peak);
            new_descriptor.shape_part.Add("left");
            abnormal_descriptors.Add(new_descriptor);
            //start_peak.description = temp_description;
            //start_peak.numerical_description = temp_numerical_description;
        }//end if
        // Check the end peak.
        if (Math.Abs(top_point.y - end_peak.y) > y_difference_threshold)
        {
            temp_description = "";
            temp_numerical_description = "";
            // Check if it's below the top point.
            if (end_peak.y < top_point.y)
            {
                // If so, then it's too short.
                temp_description += "the shape's not tall enough";
                temp_numerical_description = "only comes up to";
            }//end if
            // Check if it's above the top point.
            else if (end_peak.y > top_point.y)
            {
                // If so, then it's too tall.
                temp_description += "the shape's too tall";
                temp_numerical_description = "stretches all the way up to";
            }//end else if
            //temp_description += " on the right";
            temp_numerical_description += " " + FindNearestReference(end_peak.y, y_refs, true) + " " + y_label;
            Descriptor new_descriptor = new Descriptor();
            new_descriptor.points.Add(end_peak);
            new_descriptor.descriptions.Add(temp_description);
            new_descriptor.descriptions.Add(temp_numerical_description);
            new_descriptor.shape_part.Add("right");
            abnormal_descriptors.Add(new_descriptor);
        }//end if

        //  4. Check if the middle trough y value is too high or too low.
        //      a. Calculate the desired middle trough height: the one that makes the vertical difference
        //          of the shape match the horizontal difference (makes the shape square).
        //      b. See if the middle trough y values is within some y-tolerance of this value.
        //      c. If it is not, check if it is above the value or below it. We can then comment that:
        //          * "the middle's too low/high" + "it stretches all the way down/only goes down to (y)"

        double desired_middle_y = top_y - desired_y_difference;
        // Check if the middle trough y value is significanty different from the desired y value.
        if (SignificantlyDifferent(desired_middle_y, middle_trough.y, y_difference_threshold * 2))
        {
            temp_description = "";
            temp_numerical_description = "";
            // If so, check if it's above the desired point.
            if (middle_trough.y > desired_middle_y)
            {
                temp_description += "too high up";
            }//end if
            // Check if it's below the desired point.
            else if (middle_trough.y < desired_middle_y)
            {
                temp_description += "too low down";
            }//end else if

            // Only actually add it as a description if the middle part had something to describe.
            if (!temp_description.Equals(""))
            {
                //temp_description += " in the middle";
                temp_numerical_description += " " + FindNearestReference(middle_trough.y, y_refs, true) + " " + y_label;
                Descriptor new_descriptor = new Descriptor();
                new_descriptor.points.Add(middle_trough);
                new_descriptor.descriptions.Add(temp_description);
                new_descriptor.descriptions.Add(temp_numerical_description);
                new_descriptor.shape_part.Add("middle");
                abnormal_descriptors.Add(new_descriptor);
            }//end if
        }//end if

        //  5. Check if the middle trough x value is halfway between the start and end x values.
        //      a. If the middle trough x value is before the midway point, we can comment that:
        //          * "it's all skewed to the left" + "the middle's at (date)"
        //      b. If the middle trough x value is after the midway point, we can comment that:
        //          * "it's all skewed to the right" + "the middle's at (date)"
        double middle_x = end_x - ((end_x - start_x) / 2);
        double x_difference_threshold = 1.0;
        // Check middle peak x position
        if (SignificantlyDifferent(middle_trough.x, middle_x, x_difference_threshold))
        {
            temp_description = "";
            string secondary_temp_description = "";
            temp_numerical_description = "";
            Descriptor new_descriptor = new Descriptor();
            // Check if it's before the middle x.
            if (middle_trough.x < middle_x)
            {
                //temp_description += "the right side's too long";
                temp_description += "too long";
                secondary_temp_description += "skewed too far to the left";
                new_descriptor.shape_part.Add("right");
            }//end if
            else if (middle_trough.x > middle_x)
            {
                //temp_description += "the left side's too long";
                temp_description += "too long";
                secondary_temp_description += "skewed too far to the right";
                new_descriptor.shape_part.Add("left");
            }//end else if
            // Secondarily, also a description of the middle.
            new_descriptor.shape_part.Add("middle");
            temp_numerical_description += "the middle's at" + FindNearestReference(middle_trough.x, x_refs);
            new_descriptor.points.Add(middle_trough);
            new_descriptor.descriptions.Add(temp_description);
            new_descriptor.descriptions.Add(secondary_temp_description);
            //new_descriptor.descriptions.Add(temp_numerical_description);
            abnormal_descriptors.Add(new_descriptor);
        }//end if 

        //  Check the regularity of the Parts.
        // Regular values for the Parts are:
        //  - First leg is symmetric to last leg about the middle point.
        //  - Second leg is symmetry to second-to-last leg about the middle point.
        //  - The slope of the second leg is about the same (inverted) as the first leg.
        //  - The slope of the second-to-last leg is about the same (inverted) as the last leg.
        //  - The segments for each Part must be close to each other in slope.

        // Locate the point of interest within the shape.
        // What we call each critical point.
        List<string> critical_point_names = new List<string>();
        critical_point_names.Add("the start"); //of the 'v'
        critical_point_names.Add("the middle");
        critical_point_names.Add("the end");

        // Which side is the point of interest on; left, middle, or right?
        string point_of_interest_side = "";
        if (!SignificantlyDifferent(point_of_interest.x, critical_points[1].x, x_difference_threshold))
            point_of_interest_side = "middle";
        else if (point_of_interest.x < critical_points[1].x)
            point_of_interest_side = "left";
        else if (point_of_interest.x > critical_points[1].x)
            point_of_interest_side = "right";

        // Keep track of which points we have talked about in relation to the point of interest.
        List<DataPoint> related_points = new List<DataPoint>();
        string point_of_interest_hint = "";
        // First, see if the point of interest is exactly one of the critical points.
        int matching_point_index = -1;
        int lower_point_index = -1;
        int upper_point_index = -1;
        point_of_interest_hint = ", with something interesting happening";
        for (int i = 0; i < critical_points.Count; i++)
        {
            // If the difference between the critical point's x value and this point's x value isn't significant, then consider it a match.
            if (!SignificantlyDifferent(point_of_interest.x, critical_points[i].x, x_difference_threshold))
            {
                matching_point_index = i;
                break;
            }//end if
        }//end for
        // If there has been a match, then describe just the matching critical point.
        if (matching_point_index != -1)
        {
            // Add the matching point as a related point.
            related_points.Add(critical_points[matching_point_index]);
            // === IMPORTANT ===
            // Explicitly tell the user that something interesting happens near this point.
            point_of_interest_hint += " at " + critical_point_names[matching_point_index] + ". But we'll get back to that later. ";
            //description += 
        }//end if
        // If there has not been a match, find two points that the point of interest lies between.
        else
        {
            for (int i = 0; i < critical_points.Count - 1; i++)
            {
                if (critical_points[i].x < point_of_interest.x
                && critical_points[i + 1].x > point_of_interest.x)
                {
                    lower_point_index = i;
                    upper_point_index = i + 1;
                    break;
                }//end if
            }//end for

            // Add the lower and upper points as related points.
            related_points.Add(critical_points[lower_point_index]);
            related_points.Add(critical_points[upper_point_index]);

            // === IMPORTANT ===
            // Explicitly tell the user that something interesting happens between these two points.
            point_of_interest_hint += " between " + critical_point_names[lower_point_index] + " and " + critical_point_names[upper_point_index] + ". But more on that later.";
            //description += 
        }//end else

        // Add abnormal descriptors.
        //description = "";
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

        // Group all left-side and right-side abnormal descriptors.
        List<Descriptor> left_abnormal_descriptors = new List<Descriptor>();
        List<Descriptor> right_abnormal_descriptors = new List<Descriptor>();
        List<Descriptor> middle_abnormal_descriptors = new List<Descriptor>();
        foreach (Descriptor temp_descriptor in relevant_abnormal_descriptors)
        {
            if (temp_descriptor.shape_part.Contains("left"))
            {
                // If this descriptor also describes the middle, check which side the point of interest is on.
                if (temp_descriptor.shape_part.Contains("middle") && point_of_interest_side.Equals("middle"))
                {
                    // If the point of interest is in the middle, talk about this like it's about a middle point.
                    temp_descriptor.SwitchDescriptions(0, 1);
                    middle_abnormal_descriptors.Add(temp_descriptor);
                }//end if
                else
                    left_abnormal_descriptors.Add(temp_descriptor);
            }//end if
            else if (temp_descriptor.shape_part.Contains("right"))
            {
                // If this descriptor also describes the middle, check which side the point of interest is on.
                if (temp_descriptor.shape_part.Contains("middle") && point_of_interest_side.Equals("middle"))
                {
                    // If the point of interest is in the middle, talk about this like it's about a middle point.
                    temp_descriptor.SwitchDescriptions(0, 1);
                    middle_abnormal_descriptors.Add(temp_descriptor);
                }//end if
                else
                    right_abnormal_descriptors.Add(temp_descriptor);
            }//end else if
            else if (temp_descriptor.shape_part.Contains("middle"))
                middle_abnormal_descriptors.Add(temp_descriptor);
        }//end foreach

        string abnormal_description_text = "";

        int side_counter = 0;
        Descriptor next_descriptor = new Descriptor();
        string first_transition = "";
        string second_transition = "";
        int transition_count = 0;
        // Different transitions between each part (left, middle, or right) 
        // depending on how many parts we're going to talk about.
        int number_of_parts = 0;
        if (left_abnormal_descriptors.Count > 0)
            number_of_parts += 1;
        if (right_abnormal_descriptors.Count > 0)
            number_of_parts += 1;
        if (middle_abnormal_descriptors.Count > 0)
            number_of_parts += 1;
        if (number_of_parts == 2)
            first_transition = ", and ";
        else if (number_of_parts == 3)
        {
            first_transition = ", and ";
            second_transition = ". Also, ";
        }//end else if

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
            if (left_abnormal_descriptors.Count > 0)
            {
                next_descriptor = left_abnormal_descriptors[side_counter];
                // If this is about the left AND the middle, then it's talking about how the left side's too long.
                if (next_descriptor.shape_part.Contains("middle"))
                    abnormal_description_text += "the whole left side looks ";
                else if (side_counter == 0)
                    abnormal_description_text += "on the left side, ";

                abnormal_description_text += next_descriptor.descriptions[0];
                // If we've reached the end of the left-side abnormal descriptors, empty its list so we don't go through them again.
                if (side_counter + 1 == left_abnormal_descriptors.Count)
                {
                    side_counter = -1;
                    transition_count += 1;
                    left_abnormal_descriptors = new List<Descriptor>();
                }//end if
            }//end if
            else if (right_abnormal_descriptors.Count > 0)
            {
                next_descriptor = right_abnormal_descriptors[side_counter];
                // If this is about the right AND the middle, then it's talking about how the right side's too long.
                if (next_descriptor.shape_part.Contains("middle"))
                    abnormal_description_text += "the whole right side looks ";
                else if (side_counter == 0)
                    abnormal_description_text += "on the right side, ";

                abnormal_description_text += next_descriptor.descriptions[0];
                // If we've reached the end of the right-side abnormal descriptors, empty its list so we don't go through them again.
                if (side_counter + 1 == right_abnormal_descriptors.Count)
                {
                    side_counter = -1;
                    transition_count += 1;
                    right_abnormal_descriptors = new List<Descriptor>();
                }//end if
            }//end else if
            else if (middle_abnormal_descriptors.Count > 0)
            {
                next_descriptor = middle_abnormal_descriptors[side_counter];

                if (side_counter == 0)
                    abnormal_description_text += "the middle point's ";

                abnormal_description_text += next_descriptor.descriptions[0];
                // If we've reached the end of the middle abnormal descriptors, empty its list so we don't go through them again.
                if (side_counter + 1 == middle_abnormal_descriptors.Count)
                {
                    side_counter = -1;
                    transition_count += 1;
                    middle_abnormal_descriptors = new List<Descriptor>();
                }//end if
            }//end else if

            side_counter += 1;
        }//end for

        abnormal_description_text += ". ";

        // Now that we know what abnormal descriptors are being used, we can choose
        // what critical points we have to use to describe the shape.
        // Use critical point indicies for this.
        List<int> relevant_critical_points = new List<int>();
        List<int> unassigned_critical_points = new List<int>();
        for (int i = 0; i < critical_points.Count; i++)
            unassigned_critical_points.Add(i);
        // Choose the closest critical points to the points relevant to the point of interest.
        int critical_point_limit = 2;
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
        if (relevant_critical_points.Contains(0) && relevant_critical_points.Contains(2))
        {
            critical_point_text += "You can see where the shape starts and ends at ";
            critical_point_text += FindNearestReference(critical_points[0].x, x_refs);
            critical_point_text += " and ";
            critical_point_text += FindNearestReference(critical_points[4].x, x_refs);
            points_mentioned.Add(0);
            points_mentioned.Add(4);
        }//end if

        // Don't re-state any points that were in a pair above.
        foreach (int index in points_mentioned)
            relevant_critical_points.Remove(index);

        bool first_part_of_sentence = true;
        bool middle_part_of_sentence = false;
        bool last_part_of_sentence = false;
        for (int i = 0; i < relevant_critical_points.Count; i++)
        {

            if (i == 0 && critical_point_text.Equals(""))
            {
                first_part_of_sentence = true;
                middle_part_of_sentence = false;
                last_part_of_sentence = false;
            }//end if
            else if (i == relevant_critical_points.Count - 1)
            {
                first_part_of_sentence = false;
                middle_part_of_sentence = false;
                last_part_of_sentence = true;
            }//end else if
            else
            {
                first_part_of_sentence = false;
                middle_part_of_sentence = true;
                last_part_of_sentence = false;
            }//end else

            // Pre-information part of this phrase
            if (first_part_of_sentence)
                critical_point_text += "You can see ";
            else if (last_part_of_sentence)
                critical_point_text += ", and ";
            else if (middle_part_of_sentence)
                critical_point_text += ", ";

            critical_point_text += critical_point_names[relevant_critical_points[i]];
            // Whether we reference the shape name or not
            if (first_part_of_sentence)
                critical_point_text += " of the 'v'";
            else if (middle_part_of_sentence)
                critical_point_text += " of it";
            critical_point_text += " at " + FindNearestReference(critical_points[relevant_critical_points[i]].x, x_refs);

            // Note that these points were mentioned.
            points_mentioned.Add(relevant_critical_points[i]);
        }//end for
        critical_point_text += ". ";

        // Finally, generate the description of the shape, overall, using the start and end points.
        //string shape_name = "w";
        string overall_description = "";
        overall_description = "it makes a '" + shape_name + "' shape";
        // Try not to state a point if it will be mentioned later.
        // If neither the start nor end point are mentioned later.
        if (!points_mentioned.Contains(0) && !points_mentioned.Contains(2))
        {
            overall_description += " from ";
            overall_description += FindNearestReference(critical_points[0].x, x_refs);
            overall_description += " to ";
            overall_description += FindNearestReference(critical_points[4].x, x_refs);
            overall_description += "";
        }//end if
        // If both points are mentioned later.
        else if (points_mentioned.Contains(0) && points_mentioned.Contains(2))
        {
            overall_description += "";
        }//end else if
        // If just the start point is mentioned later.
        else if (points_mentioned.Contains(0) && !points_mentioned.Contains(2))
        {
            overall_description += " up until ";
            overall_description += FindNearestReference(critical_points[4].x, x_refs);
            overall_description += "";
        }//end else if
        // If just the end point is mentioned later.
        else if (!points_mentioned.Contains(0) && points_mentioned.Contains(2))
        {
            overall_description += " starting from ";
            overall_description += FindNearestReference(critical_points[0].x, x_refs);
            overall_description += "";
        }//end else if

        // === IMPORTANT ===
        string point_of_interest_text = "";
        // Finally, reveal the information for the point of interest.
        if (matching_point_index != -1)
        {
            point_of_interest_text += "Interestingly, near " + critical_point_names[matching_point_index]; 
        }//end if
        else
        {
            point_of_interest_text += "Interestingly, between " + critical_point_names[lower_point_index]
            + " and " + critical_point_names[upper_point_index];
        }//end else

        point_of_interest_text += ", at around " + FindNearestReference(point_of_interest.x, x_refs) 
        + ", the average value for " + variable_name + " at " + site_name 
        + " peaks, reaching " + Math.Round(point_of_interest.y, 3) + " " + y_label;

        point_of_interest_text += ". ";
        
        // Make the full description.
        description = overall_description;
        
        // No hint if it's description type 1 or 2.
        if (description_type == 1
            || description_type == 2)
            description += ". ";
        else
            // Hint at the point of interest.
            description += point_of_interest_hint;

        // Add critical point descriptors, so the shape is described.
        description += critical_point_text;

        // Add the descriptions of the shape's abnormalities.
        description += abnormal_description_text;

        // No point of interest if it's description type 2.
        if (description_type == 2)
            description += "";
        else
        // Add the description of the point of interest.
            description += point_of_interest_text;

        return description;
    }// end method GenerateDetailedDescription

    // Generate a shallow description of this shape.
    private string GenerateShallowDescription(List<double> x_refs)
    {
        string description = "";

        // Describe the shape, overall
        //string shape_name = "v";
        description = "the graph resembles a '" + shape_name + "' from ";
        description += FindNearestReference(critical_points[0].x, x_refs);
        description += " to ";
        description += FindNearestReference(critical_points[2].x, x_refs);        
        description += ". ";

        return description;
    }//end method GenerateLightDescription

    // Helper Functions

    // Returns true when the difference between value 1 and value 2 is above the given threshold
    private bool SignificantlyDifferent(double value_1, double value_2, double threshold)
    {
        if (Math.Abs(value_2 - value_1) > threshold)
            return true;
        else
            return false;
    }//end method SignificantlyDifferent
}//end class ShapeW