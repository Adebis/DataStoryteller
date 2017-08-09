using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Represents a polynomial equation.
// Defined by a set of coefficients and their polynomial degree.
public class Equation
{
    // Key: degree of the variable.
    // Value: coefficient for that degree.
    public SortedDictionary<int, double> coefficients;

    public Equation()
    {
        coefficients = new SortedDictionary<int, double>();
    }//end constructor Node

    // Add the following coefficient for the following degree.
    public void AddCoefficient(double coefficient, int degree)
    {
        // If the degree already exists in our list of coefficients,
        // add the given coefficient to the existing one.
        if (coefficients.ContainsKey(degree))
            coefficients[degree] += coefficient;
        else
            coefficients.Add(degree, coefficient);
    }//end method AddCoefficient
}//end class Equation