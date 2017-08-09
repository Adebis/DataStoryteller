using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// A collection of datapoints.
public class DataSet
{
    public List<DataPoint> datapoints;
    // The set of datapoints sorted by increasing order of time.
    public List<DataPoint> time_sorted_data;

    public DataSet()
    {
        datapoints = new List<DataPoint>();
        time_sorted_data = new List<DataPoint>();
    }//end constructor DataPoint

    public void AddDatapoint(DataPoint point_to_add)
    {
        datapoints.Add(point_to_add);
        for (int i = 0; i <= time_sorted_data.Count; i++)
        {
            // If we are past the end of the time sorted data,
            // then the point we wish to add occurs after all other points.
            if (i >= time_sorted_data.Count)
            {
                time_sorted_data.Add(point_to_add);
                break;
            }//end if
            // If the point we wish to add occurs at an earlier time than
            // the point currently at this index, insert the point we wish
            // to add at this index.
            if (point_to_add.time < time_sorted_data[i].time)
            {
                time_sorted_data.Insert(i, point_to_add);
                break;
            }//end if
        }//end for
    }//end method AddDatapoint

    // Merge the given dataset into this dataset
    public void MergeDataset(DataSet dataset_in)
    {
        foreach (DataPoint point_to_merge in dataset_in.datapoints)
            AddDatapoint(point_to_merge);
    }//end method MergeDataset

    public List<DataPoint> GetTimeSortedDataRange(DataPoint start_point, DataPoint end_point)
    {
        List<DataPoint> data_range = new List<DataPoint>();
        // Includes the start and end points.
        for (int i = time_sorted_data.IndexOf(start_point); i <= time_sorted_data.IndexOf(end_point); i++)
        {
            data_range.Add(time_sorted_data[i]);
        }//end for
        return data_range;
    }//end method GetTimeSortedDataRange
} //end class DataSet
