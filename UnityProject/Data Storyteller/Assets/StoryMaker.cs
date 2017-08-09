using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System;

// A class which creates stories.
public class StoryMaker
{
    public StoryMaker()
    {
    }//end constructor Node

    // Inputs: type of curve to match, set of data to make a story out of.
    public Story MakeStory(string curve_type, DataSet dataset_in)
    {
        double start_neighborhood_size = 0.05f;
        double mid_neighborhood_size = 0.1f;
        double end_neighborhood_size = 0.05f;
        double coverage = 1.0f;
        Story return_story = new Story();
        if (curve_type.Equals("man_in_hole"))
            return_story = MakeStoryManInHole(dataset_in
                                        , start_neighborhood_size
                                        , mid_neighborhood_size
                                        , end_neighborhood_size
                                        , coverage);
        return return_story;
    }//end function MakeStory

    // Create a man-in-a-hole story.
    // Inputs: data_map - a dataset to make a story out of.
    // start_neighborhood_size - the size of the start neighborhood, as a percentage of the input dataset.
    // mid_neighborhood_size - the size of the mid neighborhood, as a percentage of the input dataset.
    // end_neighborhood_size - the size of the end neighborhood, as a percentage of the input dataset.
    // coverage - the percentage of the data that the story should cover.
    private Story MakeStoryManInHole(DataSet dataset_in
                                    , double start_neighborhood_size
                                    , double mid_neighborhood_size
                                    , double end_neighborhood_size
                                    , double coverage)
    {
        // TODO: Place input checkers here.
        // Part 1: Generate candidate stories.
        // Decide how many nodes the coverage represents (round down)
        int coverage_count = (int)Math.Floor(dataset_in.datapoints.Count * coverage) - 1;
        int start_neighborhood_count = (int)Math.Floor(dataset_in.datapoints.Count * start_neighborhood_size);
        int mid_neighborhood_count = (int)Math.Floor(dataset_in.datapoints.Count * mid_neighborhood_size);
        int end_neighborhood_count = (int)Math.Floor(dataset_in.datapoints.Count * end_neighborhood_size);
        
        // Get the absolute start, absolute middle and absolute end indices.
        // These are the points the start, middle, and end neighborhoods will be calculated about.
        // Use the first index as the absolute start.
        int absolute_start_index = 0;
        // The absolute last index is at the end of the story's coverage
        int absolute_end_index = absolute_start_index + coverage_count;
        // The absolute mid index is halfway between the start and the end of the story's coverage.
        int absolute_mid_index = absolute_start_index + coverage_count / 2;

        int start_index = absolute_start_index;
        int mid_index = absolute_mid_index;
        int end_index = absolute_end_index;

        List<Story> candidate_stories = new List<Story>();

        for (int start_offset = 0; start_offset <= start_neighborhood_count; start_offset++)
        {
            start_index = absolute_start_index + start_offset;
            for (int end_offset = 0; end_offset <= end_neighborhood_count; end_offset++)
            {
                end_index = absolute_end_index - end_offset;
                for (int mid_offset = 0; mid_offset <= mid_neighborhood_count / 2; mid_offset++)
                {
                    // Check positive mid neighborhood
                    mid_index = absolute_mid_index + mid_offset;

                    DataPoint start_point = dataset_in.time_sorted_data[start_index];
                    DataPoint mid_point = dataset_in.time_sorted_data[mid_index];
                    DataPoint end_point = dataset_in.time_sorted_data[end_index];
                    if (ValidKnownPointsManInHole(start_point, mid_point, end_point))
                    {
                        // Consider the initial points as a candidate story.
                        StoryCurve new_curve = new StoryCurve(start_point
                                                                , mid_point
                                                                , end_point
                                                                , dataset_in);
                        Story new_story = new Story();
                        new_story.SetCurve(new_curve);
                        candidate_stories.Add(new_story);
                    }//end if

                    // Check negative mid neighborhood
                    mid_index = absolute_mid_index - mid_offset;
                    mid_point = dataset_in.time_sorted_data[mid_index];
                    if (ValidKnownPointsManInHole(start_point, mid_point, end_point))
                    {
                        // Consider the initial points as a candidate story.
                        StoryCurve new_curve = new StoryCurve(start_point
                                                                , mid_point
                                                                , end_point
                                                                , dataset_in);
                        Story new_story = new Story();
                        new_story.SetCurve(new_curve);
                        candidate_stories.Add(new_story);
                    }//end if
                }//end for
            }//end for
        }//end for
        // At this point, we have a list of candidate stories.
        // For each one, calculate the distance of the candidate story's data from the ideal.
        double lowest_difference = double.MaxValue;
        Story best_story = new Story();
        foreach (Story candidate_story in candidate_stories)
        {
            double difference_from_ideal = candidate_story.curve.DistanceFromIdealCurve();
            if (difference_from_ideal < lowest_difference)
            {
                lowest_difference = difference_from_ideal;
                best_story = candidate_story;
            }//end if
        }//end foreach
        // Return the best story.
        return best_story;
    }//end function MakeManInHoleStory

    // Checks whether the given known points are valid for a man-in-hole story.
    private bool ValidKnownPointsManInHole(DataPoint start_point
                                            , DataPoint mid_point
                                            , DataPoint end_point)
    {
        // Chronological ordering check.
        if (mid_point.time <= start_point.time
            || end_point.time <= start_point.time
            || end_point.time <= mid_point.time)
            return false;
        // The mid point needs to be lower than the start and end points
        if (mid_point.value >= start_point.value
            || mid_point.value >= end_point.value)
            return false;
        // The end point needs to be higher than the start point.
        if (end_point.value <= start_point.value)
            return false;
        return true;
    }//end method ValidKnownPointsManInHole
}
