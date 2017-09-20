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
    // Each DataPoint of the DataSet consists of the date of the measurement,
    // the secchi depth of the measurement, and the name of the character the measurement
    // belongs to.
    public Dictionary<string, List<DataPoint>> data_by_character;

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

            data_by_character = new Dictionary<string, List<DataPoint>>();

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

            // Look at time series of chlorophyll concentrations for each character (site).
            // Chlorophyll is CHLA.
            // Make a DataPoint out of each data row.
            foreach (Dictionary<string, string> data_row in all_data)
            {
                // Create a new datapoint with Chlorophyll A as the main value.
                DataPoint new_datapoint = new DataPoint(data_row, "CHLA");
                // If there is a valid Chlorophyll A value, add the datapoint to a character's list of datapoints.
                if (new_datapoint.population_success)
                {
                    // Check which character it is under.
                    string current_character = new_datapoint.character;
                    // Add the datapoint to the appropriate list of datapoints per character.
                    if (data_by_character.ContainsKey(current_character))
                    {
                        data_by_character[current_character].Add(new_datapoint);
                    }//end if
                    else
                    {
                        data_by_character.Add(current_character, new List<DataPoint>());
                        data_by_character[current_character].Add(new_datapoint);
                    }//end else
                }//end if
            }//end foreach

            // We now have a list of datapoints for each character's chlorophyll A level.
            // Run segmenter on a single character's datapoints at a time.
            int number_of_segments = 10;
            //string character_name = "Northwest Bay";
            // Characters:
            //  Northwest Bay
            //  French Point
            Dictionary<String, List<Segment>> segmentations_by_character = new Dictionary<String, List<Segment>>();
            character_names = site_code_map.Values.ToList();
            foreach (string character_name in character_names)
            {
                List<Segment> current_segmentation = new List<Segment>();
                current_segmentation = SegmentData(character_name, number_of_segments);
                segmentations_by_character.Add(character_name, current_segmentation);
            }//end foreach

            foreach (KeyValuePair<string, List<Segment>> segmentation_entry in segmentations_by_character)
            {
                // Describe the segmentation.
                Console.WriteLine(DescribeSegmentation(segmentation_entry.Value, segmentation_entry.Key, "Chlorophyll A"));
            }//end foreach
        }//end try
        catch (Exception e)
        {
            character_names = new List<string>();
        }//end catch
    }//end constructor DataStoryteller

    private String DescribeSegmentation(List<Segment> segmentation, String character_name, String value_header)
    {
        String description = "";

        
        description = "\n" + value_header + " for " + character_name + " starts " + MagnitudeDescriptor(segmentation[0].point_a.y) + " at " + segmentation[0].point_a.y.ToString() + ". ";
        description += "This is in " + (new DateTime((long)segmentation[0].point_a.x)).ToString() + ". ";
        description += "From there, ";
        for (int i = 1; i < segmentation.Count - 1; i++)
        {
            description += " it " + SlopeDescriptor(segmentation[i]) + ".\n ";
        }//end for
        description += "Finally, in " + (new DateTime((long)segmentation[0].point_a.x)).ToString() + ", " + value_header + " for " + character_name + " ends " + MagnitudeDescriptor(segmentation[0].point_a.y) + " at " + segmentation[segmentation.Count - 1].point_b.y.ToString() + ". ";

        return description;
    }//end method DescribeSegmentation
    
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

    private List<Segment> SegmentData(String character_name, int number_of_segments)
    {
        Console.WriteLine("========== Segmenting for " + character_name + " ==========");
        DataSegmenter segmenter = new DataSegmenter();
        List<Segment> segmentation = segmenter.SegmentByPLA(data_by_character[character_name], number_of_segments);
        int segment_counter = 0;
        foreach (Segment segment in segmentation)
        {
            Console.WriteLine("Segment: " + segment.ToString());
        }//end foreach
        Console.WriteLine("Segmentation complete.");

        return segmentation;
    }//end method SegmentData
}