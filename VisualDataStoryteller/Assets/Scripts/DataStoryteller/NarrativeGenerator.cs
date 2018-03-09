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

    public List<double> x_refs;
    public List<double> y_refs;
    private string y_label;
    private string x_label;
    private double starting_year;
    private string global_shape;
    private List<string> global_shape_names;
    private double ticks_per_year;

    public List<DataPoint> all_data_points;

    private List<Segment> all_segments;

    private List<Segment> all_segments_chronological;

    private List<Segment> all_segments_hierarchical;

    // Which piece of information we will try to make more salient.
    private DataPoint point_of_interest;

    public NarrativeGenerator()
    {
        //this.GenerateNarrative();
    }//end constructor NarrativeGenerator

    public Narrative GenerateNarrative(string data_file_name, string segment_file_name, string info_file_name, double starting_year, int description_type = 0)
    {
        // First, read the input data.
        this.starting_year = starting_year;
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
        this.y_label = info["info"].Value<string>("y_label");
        this.x_label = info["info"].Value<string>("x_label");
        // How many of the input x value ticks there are in a single year.
        string string_ticks_per_year = info["info"].Value<string>("ticks_per_year");
        this.ticks_per_year = 0;
        double.TryParse(string_ticks_per_year, out this.ticks_per_year);
        // Get the list of reference values for both axis.
        this.x_refs = new List<double>();
        foreach (JToken x_ref_entry in info["info"]["x_refs"])
        {
            //double converted_x_ref = x_ref_entry.ToObject<double>();
            //converted_x_ref = converted_x_ref * 365 - (1981 * 365);
            //x_refs.Add(converted_x_ref);
            x_refs.Add(x_ref_entry.ToObject<double>());
        }//end foreach
        this.y_refs = new List<double>();
        foreach (JToken y_ref_entry in info["info"]["y_refs"])
        {
            y_refs.Add(y_ref_entry.ToObject<double>());
        }//end foreach
        
        // Get the list of global shapes.
        //global_shape = info["info"].Value<string>("global_shape");
        global_shape_names = new List<string>();
        foreach (JToken shape_entry in info["info"]["global_shape"])
        {
            global_shape_names.Add(shape_entry.ToObject<string>());
        }//end foreach

        // Get the lists of critical points
        // A list of the list of critical points for each shape. The index of each shape's critical point
        // list should be the index of each shape in the global shapes list.
        List<List<double>> all_critical_points = new List<List<double>>();
        List<double> current_critical_points = new List<double>();
        double converted_critical_point = -1;
        foreach (JToken critical_point_list in info["info"]["critical_points"])
        {
            foreach (JToken critical_point_entry in critical_point_list)
            {
                converted_critical_point = ConvertXValue(critical_point_entry.ToObject<double>());
                current_critical_points.Add(converted_critical_point);
            }//end foreach
            all_critical_points.Add(current_critical_points);
            current_critical_points = new List<double>();
        }//end foreach
        
        // Read input segments
        all_segments = this.ReadInputCSV(segment_file_name);

        all_data_points = this.ReadInputData(data_file_name);

        //Console.WriteLine("Done reading input files.");

        // We now have a list of segments.
        double max_y = double.MinValue;
        DataPoint max_point = new DataPoint();
        // Make the Point of Interest the one with the highest y value amongst all segments points.
        foreach (Segment temp_segment in all_segments)
        {
            if (temp_segment.start_point.y > max_y)
            {
                max_y = temp_segment.start_point.y;
                max_point = temp_segment.start_point;
            }//end if
            else if (temp_segment.end_point.y > max_y)
            {
                max_y = temp_segment.end_point.y;
                max_point = temp_segment.end_point;
            }//end else if
        }//end foreach
        point_of_interest = max_point;
        // POI as first point in graph.
        point_of_interest = all_segments[0].start_point;

        List<Shape> all_shapes = new List<Shape>();
        Shape current_shape = null;
        // The index of each shape will be the index of its name in the list of global shape names.
        // Divide the segments up into each shape, according to the critical points.
        List<List<Segment>> shape_segment_sets = new List<List<Segment>>();
        string current_shape_name = "";
        current_critical_points = new List<double>();
        double first_critical_point = -1;
        double last_critical_point = -1;
        List<Segment> current_shape_segment_set = new List<Segment>();
        // For now, limit it to only one shape of interest.
        bool shape_of_interest_found = false;
        for (int i = 0; i < global_shape_names.Count; i++)
        {
            current_shape_name = global_shape_names[i];
            //Console.WriteLine("Making shape: " + current_shape_name);
            current_critical_points = all_critical_points[i];
            first_critical_point = current_critical_points[0];
            last_critical_point = current_critical_points[current_critical_points.Count - 1];

            current_shape_segment_set = new List<Segment>();
            // Group all segments that lie between the first and last critical points.
            foreach (Segment current_segment in all_segments)
            {
                if (current_segment.start_point.x >= first_critical_point
                && current_segment.end_point.x <= last_critical_point)
                {
                    current_shape_segment_set.Add(current_segment);
                }//end if
            }//end foreach
            shape_segment_sets.Add(current_shape_segment_set);

            // Make a shape.
            bool shape_made = false;
            if (current_shape_name.Equals("w"))
            {
                current_shape = new ShapeW();
                shape_made = true;
            }//end if
            else if (current_shape_name.Equals("v"))
            {
                current_shape = new ShapeV();
                shape_made = true;
            }//end if
            else if (current_shape_name.Equals("line"))
            {
                current_shape = new ShapeLine();
                shape_made = true;
            }//end else if

            if (shape_made)
            {
                current_shape.MatchSegmentsToShape(shape_segment_sets[i], all_critical_points[i]);

                // See if the point of interest also lies within this set.
                // If so, mark this shape as the shape of interest; all other shapes are ancillary.
                if (point_of_interest.x >= first_critical_point && point_of_interest.x <= last_critical_point && !shape_of_interest_found)
                {
                    current_shape.shape_of_interest = true;
                    shape_of_interest_found = true;
                }//end if

                all_shapes.Add(current_shape);
            }//end if
        }//end for
        
        Narrative main_narrative = new Narrative();

        // Generate a description.
        string description = "";
        for (int i = 0; i < all_shapes.Count; i++)
        {
            current_shape = all_shapes[i];
            // Only continue if this is the shape of interest.
            if (!current_shape.shape_of_interest)
            {
                continue;
            }//end if

            // Make critical point objects out of each of the shape's critical points.
            List<CriticalPoint> critical_points = new List<CriticalPoint>();
            critical_points = FindCriticalPoints(current_shape);

            double y_per_x = ((ShapeW)current_shape).YPerX(y_refs, x_refs);

            // Find normal values for each of the shape's critical points.
            List<DataPoint> normal_values = new List<DataPoint>();
            normal_values = current_shape.FindCriticalPointNormalValues(x_refs, y_refs);

            // Assign normal values to each critical point, in order.
            for (int j = 0; j < critical_points.Count; j++)
            {
                critical_points[j].normal_point = normal_values[j];
            }//end for

            // Make abnormalities for each critical point that is abnormal.
            List<Abnormality> abnormalities = new List<Abnormality>();
            abnormalities = FindAbnormalities(critical_points, y_per_x);

            // Make narrative events for each critical point and each abnormality.
            List<NarrativeEvent> narrative_events = new List<NarrativeEvent>();
            narrative_events = MakeNarrativeEvents(critical_points, abnormalities);

            // Order narrative events.
            List<NarrativeEvent> ordered_narrative_events = new List<NarrativeEvent>();
            ordered_narrative_events = OrderNarrativeEvents(narrative_events);

        }//end foreach
 
        return main_narrative;
    }//end method GenerateNarrative

    private List<NarrativeEvent> OrderNarrativeEvents(List<NarrativeEvent> unordered_narrative_events)
    {
        List<NarrativeEvent> ordered_narrative_events = new List<NarrativeEvent>();
        List<NarrativeEvent> unassigned_narrative_events = new List<NarrativeEvent>();
        foreach (NarrativeEvent temp_event in unordered_narrative_events)
            unassigned_narrative_events.Add(temp_event);

        // How quickly we assume tension degrades.
        double tension_degrade = -0.5;

        // Define the ideal tension curve.
        // For a 5-turn story.
        int narrative_length = 5;
        // It rises for 2 turns, peaks at the middle turn, then falls for 2 turns.
        List<double> discrete_tension_values = new List<double>();
        // Each value represents what we want the tension value to be at at the END of the turn the value is on (after the event ON the turn has passed).
        discrete_tension_values = new List<double>{0, 0.5, 1.0, 0.5, 0};

        // Look for the max tension change achievable amongst the narrative events.
        double max_tension_change = double.MinValue;
        foreach (NarrativeEvent temp_event in unordered_narrative_events)
        {
            if (temp_event.tension_change > max_tension_change)
                max_tension_change = temp_event.tension_change;
        }//end foreach

        // The largest tension change is considered to raise tension to "1". All other tension changes are normalized to this.
        double tension_normalizer = 1 / max_tension_change;
        
        double current_tension = 0;
        double desired_tension_difference = 0;
        double next_best_tension_change = 0;
        NarrativeEvent next_best_event = new NarrativeEvent();
        for (int i = 0; i < narrative_length; i++)
        {
            next_best_tension_change = 0;
            next_best_event = new NarrativeEvent();
            desired_tension_difference = discrete_tension_values[i] - current_tension;
            // If we have less tension than we want, pick the narrative event which will increase the tension closest to the amount we wish for.
            if (desired_tension_difference > 0)
            {
                foreach (NarrativeEvent temp_event in unassigned_narrative_events)
                {
                    if (Math.Abs(desired_tension_difference - next_best_tension_change) > Math.Abs(desired_tension_difference - temp_event.tension_change * tension_normalizer))
                    {
                        next_best_tension_change = temp_event.tension_change * tension_normalizer;
                        next_best_event = temp_event;
                    }//end if
                }//end foreach
            }//end if
            else
                next_best_event = unassigned_narrative_events[0];
            
            // Add the event to the ordered list of events and remove it from the unordered list.
            ordered_narrative_events.Add(next_best_event);
            unassigned_narrative_events.Remove(next_best_event);

            // If the tension is not being changed by an event, degrade it.
            if (next_best_tension_change == 0)
                next_best_tension_change = tension_degrade;

            // Increase tension according to event chosen.
            current_tension += next_best_tension_change;
        }//end for

        return ordered_narrative_events;
    }//end method OrderNarrativeEvents

    private List<NarrativeEvent> MakeNarrativeEvents(List<CriticalPoint> critical_points, List<Abnormality> abnormalities)
    {
        List<NarrativeEvent> return_list = new List<NarrativeEvent>();

        NarrativeEvent new_narrative_event = new NarrativeEvent();

        // Make a narrative event for each critical point.
        foreach (CriticalPoint critical_point in critical_points)
        {
            new_narrative_event = new NarrativeEvent(critical_point, 0);
            return_list.Add(new_narrative_event);
        }//end foreach
        
        // Make a narrative event for each abnormality.
        foreach (Abnormality abnormality in abnormalities)
        {
            new_narrative_event = new NarrativeEvent(abnormality, 1);
            return_list.Add(new_narrative_event);
        }//end foreach

        return return_list;
    }//end method MakeNarrativeEvents

    private List<CriticalPoint> FindCriticalPoints(Shape shape_in)
    {
        List<CriticalPoint> return_list = new List<CriticalPoint>();
        
        CriticalPoint new_critical_point = new CriticalPoint();

        foreach (DataPoint critical_data_point in shape_in.critical_points)
        {
            new_critical_point = new CriticalPoint(critical_data_point);
            new_critical_point.name = critical_data_point.name;
            return_list.Add(new_critical_point);
        }//end foreach

        return return_list;
    }//end method FindCriticalPoints

    private List<Abnormality> FindAbnormalities(List<CriticalPoint> critical_points, double y_per_x)
    {
        List<Abnormality> return_list = new List<Abnormality>();

        // Threshold distance of critical point from normal point to consider it an abnormality.
        double distance_threshold = 1.0f;

        double current_distance = 0.0f;

        Abnormality new_abnormality = new Abnormality();
        foreach (CriticalPoint critical_point in critical_points)
        {
            new_abnormality = new Abnormality();
            new_abnormality.critical_point = critical_point;

            // Measure the distance between the critical point and its normal value.
            current_distance = DistanceBetweenPoints(critical_point.data_point, critical_point.normal_point, y_per_x);
            // Set it as the abnormality's degree
            new_abnormality.degree = current_distance;
            return_list.Add(new_abnormality);
        }//end foreach

        return return_list;
    }//end method FindAbnormalities

    // Returns the distance from point 1 to point 2.
    private double DistanceBetweenPoints(DataPoint point_1, DataPoint point_2, double y_per_x)
    {
        // Measure the distance as though the two axis are scaled to each other.
        // Convert x values to their y equivalents using y_per_x parameter.

        double x_diff = point_2.x * y_per_x - point_1.x * y_per_x;
        double y_diff = point_2.y - point_1.y;
        double sum_squares = x_diff * x_diff + y_diff * y_diff;
        double distance = Math.Sqrt(sum_squares);

        return distance;
    }//end method DistanceBetweenPoints

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

    // Reads an input set of segments.
    // All input files are assumed to be in the "data" folder.
    public List<Segment> ReadInputCSV(string csv_file_name)
    {
        List<Segment> return_list = new List<Segment>();
        string application_path = System.IO.Directory.GetCurrentDirectory();
        
        application_path = "C:/Users/zevsm/Documents/GitHub/DataStoryteller/VisualDataStoryteller/Assets/Scripts/DataStoryteller";
        //application_path = "C:/Users/Zev/Documents/GitHub/DataStoryteller/VisualDataStoryteller/Assets/Scripts/DataStoryteller";

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
            start_x = ConvertXValue(start_x);
            end_x = ConvertXValue(end_x);
            Segment new_segment = new Segment(id, start_x, start_y, end_x, end_y);
            // Add it to the list of segments.
            return_list.Add(new_segment);
        }//end foreach

        return return_list;
    }//end method ReadInputCSV

    // Read an input file containing all datapoints for the graph used to generate the current narrative.
    private List<DataPoint> ReadInputData(string data_file_name)
    {
        List<DataPoint> data_points = new List<DataPoint>();

        string application_path = "C:/Users/zevsm/Documents/GitHub/DataStoryteller/VisualDataStoryteller/Assets/Scripts/DataStoryteller";
        //application_path = "C:/Users/Zev/Documents/GitHub/DataStoryteller/VisualDataStoryteller/Assets/Scripts/DataStoryteller";
        
        string data_file_path = application_path + "/data/" + data_file_name;
        // CSV headers are: Date, Value
        // Read all lines from the csv.
        string[] file_lines = File.ReadAllLines(data_file_path);

        bool first_line = true;

        int reference_year = 1900;
        int reference_month = 1;
        int reference_day = 1;
        Dictionary<String, int> days_per_month = new Dictionary<string, int>();
        days_per_month.Add("Jan", 31);
        days_per_month.Add("Feb", 28);
        days_per_month.Add("Mar", 31);
        days_per_month.Add("Apr", 30);
        days_per_month.Add("May", 31);
        days_per_month.Add("Jun", 30);
        days_per_month.Add("Jul", 31);
        days_per_month.Add("Aug", 31);
        days_per_month.Add("Sep", 30);
        days_per_month.Add("Oct", 31);
        days_per_month.Add("Nov", 30);
        days_per_month.Add("Dec", 31);

        List<string> month_order = new List<String>{"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

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

            // Convert date to number of days since 1/1/1900
            string date_string = separated_line[0];
            // Split the date string by dashes
            string[] separated_date = date_string.Split('-');
            // Date is in day-month-year format, with month as the month's 3-letter shortened name.
            int total_days = 0;
            // Day
            total_days += int.Parse(separated_date[0]);
            // Month
            List<string> months_before = month_order.GetRange(0, month_order.IndexOf(separated_date[1]));
            foreach (string month in months_before)
                total_days += days_per_month[month];
            // Year
            total_days += (int.Parse(separated_date[2]) - reference_year) * 365;

            double value = double.Parse(separated_line[1]);

            data_points.Add(new DataPoint(total_days, value));
        }//end foreach        

        return data_points;
    }//end method ReadInputData

    // Converts an x value from matlab into years.
    // Convert years into number of days since 1/1/1900.
    private double ConvertXValue(double x_val_to_convert)
    {
        double return_value = (x_val_to_convert + this.starting_year * this.ticks_per_year) / this.ticks_per_year;
        return_value = return_value - 1900;
        return_value = return_value * 365;
        return return_value;
    }//end method ConvertXValue

    // Read info file that gives reference x and y points in the graph,
    // as well as other pieces of information like the site name, the
    // variable name, and the x and y labels.
    private JObject ReadInputInfo(string info_file_name)
    {
        string application_path = System.IO.Directory.GetCurrentDirectory();

        application_path = "C:/Users/zevsm/Documents/GitHub/DataStoryteller/VisualDataStoryteller/Assets/Scripts/DataStoryteller";
        //application_path = "C:/Users/Zev/Documents/GitHub/DataStoryteller/VisualDataStoryteller/Assets/Scripts/DataStoryteller";

        string info_file_path = application_path + "/data/" + info_file_name;
        StreamReader info_file = File.OpenText(info_file_path);
        
        JsonTextReader json_reader = new JsonTextReader(info_file);
        JObject info_object = (JObject)JToken.ReadFrom(json_reader);

        return info_object;
    }
}// end class NarrativeGenerator