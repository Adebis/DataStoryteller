// Represents a piece of information from the graph.
using System.Collections.Generic;

// A class representing an abnormality in the graph.
public class Abnormality : GraphInfo
{
    public CriticalPoint critical_point;
    public Abnormality()
    {
        critical_point = new CriticalPoint();
    }//end constructor

    public void SetCriticalPoint(CriticalPoint point_in)
    {
        critical_point = point_in;
    }//end method SetCriticalPoint
    public CriticalPoint GetCriticalPoint()
    {
        return critical_point;
    }//end method GetCriticalPoint
}//end class Abnormality