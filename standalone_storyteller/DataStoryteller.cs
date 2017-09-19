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

            //Dictionary<string, Dictionary<string, double>> grouping_results = FilterData("Zsec", "month");
        }//end try
        catch (Exception e)
        {
            character_names = new List<string>();
        }//end catch
    }//end constructor DataStoryteller

    // Filter data by a variable and by a type of grouping.
    // variable_name must be a valid header.
    public Dictionary<string, Dictionary<string, double>> FilterData(string variable_name, string grouping, bool separate_sites = false)
    {
        // Variables handled:
        // Zsec = Secchi Depth
        // CHLA = Chlorophyll A
        // TFP = Total Soluble Phosphorus
        // Headers:
        // SITE,Z,Date,Zsec,PH,COND,ALK,OP,TFP,TP,CL,NO3,SO4,TN,NH4,SI,Na,Mg,Ca,K,Fe,CHLA,T,DO (mg/l),DO (sat),Zcomp (m)
        // Types of groupings:
        // seasonal = time series group by month
        // yearly = time series group by year
        // location = spatial series group by site
        // separate_sites = if true, do separate series for each site.
        //                  if false, do

        List<Dictionary<string, string>> data_in = new List<Dictionary<string, string>>();
        Dictionary<string, Dictionary<string, double>> grouping_by_site = new Dictionary<string, Dictionary<string, double>>();
        List<string> site_names = new List<string>();
        site_names.Add("French Point");
        site_names.Add("Sabbath Day Point");
        site_names.Add("Smith Bay");
        site_names.Add("Rogers Rock");
        site_names.Add("Northwest Bay");
        site_names.Add("Basin Bay");
        site_names.Add("Dome Island");
        site_names.Add("Tea Island");
        site_names.Add("Green Island");
        site_names.Add("Anthonys Nose");
        site_names.Add("Calves Pen");

        if (!separate_sites)
        {
            data_in = this.all_data;
            grouping_by_site = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<string, double> grouped_data = new Dictionary<string, double>();
            if (grouping == "month")
            {
                grouped_data = FilterByMonth(variable_name, data_in);
            }//end if
            else if (grouping == "year")
            {
                grouped_data = FilterByYear(variable_name, data_in);
            }//end else if
            grouping_by_site.Add("ALL", grouped_data);
        }//end if
        else if (separate_sites || grouping == "site")
        {
            // Group data by site name.
            grouping_by_site = new Dictionary<string, Dictionary<string, double>>();
            foreach (string site_name in site_names)
            {
                data_in = new List<Dictionary<string, string>>();
                // Get all the data rows for this site.
                foreach (Dictionary<string, string> data_row in this.all_data)
                {
                    if (data_row["SITE"] == site_name)
                    {
                        data_in.Add(data_row);
                    }//end if
                }//end foreach
                // Get averages for this site according to the method of grouping.
                Dictionary<string, double> grouped_data = new Dictionary<string, double>();
                if (grouping == "month")
                {
                    grouped_data = FilterByMonth(variable_name, data_in);
                }//end if
                else if (grouping == "year")
                {
                    grouped_data = FilterByYear(variable_name, data_in);
                }//end else if
                else if (grouping == "site")
                {
                    // Sum the values for the given variable name for this site.
                    double sum = 0.0f;
                    double count = 0.0f;
                    foreach (Dictionary<string, string> data_row in this.all_data)
                    {
                        if (data_row["SITE"] != site_name)
                            continue;
                        // First, try to parse the appropriate variable into a number.
                        string variable_string = data_row[variable_name];
                        double variable_value = 0.0f;
                        bool parse_success = double.TryParse(variable_string, out variable_value);
                        if (!parse_success)
                            continue;
                        sum += variable_value;
                        count += 1;
                    }//end foreach
                    // Now that we have a sum, average it.
                    double average = sum / count;
                    grouped_data.Add("N/A", average);
                }//end else if
                grouping_by_site.Add(site_name, grouped_data);
            }//end foreach
        }//end else
        return grouping_by_site;
    }//end method FilterData

    // Get the average value for the given variable name for each month.
    private Dictionary<string, double> FilterByMonth(string variable_name, List<Dictionary<string, string>> data_in)
    {
        Dictionary<string, List<double>> entries_by_month = new Dictionary<string, List<double>>();
        Dictionary<string, double> average_by_month = new Dictionary<string, double>();

        List<string> month_strings = new List<string>();
        month_strings.Add("Jan");
        month_strings.Add("Feb");
        month_strings.Add("Mar");
        month_strings.Add("Apr");
        month_strings.Add("May");
        month_strings.Add("Jun");
        month_strings.Add("Jul");
        month_strings.Add("Aug");
        month_strings.Add("Sep");
        month_strings.Add("Oct");
        month_strings.Add("Nov");
        month_strings.Add("Dec");
        foreach (string month_string in month_strings)
        {
            entries_by_month.Add(month_string, new List<double>());
            average_by_month.Add(month_string, 0.0f);
        }//end foreach

        foreach (Dictionary<string, string> data_row in data_in)
        {
            // First, try to parse the appropriate variable into a number.
            string variable_string = data_row[variable_name];
            double variable_value = 0.0f;
            bool parse_success = double.TryParse(variable_string, out variable_value);
            if (!parse_success)
                continue;

            // Check the date.
            string date_string = data_row["Date"];
            // Get the month.
            for (int i = 0; i < month_strings.Count; i++)
            {
                if (date_string.Contains(month_strings[i]))
                {
                    entries_by_month[month_strings[i]].Add(variable_value);
                }//end if
            }//end for
        }//end foreach

        foreach (KeyValuePair<string, List<double>> month_entries in entries_by_month)
        {
            List<double> entries = month_entries.Value;
            string month = month_entries.Key;
            double sum = 0.0f;
            foreach (double value in entries)
                sum += value;
            double average = sum / entries.Count;
            average_by_month[month] = average;
        }//end foreach

        return average_by_month;
    }//end method FilterByMonth

    private Dictionary<string, double> FilterByYear(string variable_name, List<Dictionary<string, string>> data_in)
    {
        Dictionary<string, List<double>> entries_by_year = new Dictionary<string, List<double>>();
        Dictionary<string, double> average_by_year = new Dictionary<string, double>();

        foreach (Dictionary<string, string> data_row in data_in)
        {
            // First, try to parse the appropriate variable into a number.
            string variable_string = data_row[variable_name];
            double variable_value = 0.0f;
            bool parse_success = double.TryParse(variable_string, out variable_value);
            if (!parse_success)
                continue;

            // Check the date.
            string date_string = data_row["Date"];
            // Get the year.
            string year_string = date_string.Substring(date_string.Length - 4);
            if (!entries_by_year.ContainsKey(year_string))
            {
                entries_by_year.Add(year_string, new List<double>());
            }//end if
            entries_by_year[year_string].Add(variable_value);
        }//end foreach

        foreach (KeyValuePair<string, List<double>> year_entries in entries_by_year)
        {
            List<double> entries = year_entries.Value;
            string year = year_entries.Key;
            double sum = 0.0f;
            foreach (double value in entries)
                sum += value;
            double average = sum / entries.Count;
            if (!average_by_year.ContainsKey(year))
                average_by_year.Add(year, average);
        }//end foreach

        return average_by_year;
    }//end method FilterByYear

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