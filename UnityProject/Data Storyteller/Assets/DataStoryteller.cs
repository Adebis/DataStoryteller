using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Linq;

public class DataStoryteller {

    public List<Node> nodes;
    public List<Edge> edges;

    // For parsing the data from the files.
    public List<string> character_names;

    // Key1 is the name of the character. Value1 is the DataSet for that character.
    // Each DataPoint of the DataSet consists of the date of the measurement,
    // the secchi depth of the measurement, and the name of the character the measurement
    // belongs to.
    public Dictionary<string, DataSet> data_by_character;

    // Key1 is the name of the character. Key2 is the name of the data, Value2 is the data itself.
    // Stores data about each character, as read from the data files.
    //public Dictionary<string, Dictionary<string, string>> data_by_character

    //public Dictionary<string
    public List<Node> story_nodes;

    public string unity_log;

    public DataStoryteller()
    {
        try
        {
            unity_log = "";
            character_names = new List<string>();

            data_by_character = new Dictionary<string, DataSet>();

            // The application path is in the assets folder of Unity.
            string application_path = Application.dataPath;

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

            // We are interested in Zsec (secchi depth), index 3 of each row.
            // The characters will be each site (by name, not code).
            // Each character will have a time-series of Zsec data. Date is index 2 of each row.
            // Go through all of the csv data.
            // Headers:
            // SITE,Z,Date,Zsec,PH,COND,ALK,OP,TFP,TP,CL,NO3,SO4,TN,NH4,SI,Na,Mg,Ca,K,Fe,CHLA,T,DO (mg/l),DO (sat),Zcomp (m)
            foreach (Dictionary<string, string> data_row in csv_data)
            {
                // Create a DataPoint from this data row.
                DataPoint new_point = new DataPoint();
                bool populate_success = new_point.PopulateFromLakeGeorgeData(data_row);
                if (populate_success == false)
                    continue;
                string site_name = data_row["SITE"];

                // Check to see if this site_name is in data by character.
                if (!data_by_character.ContainsKey(site_name))
                {
                    // If not, then initialize a new dataset for it.
                    data_by_character.Add(site_name, new DataSet());
                }//end if
                bool entry_added = false;
                while (!entry_added)
                {
                    // Add this DateTime and this secchi depth to the sorted dictionary for this site name.
                    data_by_character[site_name].AddDatapoint(new_point);
                    entry_added = true;
                    break;
                }//end while
            }//end foreach

            // The keys for data_by_character now contains each character name.
            // Populate the list of character names using these.
            foreach (KeyValuePair<string, DataSet> temp_item in data_by_character)
                character_names.Add(temp_item.Key);
            // Make a node for each character. Each character is connected to every other character.
            MakeCharacterNodes();

            // Create a story based on a character and a curve.
            // NOTE: We assume that story percentage is equal to percentage of data points
            // Each curve type has an IDEAL CURVE equation (e.g., man-in-hole is a parabola that ends higher than it started)
            // - The ideal curve equation is based on where in the data it starts and what length of time we want to cover.
            // To find the events for a story:
            // 1. Choose a curve type, a character, and a story length.
            // 2. Start a candidate story by finding a set of Known Points for the curve type, given:
            //  - The set of data points for the character.
            //  - A starting point (the first data point/event)
            //  - Story length (by percentage of total data points/events covered, NOT time)
            //  NOTE: Each curve type will have their own set of known points and method for finding known points.
            // 3. Find the Ideal Curve Equation given:
            //  - The known points (including the starting point)
            //  NOTE: Each curve type will have their own method of calculating an ideal curve equation.
            // 4. Find the error bewteen the candidate story and the ideal curve.
            //  - For each data-point, find the difference between the point and its ideal value from the Ideal Curve Equation.
            //  - Sum the error over all points from the start to the end, inclusive. 
            // 5. Choose the candidate story with the lowest error. 
            // INPUTS: Curve Type, Character Dataset, Story Length
            // Candidate scanning: Need to find a way to limit the number of candidates analyzed!

            // Man-In-A-Hole: Start somewhere positive, middle is somewhere lower than the start, end is higher than both
            // the start and the end. Middle should be halfway between the start and the end (or in that neighborhood).
            // FIRST PASS: Make a Man-In-A-Hole over an entire dataset (100% length), given a character.
            // INPUTS: Character + its dataset.
            // Tuning Parameters: middle neighborhood (# of nodes close to middle to consider, by percentage), end neighborhood (by percentage), start neighborhood (by percentage)
            // Method: Part 1: Generate Candidates
            // Start with the absolute start node, the absolute middle node, and the absolute end node.
            //      Gather the set of nodes in the start neighborhood, the set in the middle neighborhood, and the set in the end neighborhood.
            // Check each start from the very start to each node in its neighborhood.
            // For each of these^, check each middle from the absolute middle to each node in the neighborhood.
            // For each of these^, check each end from the absolute end to each node in its neighborhood.
            // Check the curve type to see if the trio is a valid. Create a candidate story for each valid start/end/middle trio.
            
            // Part 2: Score candidates and pick best one.
            // For each candidate, interpolate the equation for the ideal curve, using the starting points.
            // Calculate the distance between the ACTUAL curve and the IDEAL curve (the error)
            // The candidate with the least error is the best candidate. Pick it as the story.
            
            // 0 = single character single story
            // 1 = multi character single story
            int gen_index = 1;
            if (gen_index == 0)
            {
                string main_character_name = "Northwest Bay";
                //string main_character_name = "French Point";
                DataSet main_character_dataset = data_by_character[main_character_name];

                string story_shape = "man_in_hole";
                StoryMaker story_maker = new StoryMaker();
                Story story = story_maker.MakeStory(story_shape, main_character_dataset);
                // Make a node for each initial point in the story's curve.
                Dictionary<string, Node> initial_point_nodes = new Dictionary<string, Node>();
                //Node main_character_node = GetNode(main_character_name);
                foreach (KeyValuePair<string, DataPoint> initial_point in story.curve.initial_points)
                {
                    Node point_node = new Node(initial_point.Key.ToString());

                    point_node.node_data.Add("date", initial_point.Value.time.ToString());
                    point_node.node_data.Add("secchi_depth", initial_point.Value.value.ToString());

                    initial_point_nodes.Add(point_node.name, point_node);
                    // Add this node to the global list of nodes.
                    nodes.Add(point_node);
                    // Note this as a story node
                    point_node.is_story_node = true;
                    story_nodes.Add(point_node);
                    // Link all point nodes to the character the data point came from.
                    Node point_character_node = GetNode(initial_point.Value.character);
                    MakeEdge(point_node, point_character_node);
                }//end foreach
                // Link the start to the middle and the middle to the end.
                MakeEdge(initial_point_nodes["start"], initial_point_nodes["mid"]);
                MakeEdge(initial_point_nodes["mid"], initial_point_nodes["end"]);

                // Print the entire datapoint set of the story to a csv file.
                List<string> ordered_datapoint_text = new List<string>();
                string row_text_to_add = "";
                foreach (DataPoint point in story.curve.points_represented)
                {
                    row_text_to_add = point.time.ToString() + " : " + point.value.ToString();
                    // Search for an initial point that contains this datapoint.
                    foreach (KeyValuePair<string, DataPoint> initial_entry in story.curve.initial_points)
                        //if (initial_entry.Value.value == point.value
                        //    && initial_entry.Value.time == point.time
                        //    && initial_entry.Value.character == point.character)
                        if (initial_entry.Value.Equals(point))
                            row_text_to_add += " : " + initial_entry.Key;
                    ordered_datapoint_text.Add(row_text_to_add);
                }//end foreach
                string[] text_array_to_write = ordered_datapoint_text.ToArray();
                System.IO.File.WriteAllLines("outputlog.txt", text_array_to_write);
            }//end if
            else if (gen_index == 1)
            {
                // Multi-character single story.
                List<string> story_character_names = new List<string>();
                //story_character_names.Add("French Point");
                //story_character_names.Add("Sabbath Day Point");
                //story_character_names.Add("Smith Bay");
                story_character_names.Add("Rogers Rock");
                //story_character_names.Add("Northwest Bay");
                //story_character_names.Add("Basin Bay");
                //story_character_names.Add("Dome Island");
                //story_character_names.Add("Tea Island");
                //story_character_names.Add("Green Island");
                //story_character_names.Add("Anthonys Nose");
                //story_character_names.Add("Calves Pen");
                // Make a dataset combining all the characters' datasets.
                DataSet all_characters_dataset = new DataSet();
                foreach (string char_name in story_character_names)
                {
                    all_characters_dataset.MergeDataset(data_by_character[char_name]);
                }//end foreach

                string story_shape = "man_in_hole";
                StoryMaker story_maker = new StoryMaker();
                Story story = story_maker.MakeStory(story_shape, all_characters_dataset);
                // Make a node for each initial point in the story's curve.
                Dictionary<string, Node> initial_point_nodes = new Dictionary<string, Node>();
                //Node main_character_node = GetNode(main_character_name);
                foreach (KeyValuePair<string, DataPoint> initial_point in story.curve.initial_points)
                {
                    Node point_node = new Node(initial_point.Key.ToString());

                    point_node.node_data.Add("date", initial_point.Value.time.ToString());
                    point_node.node_data.Add("secchi_depth", initial_point.Value.value.ToString());

                    initial_point_nodes.Add(point_node.name, point_node);
                    // Add this node to the global list of nodes.
                    nodes.Add(point_node);
                    // Note this as a story node
                    point_node.is_story_node = true;
                    story_nodes.Add(point_node);
                    // Link all point nodes to the character the data point came from.
                    Node point_character_node = GetNode(initial_point.Value.character);
                    MakeEdge(point_node, point_character_node);
                }//end foreach
                // Link the start to the middle and the middle to the end.
                MakeEdge(initial_point_nodes["start"], initial_point_nodes["mid"]);
                MakeEdge(initial_point_nodes["mid"], initial_point_nodes["end"]);

                // Print the entire datapoint set of the story to a csv file.
                List<string> ordered_datapoint_text = new List<string>();
                string row_text_to_add = "";
                foreach (DataPoint point in story.curve.points_represented)
                {
                    double ideal_value = story.curve.ideal_curve_interpolator.Interpolate(story.curve.dataset.time_sorted_data.IndexOf(point));
                    row_text_to_add = point.time.ToString() + " | actual: " + point.value.ToString()
                            + " ideal: " + ideal_value.ToString() + " deviation: " + (Math.Abs(ideal_value - point.value)).ToString();
                    // Search for an initial point that contains this datapoint.
                    foreach (KeyValuePair<string, DataPoint> initial_entry in story.curve.initial_points)
                        //if (initial_entry.Value.value == point.value
                        //    && initial_entry.Value.time == point.time
                        //    && initial_entry.Value.character == point.character)
                        if (initial_entry.Value.Equals(point))
                            row_text_to_add += " | initial_point: " + initial_entry.Key;
                    ordered_datapoint_text.Add(row_text_to_add);
                }//end foreach
                ordered_datapoint_text.Add("total deviation from ideal: " + story.curve.total_distance_from_ideal.ToString());
                ordered_datapoint_text.Add("average deviation from ideal: " + story.curve.average_deviation_from_ideal.ToString());
                string[] text_array_to_write = ordered_datapoint_text.ToArray();
                System.IO.File.WriteAllLines("outputlog.txt", text_array_to_write);
            }//end else if



            // The 'shape' of our story. This defines the curve for the story.
            // A good graphic on these shapes: https://visual.ly/community/infographic/other/kurt-vonnegut-shapes-stories-0?utm_source=visually_embed
            // Shapes:
            //  man_in_hole
            //      - Start positive
            //      - Dip to lower point in middle
            //      - End higher than beginning
            //  boy meets girl
            //      - As previous, but ends way way higher
            //  from bad to worse
            //      - start bad, get worse
            //  which way is up?
            //      - no discernable shape
            //  creation story
            //      - start from middle/nowhere, go up incrementally.
            //  old testament
            //      - creation story that drops low at end
            //  new testament
            //      - creation story that drops low, then ends very high
            //  cinderella
            //      - Almost the same as new testament.
        }//end try
        catch (Exception e)
        {
            character_names = new List<string>();
        }//end catch
    }//end constructor DataStoryteller

    // Transform a character's data into a set of events. Try to conform to the given story shape.
    /*private Dictionary<string, KeyValuePair<DateTime, double>> DataToEvents(string character_name, string story_shape)
    {
        // The map of dates to data values for the given character, sorted by date.
        SortedDictionary<DateTime, double> time_data_map = data_by_character[character_name];

        /*foreach (KeyValuePair<DateTime, double> time_data_entry in time_data_map)
        {
            DateTime date = time_data_entry.Key;
            double secchi_depth = time_data_entry.Value;
        }//end foreach
         // Determine the events for this character's data.
        List<Dictionary<string, string>> event_list = new List<Dictionary<string, string>>();

        // Each story will consist of 3 to 5 events. 
        // Here, create events according to the story shape.
        // NOTE: Secchi depth is taken as contributing directly to the sentiment of the story. 
        // Higher secchi depth is better.
        // For each story in the list of potential stories, Key1 is the name of the story event, Value1 is the key-value pair of that event.
        // For the key-value pair, Key2 is a date, Value2 is the secchi depth measured at that date.
        List<Dictionary<string, KeyValuePair<DateTime, double>>> potential_stories = new List<Dictionary<string, KeyValuePair<DateTime, double>>>();
        if (story_shape.Equals("man_in_hole"))
        {
            // For man in hole, we want a middle that is the lowest point,
            // a beginning that is a high point, and an end that is a slightly higher point.
            // The selection loop will continue until all choices are exhausted.
            // First, choose the beginning.
            foreach (KeyValuePair<DateTime, double> time_data_entry_1 in time_data_map)
            {
                DateTime date_1 = time_data_entry_1.Key;
                double secchi_depth_1 = time_data_entry_1.Value;

                DateTime start_date = date_1;
                double start_secchi_depth = secchi_depth_1;
                KeyValuePair<DateTime, double> start_entry = time_data_entry_1;
                // Next, choose the end.
                foreach (KeyValuePair<DateTime, double> time_data_entry_2 in time_data_map.Reverse())
                {
                    DateTime date_2 = time_data_entry_2.Key;
                    double secchi_depth_2 = time_data_entry_2.Value;
                    KeyValuePair<DateTime, double> end_entry = time_data_entry_2;
                    DateTime end_date = date_2;
                    // The end must be after the beginning.
                    if (end_date <= start_date)
                        continue;

                    double end_secchi_depth = secchi_depth_2;
                    // The end must be higher than the start.
                    if (end_secchi_depth <= start_secchi_depth)
                        continue;

                    // Now that we have a start and an end, choose a middle.
                    foreach (KeyValuePair<DateTime, double> time_data_entry_3 in time_data_map.Reverse())
                    {
                        DateTime date_3 = time_data_entry_3.Key;
                        double secchi_depth_3 = time_data_entry_3.Value;
                        KeyValuePair<DateTime, double> middle_entry = time_data_entry_3;
                        DateTime middle_date = date_3;
                        // The date must be after the start and before the end.
                        if (middle_date <= start_date || middle_date >= end_date)
                            continue;

                        double middle_secchi_depth = secchi_depth_3;
                        // The middle depth must be lower than the start and lower than the end.
                        if (middle_secchi_depth >= start_secchi_depth || middle_secchi_depth >= end_secchi_depth)
                            continue;

                        // If we have reached this point, then all three dates and depths are valid.
                        // Add the set of events the list of potential stories.
                        Dictionary<string, KeyValuePair<DateTime, double>> potential_story = new Dictionary<string, KeyValuePair<DateTime, double>>();
                        potential_story.Add("start", start_entry);
                        potential_story.Add("middle", middle_entry);
                        potential_story.Add("end", end_entry);
                        potential_stories.Add(potential_story);
                    }//end foreach
                }//end foreach
            }//end foreach
            
            // Now we have a list of candidate stories. They must be evaluated to choose the best one.
            float highest_score = 0;
            Dictionary<string, KeyValuePair<DateTime, double>> best_story = new Dictionary<string, KeyValuePair<DateTime, double>>();
            foreach (Dictionary<string, KeyValuePair<DateTime, double>> potential_story in potential_stories)
            {
                KeyValuePair<DateTime, double> start_event = potential_story["start"];
                DateTime start_date = start_event.Key;
                double start_secchi_depth = start_event.Value;

                KeyValuePair<DateTime, double> middle_event = potential_story["middle"];
                DateTime middle_date = middle_event.Key;
                double middle_secchi_depth = middle_event.Value;

                KeyValuePair<DateTime, double> end_event = potential_story["end"];
                DateTime end_date = end_event.Key;
                double end_secchi_depth = end_event.Value;

                // Calculate the score for this story.
                // First, check the curve from the beginning to the middle.
                // In an ideal man in hole, this goes continually down.
                float number_conforming = 0;
                float total_number = 0;
                double previous_secchi_depth = start_secchi_depth;
                double current_secchi_depth = 0;

                foreach (KeyValuePair<DateTime, double> time_data_entry_temp in time_data_map)
                {
                    // Don't do anything before the start date and after the middle date.
                    if (time_data_entry_temp.Key <= start_date || time_data_entry_temp.Key > middle_date)
                        continue;
                    current_secchi_depth = time_data_entry_temp.Value;
                    // Check that the current depth is decreasing from the previous depth. If so, then this step in the story is conforming.
                    if (current_secchi_depth < previous_secchi_depth)
                        number_conforming += 1;
                    total_number += 1;
                    previous_secchi_depth = current_secchi_depth;
                }//end foreach

                // Next, check the curve from the middle to the end.
                // For the second half of the story, in an ideal man in hole, the story goes continually up.
                previous_secchi_depth = middle_secchi_depth;
                current_secchi_depth = 0;
                foreach (KeyValuePair<DateTime, double> time_data_entry_temp in time_data_map)
                {
                    // Don't do anything for dates before the middle date or after the end date.
                    if (time_data_entry_temp.Key <= middle_date || time_data_entry_temp.Key > end_date)
                        continue;
                    current_secchi_depth = time_data_entry_temp.Value;
                    // Check that the current depth is increasing from the previous depth. If so, then this step in the story is conforming.
                    if (current_secchi_depth > previous_secchi_depth)
                        number_conforming += 1;
                    total_number += 1;
                    previous_secchi_depth = current_secchi_depth;
                }//end for
                // This is the percent conformity of the story to the expected curve.
                float percent_conformity = number_conforming / total_number;

                // Check how much of the character's total data this story covers.
                int total_events = time_data_map.Count;
                float events_included = 0;
                // Count the number of entries that from the start date to the end date, inclusive.
                foreach (KeyValuePair<DateTime, double> time_data_entry_temp in time_data_map)
                {
                    if (time_data_entry_temp.Key >= start_date && time_data_entry_temp.Key <= end_date)
                        events_included += 1;
                }//end foreach

                float coverage = events_included / (float)total_events;

                // Take the average of the two to calculate the score.
                float current_score = (percent_conformity + coverage) / 2;

                // Pick the highest conforming story.
                if (current_score > highest_score)
                {
                    highest_score = current_score;
                    best_story = potential_story;
                }//end if
            }//end foreach
            return best_story;
        }//end if

        return null;
    }//end method DataToEvents*/

    // Transform a set of events into a story corresponding to a given shape.
    /*private List<Node> EventsToStory(List<Dictionary<string, Object>> events, string shape)
    {
        List<Node> story = new List<Node>();

        return story;
    }//end method EventsToStory*/

    // Make a node for each character in the global list of character names.
    private void MakeCharacterNodes()
    {
        // A graph is a collection of nodes with edges.
        nodes = new List<Node>();
        edges = new List<Edge>();
        story_nodes = new List<Node>();

        // Make a node for each character
        List<Node> character_nodes = new List<Node>();
        foreach (string character_name in character_names)
        {
            Node character_node = new Node(character_name);
            nodes.Add(character_node);
            character_nodes.Add(character_node);
        }//end foreach
        foreach (Node character_node in character_nodes)
        {
            // Make an edge between each character node.
            foreach (Node other_char_node in character_nodes)
            {
                if (!character_node.name.Equals(other_char_node.name))
                    MakeEdge(character_node, other_char_node);
            }//end foreach
        }//end foreach
    }//end method MakeCharacterNodes

    // Get a node by name
    public Node GetNode(string node_name)
    {
        foreach (Node node in nodes)
            if (node.name.Equals(node_name))
                return node;
        return null;
    }//end method GetNode
    private void MakeEdge(Node source, Node dest)
    {
        edges.Add(new Edge(source, dest));
        source.AddNeighbor(dest);
        dest.AddNeighbor(source);
    }//end method MakeEdge

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
