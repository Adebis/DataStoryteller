using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Represents a story. 
// A story is defined as a set of events (the content) as well as
// the presentation of the events.

// Each story is created using some underlying dramatic curve, defined by a polynomial equation.
// Curves are defined by dramatic value (y) at each percentage of the story (t).
public class Story
{
    // The equation describing the curve for this story.
    // Consists of coefficients at indexed by their degree.
    //public Equation equation;
    // This story's curve.
    public StoryCurve curve;

    // The name of this story.
    public string name;

    public Story()
    {
        name = "";
        //equation = new Equation();
        curve = new StoryCurve();
    }//end constructor Node

    public void SetCurve(StoryCurve curve_in)
    {
        curve = curve_in;
    }//end method SetInitialPoints
}
