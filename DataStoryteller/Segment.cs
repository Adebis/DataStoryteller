using System;
using System.Collections;
using System.Collections.Generic;

public class Segment
{
    private int id;
    private DataPoint start_point;
    private DataPoint end_point;

    // Info variables
    

    public Segment()
    {
        this.id = 0;
        this.start_point = new DataPoint();
        this.end_point = new DataPoint();
    }//end constructor Segment
    public Segment(int id_in, double start_x, double start_y, double end_x, double end_y)
    {
        this.id = id_in;
        this.start_point = new DataPoint(start_x, start_y);
        this.end_point = new DataPoint(end_x, end_y);
    }//end constructor Segment

    private void Initialize()
    {
        
    }//end method Initialize
}//end class Segment