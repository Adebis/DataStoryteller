using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Represents a single datapoint.
public class DataPoint
{
    public DateTime time;
    public double value;
    // To which character does this datapoint belong.
    public string character;
    // The raw data for this datapoint.
    // Keys are header names from the CSV.
    public Dictionary<string, string> raw_data;

    public DataPoint()
    {
        Initialize();
    }//end constructor DataPoint
    public DataPoint(DateTime time_in, double value_in, string char_in)
    {
        Initialize();
        time = time_in;
        value = value_in;
        character = char_in;
    }//end constructor DataPoint
    private void Initialize()
    {        
        time = DateTime.MinValue;
        value = 0.0f;
        character = "";
        raw_data = new Dictionary<string, string>();
    }//end method Initialize

    public bool PopulateFromLakeGeorgeData(Dictionary<string, string> data_row_in)
    {
        bool populate_success = true;
        raw_data = data_row_in;

        string site_name = data_row_in["SITE"];
        double secchi_depth = 0.0f;
        bool parse_success = double.TryParse(data_row_in["Zsec"], out secchi_depth);
        DateTime date = DateTime.Now;
        parse_success = DateTime.TryParse(data_row_in["Date"], out date);
        // If we could not parse the secchi depth or the date, do not use this row of data.
        if (!parse_success)
        {
            return false;
        }//end if

        time = date;
        value = secchi_depth;
        character = site_name;

        return populate_success;
    }//end method PopulateFromLakeGeorgeData
}//end class DataPoint
