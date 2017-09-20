using System.Collections;
using System.Collections.Generic;
using System;

// A collection of datapoints.
public class DataSet
{
    public List<DataPoint> datapoints;

    public DataSet()
    {
        datapoints = new List<DataPoint>();
    }//end constructor DataPoint

    public void AddDatapoint(DataPoint point_to_add)
    {
        datapoints.Add(point_to_add);
    }//end method AddDatapoint

    // Merge the given dataset into this dataset
    public void MergeDataset(DataSet dataset_in)
    {
        foreach (DataPoint point_to_merge in dataset_in.datapoints)
            AddDatapoint(point_to_merge);
    }//end method MergeDataset
} //end class DataSet
