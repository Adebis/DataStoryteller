using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataStoryteller {

    public List<Node> nodes;
    public List<Edge> edges;

    // For parsing the data from the files.
    public List<string> character_names;
    // The number of citations for each year, by character.
    // Key1 is the name of the character. Key2 is the year. Value2 is the number of citations that year.
    public Dictionary<string, Dictionary<int, int>> yearly_citations_by_algorithm;
    // Key1 is the name of the character. Key2 is the name of the data, Value2 is the data itself.
    // Stores data about each character, as read from the data files.
    public Dictionary<string, Dictionary<string, string>> data_by_character;

    // Has data for each paper for each character.
    // Key1 is the character name. Value1 is the list of paper data.
    // Key2 is the name of the piece of information (e.g. year, citations, authors). Value2 is the information itself.
    public Dictionary<string, List<Dictionary<string, string>>> papers_by_character;

    //public Dictionary<string
    public List<Node> story_nodes;

    public DataStoryteller()
    {
        character_names = new List<string>{"artificial_neural_network"
                            , "decision_tree"
                            , "k-means_clustering"
                            , "linear_regression"
                            , "support_vector_machine" };

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

        yearly_citations_by_algorithm = new Dictionary<string, Dictionary<int, int>>();
        papers_by_character = new Dictionary<string, List<Dictionary<string, string>>>();

        data_by_character = new Dictionary<string, Dictionary<string, string>>();

        // The application path is in the assets folder of Unity.
        string application_path = Application.dataPath;

        // Headers:
        // Cites,Authors,Title,Year,Source,Publisher,ArticleURL,CitesURL,GSRank,QueryDate,Type,DOI,ISSN,CitationURL,Volume,Issue,StartPage,EndPage,ECC

        // Go through each algorithm and gather its data from the CSVs
        foreach (string character_name in character_names)
        {
            // Initialize some data things for this character.
            //papers_by_character.Add(character_name, new List<Dictionary<string, string>>());
            // Data for each paper
            List<Dictionary<string, string>> paper_list = new List<Dictionary<string, string>>();
            // Number of citations for each year.
            Dictionary<int, int> citations_by_year = new Dictionary<int, int>();
            // Total citation count
            int total_citations = 0;

            string data_path = application_path + "/data/" + character_name + ".csv";
            string[] file_lines = File.ReadAllLines(data_path);
            bool first_line = true;
            foreach (string file_line in file_lines)
            {
                // Skip the header.
                if (first_line)
                {
                    first_line = false;
                    continue;
                }//end if
                // These CSVs are separated by commas.
                string[] separated_line = file_line.Split(',');
                // The number of citations
                string cites_string = separated_line[0];
                int citations = 0;
                int.TryParse(cites_string, out citations);
                total_citations += citations;

                string authors = separated_line[1].Replace("\"", "");
                string title = separated_line[2].Replace("\"", "");

                string year_string = separated_line[3];
                int year = 0;
                int.TryParse(year_string, out year);

                string source = separated_line[4].Replace("\"", "");
                string publisher = separated_line[5].Replace("\"", "");

                // Either create a new entry for this year, or increment the number of citations for this year.
                if (citations_by_year.ContainsKey(year))
                    citations_by_year[year] += citations;
                else
                    citations_by_year.Add(year, citations);

                // Create a new paper entry for this character.
                Dictionary<string, string> paper_entry = new Dictionary<string, string>();
                paper_entry.Add("citations", cites_string);
                paper_entry.Add("year", year_string);
                paper_entry.Add("authors", authors);
                paper_entry.Add("title", title);
                paper_entry.Add("source", source);
                paper_entry.Add("publisher", publisher);
                paper_list.Add(paper_entry);
            }//end foreach

            // Now that we have our list of paper data, add it to the global dictionary for this character.
            papers_by_character.Add(character_name, paper_list);
            // Additionally, we have the number of citations each year for this character. Add it to the global dictionary.
            yearly_citations_by_algorithm.Add(character_name, citations_by_year);
        }//end foreach

        // Our main character
        //string main_character_name = "artificial_neural_network";
        string main_character_name = "decision_tree";
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
        string story_shape = "man_in_hole";
        Dictionary<string, Dictionary<int, int>> main_character_events = DataToEvents(main_character_name, story_shape);
        // Make a node of each event.
        Dictionary<string, Node> event_nodes = new Dictionary<string, Node>();
        Node main_character_node = GetNode(main_character_name);
        foreach (KeyValuePair<string, Dictionary<int, int>> event_entry in main_character_events)
        {
            Node event_node = new Node(event_entry.Key);
            Dictionary<int, int> event_data = event_entry.Value;
            foreach (KeyValuePair<int, int> temp in event_data)
            {
                event_node.node_data.Add("year", temp.Key.ToString());
                event_node.node_data.Add("citations", temp.Value.ToString());
            }//end foreach
            event_nodes.Add(event_node.name, event_node);
            // Add this node to the global list of nodes.
            nodes.Add(event_node);
            // Note this as a story node
            event_node.is_story_node = true;
            story_nodes.Add(event_node);
            // Link all event nodes to the main character node.
            MakeEdge(event_node, main_character_node);
        }//end foreach
        // Link the start to the middle and the middle to the end.
        MakeEdge(event_nodes["start"], event_nodes["middle"]);
        MakeEdge(event_nodes["middle"], event_nodes["end"]);
        //List<Node> main_story = EventsToStory(main_character_events, story_shape);
    }//end constructor DataStoryteller

    // Transform a character's data into a set of events. Try to conform to the given story shape.
    private Dictionary<string, Dictionary<int, int>> DataToEvents(string character_name, string story_shape)
    {
        Dictionary<int, int> citations_by_year = yearly_citations_by_algorithm[character_name];
        //Dictionary<int, int> ordered_citations_by_year = new Dictionary<int, int>();
        // Determine the events for this character's data.
        List<Dictionary<string, string>> event_list = new List<Dictionary<string, string>>();
        int minimum_year = 1900;
        int maximum_year = 2018;
        int last_count = 0;
        for (int year = minimum_year; year <= maximum_year; year++)
        {
            // If this year doesn't appear in the dictionary, then no citations were made that year.
            int current_count = 0;
            if (citations_by_year.ContainsKey(year))
                current_count = citations_by_year[year];
            else
                citations_by_year.Add(year, 0);
            /*// Check for appear event
            // Where the count goes from 0 to anything positive.
            if (last_count == 0 && current_count > 0)
            {
                Dictionary<string, string> appear_event = new Dictionary<string, string>();
                appear_event.Add("type", "appear");
                appear_event.Add("time", year.ToString());
                appear_event.Add("actor", character_name);
            }//end if
            // Check for disappear event.
            // This is when the count goes from anything positive to 0.*/
        }//end for

        // Each story will consist of 3 to 5 events. 
        // Here, create events according to the story shape.
        // NOTE: Number of citations for each year is taken as contributing directly to the sentiment of the story.
        // For each story, Key is a year, Value is the number of citations that year.
        List<Dictionary<string, Dictionary<int, int>>> potential_stories = new List<Dictionary<string, Dictionary<int, int>>>();
        if (story_shape.Equals("man_in_hole"))
        {
            // For man in hole, we want a middle that is the lowest point,
            // a beginning that is a high point, and an end that is a slightly higher point.
            // The selection loop will continue until all choices are exhausted.
            // First, choose the beginning.
            for (int start_year = minimum_year; start_year < maximum_year; start_year++)
            {
                int start_count = citations_by_year[start_year];
                // Don't choose a 0 citation year for the beginning.
                if (start_count <= 0)
                    continue;
                // Next, choose the end.
                for (int end_year = maximum_year; end_year > start_year; end_year--)
                {
                    // The end must be higher than the start.
                    int end_count = citations_by_year[end_year];
                    if (end_count <= start_count)
                        continue;
                    // Now that we have a start and an end, choose a middle.
                    // It must be at least lower than the start year's citation count.
                    for (int middle_year = start_year + 1; middle_year < end_year; middle_year++)
                    {
                        int middle_count = citations_by_year[middle_year];
                        if (middle_count >= start_count || middle_count >= end_count)
                            continue;
                        // This is officially a candidate. Note the trio as a potential story.
                        Dictionary<string, Dictionary<int, int>> potential_story = new Dictionary<string, Dictionary<int, int>>();
                        Dictionary<int, int> start_entry = new Dictionary<int, int>();
                        start_entry.Add(start_year, start_count);
                        potential_story.Add("start", start_entry);
                        Dictionary<int, int> middle_entry = new Dictionary<int, int>();
                        middle_entry.Add(middle_year, middle_count);
                        potential_story.Add("middle", middle_entry);
                        Dictionary<int, int> end_entry = new Dictionary<int, int>();
                        end_entry.Add(end_year, end_count);
                        potential_story.Add("end", end_entry);
                        potential_stories.Add(potential_story);
                    }//end for
                }//end for
            }//end for
            // Now we have a list of candidate stories. They must be evaluated to choose the best one.
            float highest_score = 0;
            Dictionary<string, Dictionary<int, int>> best_story = new Dictionary<string, Dictionary<int, int>>();
            foreach (Dictionary<string, Dictionary<int, int>> potential_story in potential_stories)
            {
                Dictionary<int, int> start_event = potential_story["start"];
                int start_year = 0;
                int start_count = 0;
                foreach (KeyValuePair<int, int> temp in start_event)
                {
                    start_year = temp.Key;
                    start_count = temp.Value;
                }//end foreach
                Dictionary<int, int> middle_event = potential_story["middle"];
                int middle_year = 0;
                int middle_count = 0;
                foreach (KeyValuePair<int, int> temp in middle_event)
                {
                    middle_year = temp.Key;
                    middle_count = temp.Value;
                }//end foreach
                Dictionary<int, int> end_event = potential_story["end"];
                int end_year = 0;
                int end_count = 0;
                foreach (KeyValuePair<int, int> temp in end_event)
                {
                    end_year = temp.Key;
                    end_count = temp.Value;
                }//end foreach
                // Calculate the score for this story.
                // First, check the curve from the beginning to the middle.
                // In an ideal man in hole, this goes continually down.
                // Tally the number of years that descend from the previous year.
                float number_conforming = 0;
                float total_number = 0;
                int previous_count = start_count;
                int current_count = 0;
                for (int year = start_year + 1; year <= middle_year; year++)
                {
                    current_count = citations_by_year[year];
                    if (current_count < previous_count)
                    {
                        number_conforming += 1;
                    }//end if
                    total_number += 1;
                    previous_count = current_count;
                }//end for

                // For the second half of the story, in an ideal man in hole, the story goes continually up.
                // Tally the years that ascend from the previous year.
                previous_count = middle_count;
                current_count = 0;
                for (int year = middle_year + 1; year <= end_year; year++)
                {
                    current_count = citations_by_year[year];
                    if (current_count > previous_count)
                    {
                        number_conforming += 1;
                    }//end if
                    total_number += 1;
                    previous_count = current_count;
                }//end for
                // This is the percent conformity of the story to the expected curve.
                float percent_conformity = number_conforming / total_number;

                // Check how much of the data this story covers.
                float coverage = (end_year - start_year)/(maximum_year - minimum_year);

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
    }//end method DataToEvents

    // Transform a set of events into a story corresponding to a given shape.
    private List<Node> EventsToStory(List<Dictionary<string, Object>> events, string shape)
    {
        List<Node> story = new List<Node>();

        return story;
    }//end method EventsToStory

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
