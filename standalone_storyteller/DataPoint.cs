using System.Collections;
using System.Collections.Generic;
using System;

// Represents a single datapoint.
public class DataPoint
{
    // If x is a date, expressed in ticks.
    public double x;
    // y is the actual data value (e.g. secchi depth)
    public double y;
    public string data_measure;
    // To which character does this datapoint belong.
    public string character;
    // The raw data for this datapoint.
    // Keys are header names from the CSV.
    public Dictionary<string, string> raw_data;

    public bool population_success;

    public DataPoint()
    {
        Initialize();
    }//end constructor DataPoint
    public DataPoint(DateTime time_in, double value_in, string char_in)
    {
        Initialize();
        x = time_in.Ticks;
        y = value_in;
        character = char_in;
    }//end constructor DataPoint
    public DataPoint(Dictionary<string, string> data_row_in, string value_header)
    {
        Initialize();
        population_success = PopulateFromLakeGeorgeData(data_row_in, value_header);
    }//end constructor DataPoint
    private void Initialize()
    {        
        x = 0;
        y = 0;
        character = "";
        raw_data = new Dictionary<string, string>();
        population_success = false;
        data_measure = "";
    }//end method Initialize

    // Populate this data point straight from a row of Lake George data.
    // The given value header name will be used to get the y value of this data row.
    public bool PopulateFromLakeGeorgeData(Dictionary<string, string> data_row_in, string value_header)
    {
        bool populate_success = true;
        raw_data = data_row_in;

        string site_name = data_row_in["SITE"];
        double value = 0.0f;
        bool parse_success = double.TryParse(data_row_in[value_header], out value);
        // If we could not parse the secchi depth or the date, do not use this row of data.
        if (!parse_success)
        {
            return false;
        }//end if
        DateTime date = DateTime.Now;
        parse_success = DateTime.TryParse(data_row_in["Date"], out date);
        // If we could not parse the secchi depth or the date, do not use this row of data.
        if (!parse_success)
        {
            return false;
        }//end if

        x = date.Ticks;
        y = value;
        character = site_name;
        data_measure = value_header;

        return populate_success;
    }//end method PopulateFromLakeGeorgeData
}//end class DataPoint
