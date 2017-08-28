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

    List<Dictionary<string, string>> all_data;
    List<string> all_headers;

    //public Dictionary<string
    public List<Node> story_nodes;

    public string unity_log;

    public DataStoryteller()
    {
        try
        {
            unity_log = "";
            // Characters are going to be 
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

            all_data = csv_data;
            all_headers = csv_headers;
        }//end try
        catch (Exception e)
        {
            character_names = new List<string>();
        }//end catch
    }//end constructor DataStoryteller

    // Filter data by a data variable and by a type of variation.
    public void filter_data(string variable_name, string variation, bool separate_sites = false)
    {
        // Variables handled:
        // Zsec = Secchi Depth
        // CHLA = Chlorophyll A
        // TFP = Total Soluble Phosphorus
        // Headers:
        // SITE,Z,Date,Zsec,PH,COND,ALK,OP,TFP,TP,CL,NO3,SO4,TN,NH4,SI,Na,Mg,Ca,K,Fe,CHLA,T,DO (mg/l),DO (sat),Zcomp (m)
        // Types of variation:
        // seasonal = time series group by month
        // yearly = time series group by year
        // location = spatial series group by site
        // separate_sites = if true, do separate series for each site.
        //                  if false, do

        foreach (Dictionary<string, string> data_row in this.all_data)
        {
            
        }//end foreach
    }//end method filter_data

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
