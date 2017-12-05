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
    private string y_label;
    private double starting_year;

    public NarrativeGenerator()
    {
        this.GenerateNarrative();
    }//end constructor NarrativeGenerator

    public void GenerateNarrative()
    {
        // First, read the input data.
        // Set 1
        //string data_file_name = "from_output_1.csv";
        //string info_file_name = "bb_cl.json";
        //this.starting_year = 1995;
        // Set 2
        string data_file_name = "from_output_2.csv";
        string info_file_name = "an_mg.json";
        double starting_year = 2012;
        List<Segment> all_segments = this.ReadInputCSV(data_file_name);
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
        y_label = info["info"].Value<string>("x_label");
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

        // We now have a list of segments.
        // Calculate and define numerical observations for each segment.
        foreach (Segment temp_segment in all_segments)
        {
            DefineObservations(temp_segment);
        }//end foreach

        // Define segment linkages.
        //DefineLinkages(all_segments);

        // Fill out value references and give words to numerical observations.
        DefineDescriptors(all_segments, x_refs, y_refs);

        string global_trend_descriptor = "";
        double global_change = all_segments[all_segments.Count - 1].end_point.y - all_segments[0].start_point.y;
        if (global_change < 0)
            global_trend_descriptor = "generally decreases";
        else
            global_trend_descriptor = "generally increases";

        // Define occurence connections.

        // The constraints
        // How many observations should be given.
        double length_constraint = 5;
        // How many y-axis values should be given.
        double y_value_count = 2;
        // How many x-axis values should be given.
        double x_value_count = 2;

        // Decide how each part of each segment is going to be presented.
        DefinePresentation(all_segments, x_refs, y_refs);

        // Assemble the description
        string description = "";

        // Global descriptors
        description += variable_name + " at " + site_name; 
        description += " from " + x_refs[0].ToString() + " to " + x_refs[x_refs.Count - 1].ToString();
        description += " " + global_trend_descriptor + ".";
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
        foreach (Segment temp_segment in all_segments)
        {
            if (segment_counter == 0)
                description += " At first, it";
            else if (segment_counter == all_segments.Count() - 1)
                description += " Finally, it";
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
            description += " " + ActualizeDescription(temp_segment, x_refs, y_refs) + ".";

            segment_counter += 1;
        }//end foreach

        Console.WriteLine("Description: " + description);
    }//end method GenerateNarrative

    // Give words to a segment's presentation.
    public string ActualizeDescription(Segment segment_in, List<double> x_refs, List<double> y_refs)
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

        Console.WriteLine("Segment " + segment_in.id.ToString() + " Use numerical x: " + use_numerical_x.ToString() + " y: " + use_numerical_y.ToString());

        // Slope
        presentation_string += ActualizeSlopeDescription(segment_in);
        // Then y and x values
        presentation_string += ActualizeXYDescription(segment_in, use_numerical_x, use_numerical_y, x_refs, y_refs);
        return presentation_string;
    }//end method ActualizeDescription
    private string ActualizeSlopeDescription(Segment segment_in)
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
            y_change_string = Math.Round(segment_in.GetObservationValue(5)).ToString() + " " + this.y_label;
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
                x_change_string = Math.Round(segment_in.GetObservationValue(4)).ToString() + " years,";
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
        Random rand = new Random();
        foreach (Segment temp_segment in segments_in)
        {
            // Generate random number between 0 and 4.
            temp_segment.x_presentation = rand.Next(5);
            temp_segment.y_presentation = rand.Next(5);
            // Generate random number between 0 and 1.
            temp_segment.slope_presentation = rand.Next(2);
            temp_segment.use_numerical_x = rand.Next(2);
            temp_segment.use_numerical_y = rand.Next(2);
        }//end foreach
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
    public void DefineLinkages(List<Segment> segments_in)
    {
        foreach (Segment temp_segment in segments_in)
        {
            // Check for near-slope in adjacent segments.
            // Specifically, check for the segment directly after this one.
            int current_id = temp_segment.id;
            double current_slope = temp_segment.slope;
            int next_id = current_id + 1;
            foreach (Segment temp_segment_2 in segments_in)
            {
                if (temp_segment_2.id == next_id)
                {
                    
                }//end if
            }//end foreach
        }//end foreach
    }//end method DefineLinkages

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
            start_x = (start_x + start_year * 365) / 365;
            end_x = (end_x + start_year * 365) / 365;
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