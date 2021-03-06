﻿using System.Collections;
using System.Collections.Generic;
using System;

// Represents a single datapoint.
public class DataPoint
{
    // If x is a date, expressed in number of days since 1980/1/1
    public double x;
    // y is the actual data value (e.g. secchi depth)
    public double y;

    // A non-numerical description
    //public string description;
    // A numerical description
    //public string numerical_description;

    // A set of descriptors involving this point.

    public DataPoint()
    {
        this.x = 0;
        this.y = 0;
    }//end constructor DataPoint
    public DataPoint(double x_in, double y_in)
    {
        this.x = x_in;
        this.y = y_in;
    }//end constructor DataPoint

    private void Initialize()
    {
    }//end method Initialize
}//end class DataPoint
