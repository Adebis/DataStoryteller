using System;
using System.Collections;
using System.Collections.Generic;

public class Segment{
    
    public int start_index;
    public int end_index;

    public DataPoint point_a;
    public DataPoint point_b;

    // Average starting and ending values
    public double start_average;
    public double end_average;
    public double slope;
    public double time_span;

    public List<DataPoint> segment_set;
    // A set of tags to describe this segment.
    public List<string> tags;

    // Mins and maxes
    public double min_value;
    public DataPoint min_point;
    public double max_value;
    public DataPoint max_point;

    public double cost;

    public Segment()
    {

    }//end constructor Segment
    public Segment(int start_index_in, int end_index_in, List<DataPoint> set_in)
    {
        start_index = start_index_in;
        end_index = end_index_in;
        segment_set = set_in;
        tags = new List<string>();
        point_a = segment_set[0];
        point_b = segment_set[segment_set.Count - 1];
        cost = 0;
        slope = 0;
        time_span = 0;
        //CalculateAverages();
        CalculateSlope();
        CalculateCost();
        CalculateMinAndMax();
    }//end constructor Segment

    // Find the optimal 2-split for this segment and return it.
    public List<Segment> GetOptimalSplit()
    {
        List<Segment> optimal_split = new List<Segment>();
        double optimal_split_cost = double.MaxValue;
        List<DataPoint> first_segment_set = new List<DataPoint>();
        List<DataPoint> second_segment_set = new List<DataPoint>();
        Segment first_segment = null;
        Segment second_segment = null;
        // Don't split at the beginning or end. We don't want any line segments becoming single points.
        for (int i = 1; i < segment_set.Count - 2; i++)
        {
            // When we split at a point, we take everything including the point itself
            // and before as the first segment, and everything after the point itself
            // as the second segment.
            first_segment_set = segment_set.GetRange(0, i + 1);
            second_segment_set = segment_set.GetRange(i, segment_set.Count - first_segment_set.Count);
            // Skip if either segment set only has 1 datapoint.
            if (first_segment_set.Count <= 1 || second_segment_set.Count <= 1)
                continue;

            int first_start_index = start_index;
            int first_end_index = start_index + i;
            int second_start_index = start_index + i;
            int second_end_index = end_index;
            // Create segments of this split to calculate their costs.
            first_segment = new Segment(first_start_index, first_end_index, first_segment_set);
            second_segment = new Segment(second_start_index, second_end_index, second_segment_set);
            // Check for a new lowest cost.
            double current_split_cost = first_segment.cost + second_segment.cost;
            if (current_split_cost < optimal_split_cost)
            {
                optimal_split_cost = current_split_cost;
                optimal_split = new List<Segment>();
                optimal_split.Add(first_segment);
                optimal_split.Add(second_segment);
            }//end if
        }//end foreach

        return optimal_split;
    }//end method GetOptimalSplit

    // Calculate the average start and end values
    // by averaging over some window of values.
    /*public void CalculateAverages()
    {
        int window_size = 10;
        // Calculate start average
        int start_window_start_index = 0;
        int start_window_end_index = 0;
        if (start_index == 0 || start_index < window_size / 2)
        {
            start_window_start_index = 0;
            start_window_end_index = start_window_start_index + window_size;
        }//end if
        else
        {
            start_window_start_index = start_index - window_size / 2;
            start_window_end_index = start_index + window_size / 2;
        }//end else if
        double temp_sum = 0;
        for (int i = 0; i < window_size; i++)
        {
            temp_sum += segment_set[i].y;
        }//end for
        start_average = temp_sum / window_size;

        // Calculate end average
        int end_window_start_index = 0;
        int end_window_end_index = 0;
        if (end_index = )
    }//end method CalculateAverages*/

    public double CalculateSlope()
    {
        double length_in_days = Math.Floor(TimeSpan.FromTicks((long)(point_b.x - point_a.x)).TotalDays);
        time_span = length_in_days;
        double value_change = point_b.y - point_a.y;

        slope = value_change / length_in_days;
        return slope;
    }//end method CalculateSlope

    public void CalculateMinAndMax()
    {
        min_value = double.MaxValue;
        min_point = new DataPoint();
        max_value = double.MinValue;
        max_point = new DataPoint();

        foreach (DataPoint d in segment_set)
        {
            if (d.y < min_value)
            {
                min_value = d.y;
                min_point = d;
            }//end if
            if (d.y > max_value)
            {
                max_value = d.y;
                max_point = d;
            }//end if
        }//end foreach
    }//end method CalculateMinimum

    // Calculate the cost of this segment.
    public double CalculateCost()
    {
        double x_sum = 0.0f;
        double y_sum = 0.0f;

        // Create a list of the x and y values for the data.
        List<double> x_values = new List<double>();
        List<double> y_values = new List<double>();

        foreach (DataPoint d_point in segment_set)
        {
            x_values.Add(d_point.x);
            y_values.Add(d_point.y);
        }//end foreach

        //x_sum = SumOfVariance(x_values);
        y_sum = SumOfVariance(y_values);

        cost = x_sum + y_sum;

        return cost;
    }//end method CalculateCost
    // Helper function.
    // Calculates a sum of the variance for a set of values
    private double SumOfVariance(List<double> values_in)
    {
        double n = values_in.Count;
        // First part, sum each value squared and divide by
        // the number of values.
        double part_1 = 0;
        foreach (double value in values_in)
            part_1 += (value * value);
        part_1 /= n;

        // Second part, sum each value and divide by
        // the number of values, then square the sum.
        double part_2 = 0;
        foreach (double value in values_in)
            part_2 += value;
        part_2 /= n;
        part_2 = part_2 * part_2;

        double result = part_1 - part_2;

        return result;
    }//end method SumOfVariance

    public String ToString()
    {
        String return_string = "";
        DateTime start_date = new DateTime((long)point_a.x);
        DateTime end_date = new DateTime((long)point_b.x);
        return_string = "Start [" + start_index.ToString() + "]: " + point_a.data_measure + "=" + point_a.y.ToString() + " at " + start_date.ToString() + " ==> ";
        return_string += "End [" + end_index.ToString() + "]: " + point_b.data_measure + "=" + point_b.y.ToString() + " at " + end_date.ToString();

        return return_string;
    }//end method ToString
}//end class Segment