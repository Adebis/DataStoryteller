// Represents a piece of information from the graph.
using System.Collections.Generic;

// A class representing a critical point in the graph.
public class CriticalPoint : GraphInfo
{
    public DataPoint data_point;

    public DataPoint normal_point;

    public CriticalPoint()
    {
        data_point = new DataPoint();
        normal_point = new DataPoint();
        name = "";
    }//end constructor CriticalPoint
    public CriticalPoint(DataPoint data_point_in)
    {
        data_point = data_point_in;
        normal_point = new DataPoint();
        name = "";
    }//end constructor CriticalPoint
}//end class Abnormality