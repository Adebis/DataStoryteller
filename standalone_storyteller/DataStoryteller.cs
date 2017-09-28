using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Reflection;

public class DataStoryteller {

    // For parsing the data from the files.
    public List<string> character_names;

    // Key1 is the name of the character. Value1 is the DataSet for that character.
    // Key2 is the header for a column of data. Value 2 is the list of datapoints for that header.
    // Each DataPoint of the DataSet consists of the date of the measurement,
    // the secchi depth of the measurement, and the name of the character the measurement
    // belongs to.
    public Dictionary<string, Dictionary<string, List<DataPoint>>> data_by_character;

    List<Dictionary<string, string>> all_data;
    List<string> all_headers;

    public string unity_log;

    public DataStoryteller()
    {
        try
        {
            unity_log = "";
            // Characters are going to be 
            character_names = new List<string>();

            data_by_character = new Dictionary<string, Dictionary<string, List<DataPoint>>>();

            string application_path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // For Lake George data, we want to replace the site codes with their proper site names.
            Dictionary<string, string> site_code_map = new Dictionary<string, string>();
            site_code_map.Add("F", "French Point");
            site_code_map.Add("SD", "Sabbath Day Point");
            site_code_map.Add("S", "Smith Bay");
            site_code_map.Add("R", "Rogers Rock");
            site_code_map.Add("A10", "Northwest Bay");
            site_code_map.Add("BB", "Basin Bay");
            site_code_map.Add("D", "Dome Island");
            site_code_map.Add("T", "Tea Island");
            site_code_map.Add("N", "Green Island");
            site_code_map.Add("AN", "Anthonys Nose");
            site_code_map.Add("CP", "Calves Pen");

            // Headers:
            // SITE,Z,Date,Zsec,PH,COND,ALK,OP,TFP,TP,CL,NO3,SO4,TN,NH4,SI,Na,Mg,Ca,K,Fe,CHLA,T,DO (mg/l),DO (sat),Zcomp (m)
            List<string> csv_headers = new List<string>();
            string data_file_name = "offshore_chemistry_data_1980_2016.csv";
            // Data from the CSV. Key is the name of the header, value is the data value. Each item in the list is a row
            // of the CSV.
            List<Dictionary<string, string>> csv_data = new List<Dictionary<string, string>>();
            string data_path = application_path + "/data/" + data_file_name;
            string[] file_lines = File.ReadAllLines(data_path);
            bool first_line = true;

            foreach (string file_line in file_lines)
            {
                // Separate the line by commas.
                string[] separated_line = file_line.Split(',');
                if (separated_line.Length != 26)
                    first_line = false;

                // If this is the first line, populate the header list.
                if (first_line)
                {
                    first_line = false;
                    foreach (string header in separated_line)
                    {
                        csv_headers.Add(header);
                    }//end foreach
                    continue;
                }//end if
                 // Otherwise, start populating a new CSV row dictionary.
                Dictionary<string, string> csv_row_data = new Dictionary<string, string>();
                for (int i = 0; i < separated_line.Length; i++)
                {
                    string header_name = csv_headers[i];
                    string item_to_add = separated_line[i];
                    // For the site code (first index), make sure to replace
                    // the code with the site name.
                    if (i == 0)
                    {
                        // First, turn the site code to upper-case.
                        item_to_add = item_to_add.ToUpper();
                        if (site_code_map.ContainsKey(item_to_add))
                            // Next, replace it with the corresponding site name (if we know its site name)
                            item_to_add = site_code_map[item_to_add];
                    }//end if
                    csv_row_data.Add(header_name, item_to_add);
                }//end for
                 // Add the row to the list of rows
                csv_data.Add(csv_row_data);
            }//end foreach

            all_data = csv_data;
            all_headers = csv_headers;

            // Make a DataPoint out of each column for each data row.
            foreach (Dictionary<string, string> data_row in all_data)
            {
                foreach (string h in all_headers)
                {
                    // Create a new datapoint with Chlorophyll A as the main value.
                    DataPoint new_datapoint = new DataPoint(data_row, h);
                    // If there is a valid Chlorophyll A value, add the datapoint to a character's list of datapoints.
                    if (new_datapoint.population_success)
                    {
                        // Check which character it is under.
                        string current_character = new_datapoint.character;
                        // Add the datapoint to the appropriate list of datapoints per character.
                        if (data_by_character.ContainsKey(current_character))
                        {
                            if (data_by_character[current_character].ContainsKey(h))
                                data_by_character[current_character][h].Add(new_datapoint);
                            else
                            {
                                data_by_character[current_character].Add(h, new List<DataPoint>());
                                data_by_character[current_character][h].Add(new_datapoint);
                            }//end else
                        }//end if
                        else
                        {
                            data_by_character.Add(current_character, new Dictionary<string, List<DataPoint>>());
                            data_by_character[current_character].Add(h, new List<DataPoint>());
                            data_by_character[current_character][h].Add(new_datapoint);
                        }//end else
                    }//end if
                }//end foreach
            }//end foreach

            // We now have a list of datapoints for each character's chlorophyll A level.
            // Run segmenter on a single character's datapoints at a time.
            int number_of_segments = 4;
            //string character_name = "Northwest Bay";
            // Characters:
            //  Northwest Bay
            //  French Point
            // Segment Chlorophyll A, CHLA
            Dictionary<String, List<Segment>> segmentations_by_character = new Dictionary<String, List<Segment>>();
            character_names = site_code_map.Values.ToList();
            string header_to_segment = "CHLA";
            List<string> headers_to_segment = new List<string>();
            headers_to_segment.Add("CHLA");
            headers_to_segment.Add("Zsec");
            string character_to_segment = "French Point";
            List<List<Segment>> segmentations = new List<List<Segment>>();
            List<List<string>> segmentation_tags = new List<List<string>>();

            foreach (string h in headers_to_segment)
            {
                List<Segment> current_segmentation = new List<Segment>();
                current_segmentation = SegmentData(character_to_segment, header_to_segment, number_of_segments);
                segmentations.Add(current_segmentation);
                segmentation_tags.Add(new List<string>());
            }//end foreach
            // Go through each segmentation and add tags for it.
            for (int i = 0; i < segmentations.Count; i++)
            {
                List<Segment> current_segmentation = segmentations[i];
                foreach (Segment s in current_segmentation)
                    s.tags = DetermineSegmentTags(s);
                // Get tags for the whole segmentation once we've gotten tags for each individual segment.
                segmentation_tags[i] = DetermineSegmentationTags(current_segmentation);
            }//end for

            // Use the tags to describe each segmentation.
            String description_text = "";
            List<string> previous_segmentation_tags = new List<string>();
            for (int i = 0; i < segmentations.Count; i++)
            {
                List<Segment> current_segmentation = segmentations[i];
                List<string> current_segmentation_tags = segmentation_tags[i];
                
                // Just speak all the tags.
                description_text += headers_to_segment[i] + " for " + character_to_segment + " ";
                foreach (string tag in current_segmentation_tags)
                {
                    description_text += tag + " ";
                    if (previous_segmentation_tags.Contains(tag))
                        description_text += "like " + headers_to_segment[i - 1] + " ";
                }//end foreach

                for (int j = 0; j < current_segmentation.Count; j++)
                {
                    Segment current_segment = current_segmentation[j];
                    if (j == 0)
                        description_text += "At first, ";
                    else
                        description_text += "Then, ";
                    foreach (string tag in current_segment.tags)
                        description_text += tag + " ";
                }//end foreach
                previous_segmentation_tags = current_segmentation_tags;
            }//end for
            Console.WriteLine(description_text);

            /*
            foreach (string character_name in character_names)
            {
                List<Segment> current_segmentation = new List<Segment>();
                current_segmentation = SegmentData(character_name, header_to_segment, number_of_segments);
                segmentations_by_character.Add(character_name, current_segmentation);
            }//end foreach

            foreach (KeyValuePair<string, List<Segment>> segmentation_entry in segmentations_by_character)
            {
                // Describe the segmentation.
                Console.WriteLine(DescribeSegmentation(segmentation_entry.Value, segmentation_entry.Key, header_to_segment));
            }//end foreach*/
        }//end try
        catch (Exception e)
        {
            character_names = new List<string>();
        }//end catch
    }//end constructor DataStoryteller
    
    // Determine tags for an entire segmentation.
    private List<string> DetermineSegmentationTags(List<Segment> segmentation_in)
    {
        List<string> return_tags = new List<string>();

        // See where the peak and the trough are.
        int min_segment_index = 0;
        double min_segment_value = double.MaxValue;
        int max_segment_index = 0;
        double max_segment_value = double.MinValue;
        // How many go up and how many go down.
        int segments_up = 0;
        int segments_down = 0;
        bool w_pattern = true;
        // Whether or not we are checking for a down or an up in the W pattern.
        bool w_down = true;
        for (int i = 0; i < segmentation_in.Count; i++)
        {
            if (segmentation_in[i].max_value > max_segment_value)
            {
                max_segment_index = i;
                max_segment_value = segmentation_in[i].max_value;
            }//end if
            if (segmentation_in[i].min_value < min_segment_value)
            {
                min_segment_index = i;
                min_segment_value = segmentation_in[i].min_value;
            }//end if
            if (segmentation_in[i].tags.Contains("decreases"))
            {
                segments_down += 1;
                if (w_down)
                    w_down = false;
                else
                    w_pattern = false;
            }//end if
            else if (segmentation_in[i].tags.Contains("increases"))
            {
                segments_up += 1;
                if (!w_down)
                    w_down = true;
                else
                    w_pattern = false;
            }//end else if
        }//end for
        if (max_segment_index == 0)
            return_tags.Add("peaks very early");
        else if (max_segment_index == 1)
            return_tags.Add("peaks early");
        else if (max_segment_index == 2)
            return_tags.Add("peaks late");
        else if (max_segment_index == 3)
            return_tags.Add("peaks very late");

        if (min_segment_index == 0)
            return_tags.Add("troughs very early");
        else if (min_segment_index == 1)
            return_tags.Add("troughs early");
        else if (min_segment_index == 2)
            return_tags.Add("troughs late");
        else if (min_segment_index == 3)
            return_tags.Add("troughs very late");

        // Check for mostly up, mostly down, and W shapes.
        if (w_pattern)
            return_tags.Add("w pattern");
        if (segments_up > segments_down)
            return_tags.Add("upwards pattern");
        else if (segments_down > segments_up)
            return_tags.Add("downward pattern");

        return return_tags;
    }//end method DetermineSegmentationTags

    // Helper functions to determine tags for a segment.
    private List<string> DetermineSegmentTags(Segment segment_in)
    {
        List<string> return_tags = new List<string>();

        // Magnitude tags for the start and end.
        return_tags.AddRange(DetermineMagnitudeTags(segment_in));
        return_tags.AddRange(DetermineSlopeTags(segment_in));

        return return_tags;
    }//end method DetermineTags

    private List<string> DetermineMagnitudeTags(Segment segment_in)
    {
        List<string> magnitude_tags = new List<string>();
        // Is the start low, middle, or high?
        double low_amount = 0;
        double middle_amount = 1.5;
        double high_amount = 3;
        double start_value = segment_in.point_a.y;
        double start_diff_low = Math.Abs(start_value - low_amount);
        double start_diff_middle = Math.Abs(start_value - middle_amount);
        double start_diff_high = Math.Abs(start_value - high_amount);
        if (start_diff_low < start_diff_middle && start_diff_low < start_diff_high)
            magnitude_tags.Add("starts low");
        else if (start_diff_high < start_diff_middle)
            magnitude_tags.Add("starts high");
        else
            magnitude_tags.Add("starts middle");
        // Is the end low, middle, or high?
        double end_value = segment_in.point_a.y;
        double end_diff_low = Math.Abs(end_value - low_amount);
        double end_diff_middle = Math.Abs(end_value - middle_amount);
        double end_diff_high = Math.Abs(end_value - high_amount);
        if (end_diff_low < end_diff_middle && end_diff_low < end_diff_high)
            magnitude_tags.Add("ends low");
        else if (end_diff_high < end_diff_middle)
            magnitude_tags.Add("ends high");
        else
            magnitude_tags.Add("ends middle");

        return magnitude_tags;
    }//end method DetermineMagnitudeTags
    private List<string> DetermineSlopeTags(Segment segment_in)
    {
        List<string> slope_tags = new List<string>();

        // Which direction is the segment going?
        if (segment_in.slope < 0)
            slope_tags.Add("decreases");
        else if (segment_in.slope > 0)
            slope_tags.Add("increases");
        else
            slope_tags.Add("stays steady");

        // Magnitude of the slope determines whether it's quickly or slowly
        // Only either slow or quick right now.
        double quick_threshold = 0.001;
        if (Math.Abs(segment_in.slope) > quick_threshold)
            slope_tags.Add("quickly");
        else
            slope_tags.Add("slowly");

        double long_threshold = 1092;
        if (segment_in.time_span > long_threshold)
            slope_tags.Add("long");
        else
            slope_tags.Add("short");

        return slope_tags;
    }//end method DetermineMagnitudeTags

    // Helper function to describe magnitude.
    private String MagnitudeDescriptor(double value)
    {
        // Define what values are low, middle, and high for this segmentation
        double low = 1;
        double middle = 1.5;
        double high = 2;

        double difference_low = Math.Abs(value - low);
        double difference_middle = Math.Abs(value - middle);
        double difference_high = Math.Abs(value - high);

        if (difference_low < difference_middle && difference_low < difference_high)
        {
            return "low";
        }//end if
        else if (difference_high < difference_low && difference_high < difference_middle)
        {
            return "high";
        }//end else if
        else
        {
            return "in the middle";
        }//end else if
    }//end method
    // Helper function to describe slope
    private String SlopeDescriptor(Segment segment)
    {
        String descriptor = "";
        // Which direction is it going?
        if (segment.slope < 0)
        {
            descriptor = "decreases ";
        }//end if
        else if (segment.slope > 0)
        {
            descriptor = "increases ";
        }//end else if

        // Magnitude of the slope determines whether it's quickly or slowly
        // Only either slow or quick right now.
        double quick_threshold = 0.001;
        if (Math.Abs(segment.slope) > quick_threshold)
            descriptor += "quickly ";
        else
            descriptor += "slowly ";

        double long_threshold = 1092;
        if (segment.time_span > long_threshold)
            descriptor += "for a long time ";
        else
            descriptor += "for a short time ";

        //descriptor += "between " + (new DateTime((long)segment.point_a.x)).ToString() + " and " + (new DateTime((long)segment.point_b.x)).ToString();
        descriptor += "starting in " + (new DateTime((long)segment.point_a.x)).ToString();
        descriptor += ", going from " + segment.point_a.y.ToString() + " to " + segment.point_b.y.ToString() + " in " + segment.time_span + " days";

        return descriptor;
    }//end method SlopeDescriptor
    private String DescribeSegmentation(List<Segment> segmentation, String character_name, String value_header)
    {
        String description = "";
        
        // Go through each segment and add descriptive tags too it.
        for (int i = 0; i < segmentation.Count; i++)
        {
            segmentation[i].tags = DetermineSegmentTags(segmentation[i]);
        }//end for

        description = "\n" + value_header + " for " + character_name + " starts " + MagnitudeDescriptor(segmentation[0].point_a.y) + " at " + segmentation[0].point_a.y.ToString() + ". ";
        description += "This is in " + (new DateTime((long)segmentation[0].point_a.x)).ToString() + ". ";
        description += "From there, ";
        for (int i = 0; i < segmentation.Count; i++)
        {
            description += " it " + SlopeDescriptor(segmentation[i]) + ".\n ";
        }//end for
        description += "Finally, in " + (new DateTime((long)segmentation[0].point_b.x)).ToString() + ", " + value_header + " for " + character_name + " ends " + MagnitudeDescriptor(segmentation[0].point_b.y) + " at " + segmentation[segmentation.Count - 1].point_b.y.ToString() + ". ";

        return description;
    }//end method DescribeSegmentation

    private List<Segment> SegmentData(String character_name, string header_name, int number_of_segments)
    {
        Console.WriteLine("========== Segmenting for " + character_name + " ==========");
        DataSegmenter segmenter = new DataSegmenter();
        List<Segment> segmentation = segmenter.SegmentByPLA(data_by_character[character_name][header_name], number_of_segments);
        int segment_counter = 0;
        foreach (Segment segment in segmentation)
        {
            Console.WriteLine("Segment: " + segment.ToString());
        }//end foreach
        Console.WriteLine("Segmentation complete.");

        return segmentation;
    }//end method SegmentData
}