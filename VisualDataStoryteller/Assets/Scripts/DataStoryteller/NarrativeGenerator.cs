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

    public void GenerateNarrative(string data_file_name, string segment_file_name, string info_file_name, double starting_year, int description_type = 0)
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

        // Calculate and define numerical observations for each segment.
        foreach (Segment temp_segment in all_segments)
        {
            DefineObservations(temp_segment);
        }//end foreach

        // Fill out value references and give words to numerical observations.
        DefineDescriptors(all_segments, x_refs, y_refs);

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
            if (all_shapes.Count == 1)
                description += "For the whole graph, ";
            else if (i == 0)
                description += "First, ";
            else if (i == all_shapes.Count - 1 && all_shapes.Count > 2)
                description += "Finally, ";
            else
                description += "Then, ";
            current_shape = all_shapes[i];

            // Set different description types for different test cases.
            current_shape.description_type = description_type;
            current_shape.verbose = false;

            description += current_shape.GenerateDescription(x_refs, y_refs, x_label, y_label, point_of_interest, site_name, variable_name);
        }//end foreach
        //string description = overall_shape.GenerateDescription(x_refs, y_refs, x_label, y_label, point_of_interest, site_name, variable_name);
        Console.WriteLine(description);
    }//end method GenerateNarrative

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
        //Console.WriteLine("High Value Threshold: " + high_value_threshold.ToString());
        double low_value_threshold = lowest_value + (value_range / 3);
        //Console.WriteLine("Low Value Threshold: " + low_value_threshold.ToString());
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
        //Console.WriteLine("Late Date Threshold: " + late_date_threshold.ToString());
        double early_date_threshold = earliest_date + (date_range / 3);
        //Console.WriteLine("Early Date Threshold: " + early_date_threshold.ToString());
        // Calculate Magnitude large/small threshold.
        double y_change_threshold = value_range / 2;
        // Calculate date long/short threshold.
        double x_change_threshold = date_range / 2;
        //Console.WriteLine("Magnitude Threshold: " + y_change_threshold.ToString());
        //Console.WriteLine("Duration Threshold: " + x_change_threshold.ToString());
        // Calculate slope threshold and tolerances.
        double graph_slope = value_range / date_range;
        double tolerance = graph_slope * 0.05; // If slope magnitude is within this value from the graph slope, it is considered 1-to-1
        double steady_threshold = graph_slope * 0.1; // If the slope magnitude is below this, it is considered steady and not up or down.
        //Console.WriteLine("Graph Slope: " + graph_slope.ToString());

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
            //Console.WriteLine("Segment " + temp_segment.id.ToString() + " slope: " + temp_segment.GetObservationValue(6).ToString());
            //Console.WriteLine("Slope Description: " + slope_dir_description + " " + slope_mag_description);
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
        
        application_path = "C:/Users/zevsm/Documents/GitHub/DataStoryteller/VisualDataStoryteller/Assets/Scripts/DataStoryteller";

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

        string info_file_path = application_path + "/data/" + info_file_name;
        StreamReader info_file = File.OpenText(info_file_path);
        
        JsonTextReader json_reader = new JsonTextReader(info_file);
        JObject info_object = (JObject)JToken.ReadFrom(json_reader);

        return info_object;
    }
}// end class NarrativeGenerator