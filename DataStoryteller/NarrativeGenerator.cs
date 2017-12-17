using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NarrativeGenerator
{
    // The set of constraint weights.
    private double hierarchy_weight = 1;
    // Chronological constraint prefers segments whose start time is closer to the start time
    // of the last segment and which goes forward in the chronology.
    private double chronological_weight = -100;

    // Preference for increasing the coverage towards the limit.
    private double coverage_weight = 1;
    // Preference for placing more space between observations.
    private double spacing_weight = 1;
    // Preference for placing observations on the same segment.
    private double stacking_weight = -1;

    private string y_label;
    private string x_label;
    private double starting_year;
    private string global_shape;

    private List<Segment> all_segments;

    private List<Segment> all_segments_chronological;

    private List<Segment> all_segments_hierarchical;

    public NarrativeGenerator()
    {
        //this.GenerateNarrative();
    }//end constructor NarrativeGenerator

    public void GenerateNarrative(string data_file_name, string info_file_name, double starting_year)
    {
        // First, read the input data.
        // Set 1
        //string data_file_name = "from_output_1.csv";
        //string info_file_name = "bb_cl.json";
        //this.starting_year = 1980;
        // Set 2
        //string data_file_name = "from_output_2.csv";
        //string info_file_name = "an_mg.json";
        //double starting_year = 2012;
        this.starting_year = starting_year;
        all_segments = this.ReadInputCSV(data_file_name);
        JObject info = this.ReadInputInfo(info_file_name);
        // Go through the info file and extract:
        //      site, the site name
        //      var, the variable name
        //      x_label, the label of the x-axis
        //      y_label, the label of the y-axis
        //      x_refs, the list of reference values for the x-axis
        //      y_refs, the list of reference values for the y-axis
        string site_name = info["info"].Value<string>("site");
        string variable_name = info["info"].Value<string>("var");
        y_label = info["info"].Value<string>("y_label");
        x_label = info["info"].Value<string>("x_label");
        global_shape = info["info"].Value<string>("global_shape");
        // Get the list of reference values for both axis.
        List<double> x_refs = new List<double>();
        foreach (JToken x_ref_entry in info["info"]["x_refs"])
        {
            //double converted_x_ref = x_ref_entry.ToObject<double>();
            //converted_x_ref = converted_x_ref * 365 - (1981 * 365);
            //x_refs.Add(converted_x_ref);
            x_refs.Add(x_ref_entry.ToObject<double>());
        }//end foreach
        List<double> y_refs = new List<double>();
        foreach (JToken y_ref_entry in info["info"]["y_refs"])
        {
            y_refs.Add(y_ref_entry.ToObject<double>());
        }//end foreach
        Console.WriteLine("Done reading input files.");

        this.all_segments_chronological = new List<Segment>();
        this.all_segments_hierarchical = new List<Segment>();

        foreach (Segment temp_segment in this.all_segments)
            this.all_segments_chronological.Add(temp_segment);

        // Define sub and super-segments.
        CreateHierarchy(this.all_segments);

        // We now have a list of segments.
        // Calculate and define numerical observations for each segment.
        foreach (Segment temp_segment in all_segments)
        {
            DefineObservations(temp_segment);
        }//end foreach

        // Fill out value references and give words to numerical observations.
        DefineDescriptors(all_segments, x_refs, y_refs);

        string global_trend_descriptor = "";
        double global_change = all_segments[all_segments.Count - 1].end_point.y - all_segments[0].start_point.y;
        if (global_change < 0)
            global_trend_descriptor = "decreases";
        else
            global_trend_descriptor = "increases";

        // Determine matching occurences. Requires a list of segments in chronological
        // order, with descriptors for each numerical observation.
        DetermineMatchingOccurrences(this.all_segments_chronological);

        // How many y-axis values should be given.
        //double y_value_count = 2;
        // How many x-axis values should be given.
        //double x_value_count = 2;

        // Determine the ordering for all the segments.
        this.all_segments = DetermineOrdering(this.all_segments);

        // Decide how each part of each segment is going to be presented.
        DefinePresentation(this.all_segments, x_refs, y_refs);

        // Assemble the description
        string description = "";

        // Global descriptors
        description += "Let me tell you about " + variable_name + " at " + site_name; 
        description += " from " + x_refs[0].ToString() + " to " + x_refs[x_refs.Count - 1].ToString() + ".";
        //description += " generally " + global_trend_descriptor + ".";
        /*string description_all = description;
        foreach (Segment temp_segment in all_segments)
        {
            description_all += " From " + temp_segment.start_date.ToString() + " to " + temp_segment.end_date.ToString();
            description_all += " start " + temp_segment.start_value_descriptor + " end " + temp_segment.end_value_descriptor;
            description_all += " " + temp_segment.direction_descriptor + " " + temp_segment.rate_descriptor;
            description_all += ".";
        }//end foreach
        Console.WriteLine("Description all: " + description_all);*/
        int segment_counter = 0;
        Random rand = new Random();
        int transition_choice = 0;
        int last_transition_choice = -1;
        int number_of_transitions = 3;
        Segment last_segment = null;
        foreach (Segment temp_segment in all_segments)
        {
            if (temp_segment.id == -1)
            {
                // Use a different transition for the global segment.
                description += " Generally, it";
            }//end if
            else if (segment_counter == 0)
                description += " At first, it";
            else if (segment_counter == all_segments.Count() - 1)
                description += " Finally, it";
            else if (last_segment != null 
                    && last_segment.HasSubsegment(temp_segment.id))
            {
                // If the last segment was a supersegment of the current segment, use a different transition.
                description += " During that time, first it";
            }//end else if
            else
            {
                transition_choice = rand.Next(number_of_transitions);
                if (transition_choice == last_transition_choice)
                {
                    transition_choice += 1;
                    if (transition_choice > number_of_transitions - 1)
                        transition_choice = 0;
                }//end if
                if (transition_choice == 0)
                    description += " Then, it";
                else if (transition_choice == 1)
                    description += " From there, it";
                else if (transition_choice == 2)
                    description += " After that, it";
                else if (transition_choice == 3)
                    description += " Following that, it";
                last_transition_choice = transition_choice;
            }//end else
            description += " " + ActualizeDescription(temp_segment, x_refs, y_refs, global_trend_descriptor) + ".";

            segment_counter += 1;
            last_segment = temp_segment;
        }//end foreach

        Console.WriteLine("Description: " + description);
    }//end method GenerateNarrative

    // From a list of segments in chronological order, determine when matching occurences happen.
    public void DetermineMatchingOccurrences(List<Segment> segments_in)
    {
        foreach(Segment temp_segment in segments_in)
        {
            temp_segment.ResetOccurences();
            for (int i = 0; i < temp_segment.observations.Length; i++)
            {
                int current_segment_id = temp_segment.id;
                string current_description = temp_segment.GetObservationDescription(i);
                // Get the ID of the last segment whose observation in index i
                // matches the description of this segment's observation in index i.
                int last_segment_id = -1;
                // After it passes this segment's id, get the first segment whose observation
                // in index i matches the description of this segment's observation in index i.
                int next_segment_id = -1;
                bool current_segment_passed = false;
                foreach (Segment other_segment in segments_in)
                {
                    int other_segment_id = other_segment.id;
                    string other_description = other_segment.GetObservationDescription(i);
                    if (other_segment_id == current_segment_id)
                    {
                        current_segment_passed = true;
                        continue;
                    }//end if
                    if (other_description.Equals(current_description))
                        {
                        // Since the list of segments is in chronological order, if we haven't
                        // reached the current segment yet, we're still looking in previous segments.
                        // Once we find it, keep looking to find the last one.
                        if (!current_segment_passed)
                            last_segment_id = other_segment_id;
                        // Otherwise, we're looking in segments to come. Once we find it, stop looking.
                        else if (current_segment_passed)
                        {
                            next_segment_id = other_segment_id;
                            break;
                        }//end else if
                    }//end if
                }//end foreach
                temp_segment.last_occurences[i] = last_segment_id;
                temp_segment.next_occurences[i] = next_segment_id;
            }//end for
        }//end foreach
    }//end method DetermineMatchingOccurences

    // Determine the ordering for the segments based on hierarchical and chronological constraints.
    public List<Segment> DetermineOrdering(List<Segment> segments_in)
    {
        // Ordering constraints: Hierarchical and chronological.
        // Hierarchical constraint prefers segments that are higher
        // in the overall hierarchy first. 
        //double hierarchy_weight = 1;
        // Chronological constraint prefers segments whose start time is closer to the start time
        // of the last segment and which goes forward in the chronology.
        //double chronological_weight = 1;
        // Chronological constraint score can be infinite if two segments start at the same time.
        // Provide a ceiling.
        double chronological_score_limit = 100;

        List<Segment> unordered_segments = new List<Segment>();
        foreach (Segment input_segment in segments_in)
            unordered_segments.Add(input_segment);
        List<Segment> ordered_segments = new List<Segment>();
        Segment last_segment_added = null;
        Segment next_best_segment = null;
        double next_best_segment_score = double.MinValue;

        double current_segment_score = 0;
        double hierarchy_score = 0;
        double chronological_score = 0;
        double last_start_time = 0;
        for (int i = 0; i < segments_in.Count; i++)
        {
            foreach (Segment current_segment in unordered_segments)
            {
                // Calculate the score for adding this segment next in the ordering.
                current_segment_score = 0;
                
                // hierarchy_score = total number of segments under this segment in hierarchy * hierarchy_weight
                hierarchy_score = current_segment.GetSubsegmentCount() * hierarchy_weight;
                
                // The chronological constraint score should be smaller if the current segment starts farther away from the last segment,
                // and negative if it starts before the last segment.
                // chronological_score = inverse of the distance between the current segment's start time and the last start time * chronological_weight
                chronological_score = Math.Min((1 / (current_segment.start_point.x - last_start_time)) * chronological_weight, chronological_score_limit);

                current_segment_score = hierarchy_score + chronological_score;
                Console.WriteLine("ID: " + current_segment.id.ToString() + " h_score: " + hierarchy_score.ToString() + " c_score: " + chronological_score.ToString() + " total_score: " + current_segment_score.ToString());
                if (current_segment_score > next_best_segment_score)
                {
                    next_best_segment = current_segment;
                    next_best_segment_score = current_segment_score;
                }//end if
            }//end foreach
            // Now we have the next best segment to add to the ordering.
            Console.WriteLine("Next best segment, ID: " + next_best_segment.id.ToString() + " score: " + next_best_segment_score.ToString());
            ordered_segments.Add(next_best_segment);
            unordered_segments.Remove(next_best_segment);
            last_segment_added = next_best_segment;
            last_start_time = next_best_segment.start_point.x;
            next_best_segment_score = double.MinValue;
        }//end for

        // DEBUG: Print ordering.
        Console.WriteLine("Segment Order: ");
        foreach (Segment temp_segment in ordered_segments)
        {
            Console.WriteLine("     " + temp_segment.id.ToString());
        }//end foreach
        return ordered_segments;
    }//end method DetermineOrdering

    // Give words to a segment's presentation.
    public string ActualizeDescription(Segment segment_in, List<double> x_refs, List<double> y_refs, string global_trend_descriptor)
    {
        string presentation_string = "";

        // Decide whether or not to use numbers for the x and y values for this segment.
        bool use_numerical_x = false;
        bool use_numerical_y = false;
        /*Random rand = new Random((int)DateTime.UtcNow.Ticks);
        int rand_int = rand.Next(2);
        int rand_int_2 = rand.Next(2);

        if (rand_int == 0)
            use_numerical_x = false;
        else if (rand_int == 1)
            use_numerical_x = true;

        if (rand_int_2 == 0)
            use_numerical_y = false;
        else if (rand_int_2 == 1)
            use_numerical_y = true;*/
        if (segment_in.use_numerical_x == 0)
            use_numerical_x = true;
        if (segment_in.use_numerical_y == 0)
            use_numerical_y = true;

        // Whether or not to state occurences.
        // If the direction of the segment is dissimilar from the global direction,
        // use occurence counts.
        bool use_occurences = false;
        if (!segment_in.GetObservationDescription(7).Equals(global_trend_descriptor))
            use_occurences = true;
        /*Random random = new Random();
        int rand_int = random.Next(2);
        if (rand_int == 0)
            use_occurences = false;
        else if (rand_int == 1)
            use_occurences = true;*/

        Console.WriteLine("Segment " + segment_in.id.ToString() + " Use numerical x: " + use_numerical_x.ToString() + " y: " + use_numerical_y.ToString() + ", use_occurences: " + use_occurences.ToString());

        // Slope
        presentation_string += ActualizeSlopeDescription(segment_in, use_occurences, x_refs);
        // Then y and x values
        presentation_string += ActualizeXYDescription(segment_in, use_numerical_x, use_numerical_y, x_refs, y_refs);
        // And end with the segment's shape, if any.
        if (!segment_in.shape.Equals(""))
            presentation_string += ", making a shape that looks like a '" + segment_in.shape + "'";
        return presentation_string;
    }//end method ActualizeDescription
    private string ActualizeSlopeDescription(Segment segment_in, bool use_occurences, List<double> x_refs)
    {
        string return_string = "";

        if (segment_in.slope_presentation == 0)
        {
            return_string = segment_in.GetObservationDescription(7);
        }//end if
        else if (segment_in.slope_presentation == 1)
        {
            return_string = segment_in.GetObservationDescription(7) + " " + segment_in.GetObservationDescription(6);
        }//end else if

        // Check for direction match.
        if (use_occurences)
        {
            return_string += " for the first time";
            if (segment_in.last_occurences[7] != -1)
            {
                Segment last_segment = all_segments[segment_in.last_occurences[7]];
                // Get the date
                return_string += " since " + FindNearestReference(last_segment.GetObservationValue(0), x_refs).ToString(); //+ Math.Floor(last_segment.GetObservationValue(2)).ToString();
            }//end if
            return_string += " and for the last time";
            if (segment_in.next_occurences[7] != -1)
            {
                Segment next_segment = all_segments[segment_in.next_occurences[7]];
                // Get the date
                return_string += " until " + FindNearestReference(next_segment.GetObservationValue(0), x_refs).ToString();
            }//end if
            //return_string += ",";
        }//end if

        return return_string;
    }//end method ActualizeSlopeDescription
    private string ActualizeXYDescription(Segment segment_in, bool use_numerical_x, bool use_numerical_y, List<double> x_refs, List<double> y_refs)
    {
        // Presentations by id:
        //  0: start_y and end_y
        //  1: start_y and y_change
        //  2: y_change and end_y
        //  3: y_change
        //  4: none
        // use_numerical:
        //  true: Numerical values (using tick mark references)
        //  false: Non-numerical descriptions

        string return_string = "";

        // transition_0 + x_change + transition_1 + start_y + transition_2 + start_x + transition_3 + y_change + transition_4 + end_y + transition_5 + end_x
        string[] transitions = new string[6];
        string[] descriptions = new string[6];
        for (int i = 0; i < transitions.Length; i++)
            transitions[i] = "";
        for (int i = 0; i < descriptions.Length; i++)
            descriptions[i] = "";

        string start_y_string = "";
        string end_y_string = "";
        string y_change_string = "";
        if (use_numerical_y)
        {
            start_y_string = FindNearestReference(segment_in.GetObservationValue(2), y_refs).ToString() + " " + this.y_label;
            end_y_string = FindNearestReference(segment_in.GetObservationValue(3), y_refs).ToString() + " " + this.y_label;
            y_change_string = "about " + Math.Round(segment_in.GetObservationValue(5), 1).ToString() + " " + this.y_label;
        }//end if
        else
        {
            start_y_string = segment_in.GetObservationDescription(2);
            end_y_string = segment_in.GetObservationDescription(3);
            y_change_string = "a " + segment_in.GetObservationDescription(5) + " amount";
        }//end else

        string start_x_string = "";
        string end_x_string = "";
        string x_change_string = "";
        if (use_numerical_x)
        {
            start_x_string = FindNearestReference(segment_in.GetObservationValue(0), x_refs).ToString();
            end_x_string = FindNearestReference(segment_in.GetObservationValue(1), x_refs).ToString();
            double years_value = Math.Round(segment_in.GetObservationValue(4));
            if (years_value < 1)
                x_change_string = "less than a year,";
            else if (years_value ==  1)
                x_change_string = "a year,";
            else
                x_change_string = Math.Round(segment_in.GetObservationValue(4)).ToString() + " years";
        }//end if
        else
        {
            start_x_string = segment_in.GetObservationDescription(0);
            end_x_string = segment_in.GetObservationDescription(1);
            x_change_string = segment_in.GetObservationDescription(4) + " time";

            start_x_string = "";
            end_x_string = "";
        }//end else
        
        // Set description strings in the array according to the x and y presentation IDs.
        //  0: start_y and end_y
        //  1: start_y and y_change
        //  2: y_change and end_y
        //  3: y_change
        //  4: none
        // transition_0 + x_change + transition_1 + start_y + transition_2 + start_x + transition_3 + y_change + transition_4 + end_y + transition_5 + end_x
        if (segment_in.x_presentation == 1 || segment_in.x_presentation == 2 || segment_in.x_presentation == 3)
            descriptions[0] = x_change_string;
        if (segment_in.y_presentation == 0 || segment_in.y_presentation == 1)
            descriptions[1] = start_y_string;
        if (segment_in.x_presentation == 0 || segment_in.x_presentation == 1)
            descriptions[2] = start_x_string;
        if (segment_in.y_presentation == 1 || segment_in.y_presentation == 2 || segment_in.y_presentation == 3)
            descriptions[3] = y_change_string;
        if (segment_in.y_presentation == 0 || segment_in.y_presentation == 2)
            descriptions[4] = end_y_string;
        if (segment_in.x_presentation == 0 || segment_in.x_presentation == 2)
            descriptions[5] = end_x_string;
        // Set transition strings in the array according to the x and y presentation IDs.
        // Transition 0 is the wording that comes right before the x_change description.
        if (descriptions[0] != "")
        {
            if (use_numerical_x)
                transitions[0] = "for";
            else if (!use_numerical_x)
                transitions[0] = "for a";
        }//end if
        // Transition 1 is the wording that comes right before the start_y description.
        if (descriptions[1] != "")
        {
            if (use_numerical_y)
            {
                if (segment_in.y_presentation == 0)
                    transitions[1] = "from";
                else if (segment_in.y_presentation == 1)
                    transitions[1] = "starting at";
            }//end if
            else if (!use_numerical_y)
            {
                if (segment_in.y_presentation == 0 || segment_in.y_presentation == 1)
                    transitions[1] = "starting";
            }//end else if
        }//end if
        // Transition 2 is the wording that comes right before the start_x description.
        if (descriptions[2] != "")
        {
            if (use_numerical_x)
            {
                // If there was a start_y description before, use 'at around'
                // If there was not, then use 'starting around'
                if (descriptions[1] != "")
                    transitions[2] = "at around";
                else
                    transitions[2] = "starting at around";
            }//end if
            else if (!use_numerical_x)
                transitions[2] = "";
        }//end if
        // Transition 3 is the wording that comes right before the y_change description.
        if (descriptions[3] != "")
        {
            // If there was a start_y or a start_x beforehand, use 'and changing by'
            // If there were not either, use 'changing by'
            if (descriptions[1] != "" || descriptions[2] != "")
                transitions[3] = "and changing by";
            else
                transitions[3] = "changing by";
        }//end if
        // Transition 4 is the wording that comes right before the end_y description
        if (descriptions[4] != "")
        {
            if (use_numerical_y)
            {
                // If there was a start_y beforehand, use 'to'
                // If there was a y_change beforehand, use 'and ending at'
                if (descriptions[1] != "")
                    transitions[4] = "to";
                else if (descriptions[3] != "")
                    transitions[4] = "and ending at";
            }//end if
            else
            {
                // If there was a start_y before hand, use 'and ending'
                // If there was not a start_y, use 'ending'
                if (descriptions[1] != "")
                    transitions[4] = "and ending";
                else
                    transitions[4] = "and ending";
            }//end else
        }//end if
        // Transition 5 is the wording that comes right before the end_x description.
        if (descriptions[5] != "")
        {
            // If there is a start_y and an end_y, use 'in'
            // If there is just an end_y or a y_change, use 'by'
            // If there are neither, use 'ending at'
            if (descriptions[1] != "" && descriptions[4] != "")
                //transitions[5] = "in";
                transitions[5] = "by";
            else if (descriptions[4] != "" || descriptions[3] != "")
                transitions[5] = "by";
            else
                transitions[5] = "ending at";
        }//end if

        // Assemble the description.
        for (int i = 0; i < descriptions.Length; i++)
        {
            if (descriptions[i] != "")
            {
                return_string += " ";
                if (transitions[i] != "")
                {
                    return_string += transitions[i];
                    return_string += " ";
                }//end if
                return_string += descriptions[i];
            }//end if
        }//end for

        return return_string;
    }//end method ActualizeXYDescription

    // Choose which presentation should be used for a segment's x, y, and slope values.
    public void DefinePresentation(List<Segment> segments_in, List<double> x_refs, List<double> y_refs)
    {
        // The constraints.
        // Preference for increasing the coverage towards the limit.
        //double coverage_weight = 10;
        // Preference for placing more space between observations.
        //double spacing_weight = 1;
        // Preference for placing observations on the same segment.
        //double stacking_weight = 1;

        // How many observations total should be in the presentation. This is used
        // to calculate coverage score.
        double coverage_limit = 8;

        // The total number of segments
        int number_of_segments = segments_in.Count;

        // How many observations should be presented in each segment.
        // Index of observation_counts_per_segment corresponds to index of
        // each segment in given list.
        List<int> observation_counts_per_segment = new List<int>();
        for (int i = 0; i < number_of_segments; i++)
        {
            // Check if the segment is a higher-level segment in the hierarchy.
            // If so, start it with at least 2 observation count; it needs to express its time values.
            if (segments_in[i].IsSupersegment())
                observation_counts_per_segment.Add(2);
            else
                observation_counts_per_segment.Add(0);
        }//end for

        // Hard limit to the total number of observations decided by the coverage limit.
        for (int i = 0; i < coverage_limit; i++)
        {
            // Decide on which segment the i-th observation will be placed.
            double highest_score = double.MinValue;
            int best_segment_id = 0;
            for (int j = 0; j < observation_counts_per_segment.Count; j++)
            {
                // Consider adding the i-th observation to segment j.
                // First, calculate the spacing score.
                // spacing_score = average distance to each other observation times spacing_weight.
                double spacing_score = 0;
                double sum_distance = 0;
                double total_observations = 0;
                double average_distance = 0;
                // Go through each segment and get the total distance to each other observation,
                // as well as counting the total number of other observations.
                for (int k = 0; k < observation_counts_per_segment.Count; k++)
                {
                    double current_distance = Math.Abs(k - j);
                    int observation_count_for_segment = observation_counts_per_segment[k];
                    sum_distance += current_distance * observation_count_for_segment;
                    total_observations += observation_count_for_segment;
                }//end for
                // On the first observation, there is no spacing score.
                if (total_observations == 0)
                    average_distance = 0;
                else
                    average_distance = sum_distance / total_observations;
                spacing_score = average_distance * spacing_weight;

                // Next, calculate the the stacking score.
                // stacking_score = number of observations on current segment times stacking_weight.
                double stacking_score = 0;
                double current_observation_count = observation_counts_per_segment[j];
                stacking_score = current_observation_count * stacking_weight;

                // Finally, calculate the coverage score.
                // coverage_score = (observation_limit - total number of observations) * coverage_score
                double coverage_score = 0;
                coverage_score = (coverage_limit - total_observations) * coverage_weight;

                double total_score = spacing_score + stacking_score + coverage_score;
                
                // See if it's higher than the highest score.
                if (total_score > highest_score)
                {
                    highest_score = total_score;
                    best_segment_id = j;
                }//end if
            }//end for

            // If the highest score does not increase the total score of the presentation (is < 0),
            // then there is no case where adding another observation will make the 
            // overall presentation better. Stop adding observations.
            if (highest_score <= 0)
                break;

            // We now have the id of the segment with the highest score.
            // Add an observation to the segment.
            observation_counts_per_segment[best_segment_id] += 1;
        }//end for

        // For each segment, look at the number of observations and decide on
        // the presentation style for x, y, and slope.
        // For x and y values, each style has a score of how many observations it costs.
        //  0: start value and end value (2)
        //  1: start value and change (2)
        //  2: change and end value (2)
        //  3: change (1)
        //  4: none (0)
        // For slope, each style has a score of how many observations it costs.
        //  0: slope direction (1)
        //  1: slope direction and magnitude (2)
        // A list of how many observations each x or y style costs.

        // Additionally, to increase output diversity, try to be dissimilar to what's already
        // been presented. 

        // Use counts for x, y, and slope styles.
        List<int> x_style_use_counts = new List<int>();
        List<int> y_style_use_counts = new List<int>();
        List<int> slope_style_use_counts = new List<int>();
        // Costs for x, y, and slope styles
        List<int> x_y_style_costs = new List<int>();
        List<int> slope_style_costs = new List<int>();

        // Initialize style use counts to 0
        for (int i = 0; i < 5; i++)
        {
            x_style_use_counts.Add(0);
            y_style_use_counts.Add(0);
        }//end for
        for (int i = 0; i < 2; i++)
        {
            slope_style_use_counts.Add(0);
        }//end for
        // Initialize style costs.
        x_y_style_costs.Add(2);
        x_y_style_costs.Add(2);
        x_y_style_costs.Add(2);
        x_y_style_costs.Add(1);
        x_y_style_costs.Add(0);
        slope_style_costs.Add(0);
        slope_style_costs.Add(1);

        for (int i = 0; i < segments_in.Count; i++)
        {
            Segment current_segment = segments_in[i];
            double desired_observation_count = observation_counts_per_segment[i];
            // Default slope style of 0, just the direction.
            double slope_style = 0;
            // Default x and y styles of 4, no presentation.
            double x_style = 4;
            double y_style = 4;
            // Find an x_style, y_style, and slope_style combination
            // whose cost most closely matches the desired observation counts for this segment
            // and which is most dissimilar to previous style uses.
            double current_slope_style = 0;
            double current_x_style = 4;
            double current_y_style = 4;

            double best_slope_style = 0;
            double best_x_style = 4;
            double best_y_style = 4;

            double current_total_cost = 0;
            double current_total_usage = 0;

            double least_total_usage = double.MaxValue;

            // Start with x styles.
            for (int j = 0; j < x_y_style_costs.Count; j++)
            {
                // If this is a supersegment, skip styles 3 and 4
                // Segments higher in the hierarchy should express where they are temporally.
                if ((current_segment.IsSupersegment()) && (j == 3 || j == 4))
                    continue;
                double x_cost = x_y_style_costs[j];
                double x_usage = x_style_use_counts[j];
                current_x_style = j;
                // Continue with y styles.
                for (int k = 0; k < x_y_style_costs.Count; k++)
                {
                    double y_cost = x_y_style_costs[k];
                    double y_usage = y_style_use_counts[k];
                    current_y_style = k;
                    // End with slope styles.
                    for (int l = 0; l < slope_style_costs.Count; l++)
                    {
                        double slope_cost = slope_style_costs[l];
                        double slope_usage = slope_style_use_counts[l];
                        current_slope_style = l;
                        double total_cost = x_cost + y_cost + slope_cost;
                        double total_usage = x_usage + y_usage + slope_usage;
                        // If total observation count cost is over the desired for this segment, skip.
                        if (total_cost > desired_observation_count)
                        {
                            continue;
                        }//end if
                        // Otherwise, check the total amount of previous usage against the least amount
                        // of previous usage.
                        if (total_usage < least_total_usage)
                        {
                            least_total_usage = total_usage;
                            best_x_style = current_x_style;
                            best_y_style = current_y_style;
                            best_slope_style = current_slope_style;
                        }//end if
                    }//end for
                }//end for
            }//end for
            // Use the best x, y, and slope styles found above for this segment.
            x_style = best_x_style;
            y_style = best_y_style;
            slope_style = best_slope_style;
            current_segment.x_presentation = (int)x_style;
            current_segment.y_presentation = (int)y_style;
            current_segment.slope_presentation = (int)slope_style;
            // Note the use of each of these styles.
            x_style_use_counts[(int)best_x_style] += 1;
            y_style_use_counts[(int)best_y_style] += 1;
            slope_style_use_counts[(int)best_slope_style] += 1;
        }//end for

        // Decide whether x and y are numerical or not.

        // Random selection.
        /*Random rand = new Random();
        foreach (Segment temp_segment in segments_in)
        {
            // Generate random number between 0 and 4.
            temp_segment.x_presentation = rand.Next(5);
            temp_segment.y_presentation = rand.Next(5);
            // Generate random number between 0 and 1.
            temp_segment.slope_presentation = rand.Next(2);
            temp_segment.use_numerical_x = rand.Next(2);
            temp_segment.use_numerical_y = rand.Next(2);
        }//end foreach*/
    }//end method DefinePresentation

    // Gives descriptors to the numeric values in a group of segments,
    // according to the given x and y axis reference numbers.
    public void DefineDescriptors(List<Segment> segments_in, List<double> x_refs, List<double> y_refs)
    {
        // First, calculate value (y-axis) high and low thresholds.
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
        double high_value_threshold = lowest_value + (value_range / 3) * 2;
        Console.WriteLine("High Value Threshold: " + high_value_threshold.ToString());
        double low_value_threshold = lowest_value + (value_range / 3);
        Console.WriteLine("Low Value Threshold: " + low_value_threshold.ToString());
        // Calculate date (x-axis) high and low thresholds
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
        double late_date_threshold = earliest_date + (date_range / 3) * 2;
        Console.WriteLine("Late Date Threshold: " + late_date_threshold.ToString());
        double early_date_threshold = earliest_date + (date_range / 3);
        Console.WriteLine("Early Date Threshold: " + early_date_threshold.ToString());
        // Calculate Magnitude large/small threshold.
        double y_change_threshold = value_range / 2;
        // Calculate date long/short threshold.
        double x_change_threshold = date_range / 2;
        Console.WriteLine("Magnitude Threshold: " + y_change_threshold.ToString());
        Console.WriteLine("Duration Threshold: " + x_change_threshold.ToString());
        // Calculate slope threshold and tolerances.
        double graph_slope = value_range / date_range;
        double tolerance = graph_slope * 0.05; // If slope magnitude is within this value from the graph slope, it is considered 1-to-1
        double steady_threshold = graph_slope * 0.1; // If the slope magnitude is below this, it is considered steady and not up or down.
        Console.WriteLine("Graph Slope: " + graph_slope.ToString());

        // Now that all thresholds are calculated, descriptors and references can be assigned.
        foreach (Segment temp_segment in segments_in)
        {
            // Find axis references for values and dates.
            // Map start and end values and dates to descriptors.
            string start_x_description = DateDescriptor(temp_segment.GetObservationValue(0)
                                                        , high_value_threshold
                                                        , low_value_threshold);
            temp_segment.AddObservationField(0, "description", start_x_description);

            string end_x_description = DateDescriptor(temp_segment.GetObservationValue(1)
                                                        , high_value_threshold
                                                        , low_value_threshold);
            temp_segment.AddObservationField(1, "description", end_x_description);

            string start_y_description = ValueDescriptor(temp_segment.GetObservationValue(2)
                                                        , high_value_threshold
                                                        , low_value_threshold);
            temp_segment.AddObservationField(2, "description", start_y_description);

            string end_y_description = ValueDescriptor(temp_segment.GetObservationValue(3)
                                                        , high_value_threshold
                                                        , low_value_threshold);
            temp_segment.AddObservationField(3, "description", end_y_description);

            // Map magnitude and duration of change to descriptors.
            string x_change_description = DurationDescriptor(temp_segment.GetObservationValue(4)
                                                        , x_change_threshold);
            temp_segment.AddObservationField(4, "description", x_change_description);

            string y_change_description = MagnitudeChangeDescriptor(temp_segment.GetObservationValue(5)
                                                                    , y_change_threshold);
            temp_segment.AddObservationField(5, "description", y_change_description);

            // Map direction of change and rate of change to descriptors.
            string slope_dir_description = DirectionDescriptor(temp_segment.GetObservationValue(6)
                                                                , temp_segment.GetObservationValue(7)
                                                                , steady_threshold);
            temp_segment.AddObservationField(7, "description", slope_dir_description);

            string slope_mag_description = RateDescriptor(temp_segment.GetObservationValue(6)
                                                            , graph_slope
                                                            , tolerance);
            Console.WriteLine("Segment " + temp_segment.id.ToString() + " slope: " + temp_segment.GetObservationValue(6).ToString());
            Console.WriteLine("Slope Description: " + slope_dir_description + " " + slope_mag_description);
            temp_segment.AddObservationField(6, "description", slope_mag_description);
        }//end foreach
    }//end method DefineDescriptors

    // Map a y-value to a descriptor, according to high and low thresholds.
    private string ValueDescriptor(double y_value, double high_threshold, double low_threshold)
    {
        if (y_value > high_threshold)
            return "high";
        else if (y_value < low_threshold)
            return "low";
        else
            return "near the middle";
    }//end method ValueDescriptor
    // Map an x-value to a descriptor, according to a late and early threshold.
    private string DateDescriptor(double date_value, double late_threshold, double early_threshold)
    {
        if (date_value > late_threshold)
            return "late";
        else if (date_value < early_threshold)
            return "early";
        else
            return "near the middle";
    }//end method DateDescriptor
    // Map a magnitude change to a descriptor, according to the given threshold.
    private string MagnitudeChangeDescriptor(double magnitude_change, double threshold)
    {
        if (magnitude_change < threshold)
            return "small";
        else
            return "large";
    }//end method MagnitudeChangeDescriptor
    // Map a date duration to a descriptor, according to the given threshold.
    private string DurationDescriptor(double duration, double threshold)
    {
        if (duration < threshold)
            return "short";
        else
            return "long";
    }//end method DurationDescriptor
    // Map the direction of the slope to a descriptor, according to the steady threshold
    private string DirectionDescriptor(double slope_mag, double slope_dir, double steady_threshold)
    {
        if (slope_mag < steady_threshold)
            return "stays steady";
        else if (slope_dir > 0)
            return "increases";
        else
            return "decreases";
    }//end method DirectionDescriptor
    // Map the magnitude of the slope to a descriptor, according to a given graph slope and a 1-to-1 envelope tolerance.
    private string RateDescriptor(double slope, double graph_slope, double envelope_tolerance)
    {
        double slope_magnitude = Math.Abs(slope);
        // NOTE: Graph slope is always positive.
        double diff = Math.Abs(graph_slope - slope_magnitude);
        // NOTE: Changed from substantially, dramatically, and steadily.
        if (diff < envelope_tolerance)
            return "consistently";
        else if (slope_magnitude > graph_slope)
            return "sharply";
        else
            return "slowly";
    }//end mehtod RateDescriptor

    // Finds the reference value in the given list closest to the real value given.
    private double FindNearestReference(double real_value, List<double> reference_list)
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

    // Flags which segments each segment should link to.
    public void CreateHierarchy(List<Segment> segments_in)
    {
        // Create the global segment.
        Segment global_segment = new Segment();
        // ID of -1 indicates the global segment.
        global_segment.id = -1;

        int highest_segment_id = segments_in[segments_in.Count - 1].id;
        int new_segment_id = highest_segment_id + 1;

        int grouping_count = 0;

        // If any new segments are made, track them here.
        List<Segment> new_segments = new List<Segment>();
        //int max_grouping_count = 0;
        // Look for the shape, and group segments accordingly.
        if (this.global_shape.Equals("w"))
        {
            // If this is a w, group segments that go down, then group them as they go up,
            // then group them as they go down again, and group them as they go up for the last time.
            // Even grouping counts mean we're looking for segments going down. Odd means we're looking
            // for segments going up.
            // Give the global segment its shape.
            global_segment.shape = this.global_shape;
            grouping_count = 0;
            bool end_current_subsegment = false;
            // The subsegment set that will be added to the global segment.
            List<Segment> global_subsegment_set = new List<Segment>();
            List<Segment> current_subsegment_set = new List<Segment>();
            bool last_segment = false;
            foreach (Segment temp_segment in segments_in)
            {
                // Group segments while they go down.
                if (grouping_count == 0 || grouping_count == 2)
                {
                    // Does this segment go down?
                    bool segment_falls = false;
                    if (temp_segment.start_point.y >= temp_segment.end_point.y)
                        segment_falls = true;
                    
                    // If not, then the segments are no longer falling. 
                    if (!segment_falls)
                    {
                        // End the subsegment we are building.
                        end_current_subsegment = true;
                    }//end if
                    // If so, then add it to the subsegment we are building.
                    else if (segment_falls)
                    {
                        current_subsegment_set.Add(temp_segment);
                    }//end else if
                }//end if
                else if (grouping_count == 1 || grouping_count == 3)
                {
                    // Does this segment go up?
                    bool segment_rises = false;
                    if (temp_segment.start_point.y <= temp_segment.end_point.y)
                        segment_rises = true;

                    // If not, then the segments are no longer rising.
                    if (!segment_rises)
                    {
                        // End the subsegment we are building.
                        end_current_subsegment = true;
                    }//end if
                    // If so, then add it to the subsegment we are building.
                    else if (segment_rises)
                    {
                        current_subsegment_set.Add(temp_segment);
                    }//end else if
                }//end else if

                // If we are ending the current subsegment, add a new segment to the global subsegment set.
                if (end_current_subsegment)
                {
                    // If the current subsegment is only one segment, add just that segment.
                    if (current_subsegment_set.Count == 1)
                    {
                        global_subsegment_set.Add(current_subsegment_set[0]);
                    }//end if
                    else
                    {
                        // Otherwise, make another segment above the current subsegment.
                        Segment global_subsegment = new Segment(new_segment_id, current_subsegment_set);
                        global_subsegment_set.Add(global_subsegment);
                        new_segments.Add(global_subsegment);
                        new_segment_id += 1;
                    }//end else
                    // Start a new subsegment set, with the current segment as the first member.
                    current_subsegment_set = new List<Segment>();
                    current_subsegment_set.Add(temp_segment);
                    grouping_count += 1;
                    end_current_subsegment = false;
                }//end if

            }//end foreach
            // Because new subsegments are made at the end of the above loop AFTER the previous one
            // is added to the set of global subsegments, the very last subsegment segment will
            // not be added when the loop ends. Do so here.
            // If the current subsegment is only one segment, add just that segment.
            if (current_subsegment_set.Count == 1)
            {
                global_subsegment_set.Add(current_subsegment_set[0]);
            }//end if
            else
            {
                // Otherwise, make another segment above the current subsegment.
                Segment global_subsegment = new Segment(new_segment_id, current_subsegment_set);
                global_subsegment_set.Add(global_subsegment);
                new_segments.Add(global_subsegment);
                new_segment_id += 1;
            }//end else

            // Add the global subsegment set to the global segment
            global_segment.SetSubsegments(global_subsegment_set);

        }//end if

        // DEBUG: Print the segment hierarchy, starting from the global segment.
        global_segment.PrintSubsegments(0);
        
        // Add the global and new segments to the set of all segments.
        this.all_segments.Add(global_segment);
        foreach (Segment new_segment in new_segments)
            this.all_segments.Add(new_segment);
    }//end method CreateHierarchy

    // Defines the numerical observations in a segment.
    public void DefineObservations(Segment segment_in)
    {
        segment_in.ResetObservations();
        // Raw values
        segment_in.AddObservation(0, "start_x", segment_in.start_point.x.ToString(), "");
        segment_in.AddObservation(1, "end_x", segment_in.end_point.x.ToString(), "");
        segment_in.AddObservation(2, "start_y", segment_in.start_point.y.ToString(), "");
        segment_in.AddObservation(3, "end_y", segment_in.end_point.y.ToString(), "");
        // Changes
        double change_x = segment_in.GetObservationValue(1) - segment_in.GetObservationValue(0);
        segment_in.AddObservation(4, "change_x", Math.Abs(change_x).ToString(), "");
        //double change_y = Math.Abs(segment_in.GetObservationValue(3) - segment_in.GetObservationValue(2));
        double change_y = segment_in.GetObservationValue(3) - segment_in.GetObservationValue(2);
        segment_in.AddObservation(5, "change_y", Math.Abs(change_y).ToString(), "");
        // Slope
        double slope = change_x / change_y;
        segment_in.AddObservation(6, "slope_mag", Math.Abs(slope).ToString(), "");
        segment_in.AddObservation(7, "slope_dir", (slope / Math.Abs(slope)).ToString(), "");
    }//end method DefineObservations

    // Reads an input set of segments.
    // All input files are assumed to be in the "data" folder.
    public List<Segment> ReadInputCSV(string csv_file_name)
    {
        List<Segment> return_list = new List<Segment>();
        string application_path = System.IO.Directory.GetCurrentDirectory();
        string csv_file_path = application_path + "/data/" + csv_file_name;
        // CSV headers are: id, start_x, start_y, end_x, end_y.
        // x is date since 1980/1/1, y is variable value.
        // Read all lines from the csv.
        string[] file_lines = File.ReadAllLines(csv_file_path);

        bool first_line = true;
        foreach (string file_line in file_lines)
        {
            // Skip the first line, which just lists the headers.
            if (first_line)
            {
                first_line = false;
                continue;
            }//end if
            // Split the line by comma.
            string[] separated_line = file_line.Split(',');
            // ========== CONVERT CSV STRINGS TO NUMBERS ==========
            bool parse_success = false;
            int id = 0;
            parse_success = int.TryParse(separated_line[0], out id);
            if (!parse_success)
            {
                Console.WriteLine("Failed parsing id " + separated_line[0]);
                continue;
            }//end if

            double start_x = 0;
            parse_success = double.TryParse(separated_line[1], out start_x);
            if (!parse_success)
            {
                Console.WriteLine("Failed parsing start_x " + separated_line[1]);
                continue;
            }//end if

            double start_y = 0;
            parse_success = double.TryParse(separated_line[2], out start_y);
            if (!parse_success)
            {
                Console.WriteLine("Failed parsing start_y " + separated_line[2]);
                continue;
            }//end if

            double end_x = 0;
            parse_success = double.TryParse(separated_line[3], out end_x);
            if (!parse_success)
            {
                Console.WriteLine("Failed parsing end_x " + separated_line[3]);
                continue;
            }//end if

            double end_y = 0;
            parse_success = double.TryParse(separated_line[4], out end_y);
            if (!parse_success)
            {
                Console.WriteLine("Failed parsing end_y " + separated_line[4]);
                continue;
            }//end if
            // ========== END CONVERT CSV STRINGS TO NUMBERS ==========

            // Create a segment for this row.
            double start_year = this.starting_year;
            // In matlab, there are 333 time ticks in a year.
            double input_date_reference = 333;
            start_x = (start_x + start_year * input_date_reference) / input_date_reference;
            end_x = (end_x + start_year * input_date_reference) / input_date_reference;
            Segment new_segment = new Segment(id, start_x, start_y, end_x, end_y);
            // Add it to the list of segments.
            return_list.Add(new_segment);
        }//end foreach

        return return_list;
    }//end method ReadInputCSV

    // Read info file that gives reference x and y points in the graph,
    // as well as other pieces of information like the site name, the
    // variable name, and the x and y labels.
    private JObject ReadInputInfo(string info_file_name)
    {
        string application_path = System.IO.Directory.GetCurrentDirectory();
        string info_file_path = application_path + "/data/" + info_file_name;
        StreamReader info_file = File.OpenText(info_file_path);
        
        JsonTextReader json_reader = new JsonTextReader(info_file);
        JObject info_object = (JObject)JToken.ReadFrom(json_reader);

        return info_object;
    }
}// end class NarrativeGenerator