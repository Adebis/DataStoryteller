using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
//using MathNet.Numerics;
//using MathNet.Numerics;
//using MathNet.Numerics;

// Represents a story curve.
// Right now, only represents man-in-hole. Will abstract later.
public class StoryCurve
{
    // The equation describing the curve for this story.
    // Consists of coefficients at indexed by their degree.
    public Equation ideal_curve;
    // A set of initial known points from which to extrapolate a curve.
    // E.g., for man-in-hole, the known points are
    // the beginning, the middle, and the end.
    public Dictionary<string, DataPoint> initial_points;
    // The full dataset.
    public DataSet dataset;
    // The name of this story.
    public string name;
    public double total_distance_from_ideal;
    // The series of points that this curve represents.
    public List<DataPoint> points_represented;

    public StoryCurve()
    {
        Initialize();
    }//end constructor StoryCurve
    public StoryCurve(DataPoint start_point, DataPoint mid_point, DataPoint end_point, DataSet dataset_in)
    {
        Initialize();
        SetInitialPoints(start_point, mid_point, end_point);
        SetDataset(dataset_in);
        points_represented = dataset.GetTimeSortedDataRange(start_point, end_point);
    }//end constructor StoryCurve
    private void Initialize()
    {
        name = "";
        //ideal_curve = new Equation();
        initial_points = new Dictionary<string, DataPoint>();
        dataset = new DataSet();
        total_distance_from_ideal = 0.0f;
        points_represented = new List<DataPoint>();
    }//end method Initialize

    public void SetInitialPoints(DataPoint start_point, DataPoint mid_point, DataPoint end_point)
    {
        initial_points.Add("start", start_point);
        initial_points.Add("mid", mid_point);
        initial_points.Add("end", end_point);
    }//end method SetInitialPoints
    public void SetDataset(DataSet dataset_in)
    {
        dataset = dataset_in;
    }//end method SetDataset

    // Calculate the ideal curve equation for a man-in-hole story with the given initial points.
    // For man-in-hole, accomplish this by interpolating a polynomial between the points.
    // Then, compare the interpolated points in the ideal equation to the real data.
    // Return the total distance from ideal.
    public double DistanceFromIdealCurve()
    {
        // Use initial points in initial points set.
        DataPoint start_point = initial_points["start"];
        DataPoint mid_point = initial_points["mid"];
        DataPoint end_point = initial_points["end"];
        // x values are the indices of the given points in the time-ordered data.
        double[] x_input = new double[3];
        x_input[0] = dataset.time_sorted_data.IndexOf(start_point);
        x_input[1] = dataset.time_sorted_data.IndexOf(mid_point);
        x_input[2] = dataset.time_sorted_data.IndexOf(end_point);
        // y values are the actual data point values themselves.
        double[] y_input = new double[3];
        y_input[0] = start_point.value;
        y_input[1] = mid_point.value;
        y_input[2] = end_point.value;
        // Feed these in as sample points in a neville polynomial interpolation
        NevillePolynomialInterpolation interpolator = new NevillePolynomialInterpolation(x_input, y_input);

        // Now that we have a polynomial interpreter, grab all the data points between the start and end points (inclusive).
        // Note that these will still be in the correct time order.
        List<DataPoint> data_range_to_check = dataset.GetTimeSortedDataRange(start_point, end_point);

        double total_difference = 0;
        foreach (DataPoint point_to_check in data_range_to_check)
        {
            // Get the index for this point in the dataset's time-ordered data.
            int t = dataset.time_sorted_data.IndexOf(point_to_check);
            // Get the interpolated value for it.
            double interpolated_value = interpolator.Interpolate(t);
            // Get the absolute difference between it and the real value.
            double difference = interpolated_value - point_to_check.value;
            if (difference < 0)
                difference *= -1.0f;
            total_difference += difference;
        }//end foreach
        total_distance_from_ideal = total_difference;
        return total_difference;
    }//end method DistanceFromIdealCurve

}
