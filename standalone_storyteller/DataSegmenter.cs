using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class DataSegmenter {

    public DataSegmenter()
    {

    }//end constructor DataSegmenter

    // Creates segments using a set of pre-defined separation points (by datapoint index in the dataset)
    public List<Segment> SegmentByHand(List<DataPoint> dataset_in, List<int> separation_point_indices)
    {
        Console.WriteLine("Segmenting by hand");
        List<Segment> current_segmentation = new List<Segment>();

        int start_index = 0;
        int end_index = 0;
        List<DataPoint> current_segment_set = new List<DataPoint>();
        for (int i = 0; i < separation_point_indices.Count; i++)
        {
            end_index = separation_point_indices[i];
            // NOTE: GetRange creates a shallow copy.
            current_segment_set = dataset_in.GetRange(start_index, end_index - start_index + 1);
            Segment new_segment = new Segment(start_index, end_index, current_segment_set);
            current_segmentation.Add(new_segment);
            start_index = end_index;
        }//end for

        return current_segmentation;
    }//end method SegmentByHand

    public List<Segment> SegmentByPLA(List<DataPoint> dataset_in, int number_of_segments)
    {
        Console.WriteLine("Segmenting by PLA");
        int k = number_of_segments;
        // The total segmentation is a sequence of individual segments.
        List<Segment> current_segmentation = new List<Segment>();
        List<Segment> base_segmentation = new List<Segment>();

        // Create an initial set of k segments.
        int initial_segment_size = dataset_in.Count / k;
        List<DataPoint> current_segment_set = new List<DataPoint>();
        int start_index = 0;
        int end_index = initial_segment_size - 1;
        for (int i = 0; i < k; i++)
        {
            // NOTE: GetRange creates a shallow copy.
            current_segment_set = dataset_in.GetRange(start_index, end_index - start_index + 1);
            Segment new_segment = new Segment(start_index, end_index, current_segment_set);
            base_segmentation.Add(new_segment);

            start_index = end_index;
            end_index = start_index + initial_segment_size - 1;
            // Have the final end index just go to the end of the dataset.
            if (i == k - 1)
                end_index = dataset_in.Count;
        }//end for

        int first_segment_index = 0;
        current_segmentation = base_segmentation;
        // We want to minimize this score.
        double current_segmentation_score = ScoreSegmentation(current_segmentation);
        // Main greedy loop
        bool last_skipped = false;
        int consecutive_skip_count = 0;
        int consecutive_skip_threshold = k * 2;
        for (int i = 0; i < 10000; i++)
        {
            List<Segment> potential_segmentation = new List<Segment>();
            potential_segmentation.AddRange(current_segmentation);

            // Picks a segment, then merges it with the segment after it.
            Segment first_segment = current_segmentation[first_segment_index];
            int second_segment_index = first_segment_index + 1;
            Segment second_segment = current_segmentation[second_segment_index];
            
            // Merge the two and get the resulting segment.
            Segment merged_segment = MergeSegments(first_segment, second_segment);

            // Remove both first and second segments from the potential segmentation list.
            // Replace them with the merged segment.
            potential_segmentation.Insert(first_segment_index, merged_segment);
            potential_segmentation.RemoveRange(first_segment_index + 1, 2);
            // Update indices for the next loop.
            if (first_segment_index == current_segmentation.Count - 2)
            {
                first_segment_index = 0;
            }//end if
            else
            {
                first_segment_index += 1;
            }//end if

            // Go through each segmentation and find the optimal point of splitting, as well as the
            // cost change for splitting at that point. We want cost to always go down. As such, savings
            // should be maximized.
            int best_segment_index = 0;
            List<Segment> best_split = new List<Segment>();
            double best_savings = double.MinValue;
            for (int j = 0; j < potential_segmentation.Count - 1; j++)
            {
                Segment segment_to_check = potential_segmentation[j];
                List<Segment> optimal_split = segment_to_check.GetOptimalSplit();
                // If the optimal split is of size 0, then the segment may be made of only 2 points.
                // Skip this one.
                if (optimal_split.Count <= 0)
                {
                    //Console.WriteLine("No optimal split");
                    continue;
                }//end if
                // Calculate the savings for performing this split. We want to maximize savings.
                double current_savings = segment_to_check.cost - (optimal_split[0].cost + optimal_split[1].cost);
                //Console.WriteLine ("Optimal split: " + optimal_split[0].ToString() +" and " + optimal_split[1].ToString() + ". Savings: " + current_savings);
                if (current_savings > best_savings)
                {
                    best_segment_index = j;
                    best_split = optimal_split;
                    best_savings = current_savings;
                }//end if
            }//end for

            // If the savings are 0 or negative, then the cost did not go down by any valid move.
            // Go on to the next iteration.
            if (best_savings <= 0)
            {
                if (last_skipped)
                    consecutive_skip_count += 1;
                else
                    consecutive_skip_count = 1;
                if (consecutive_skip_count == consecutive_skip_threshold)
                {
                    Console.WriteLine(consecutive_skip_threshold.ToString() + " consecutive skips, stopping criteria met at i = " + i.ToString());
                    break;
                }//end if
                last_skipped = true;
                continue;
            }//end if
            last_skipped = false;

            // Replace the chosen segment with its split.
            // Since insertion pushes things "forward," start with the second segment.
            potential_segmentation.Insert(best_segment_index, best_split[1]);
            potential_segmentation.Insert(best_segment_index, best_split[0]);
            // Now we have to look 2 forwards to get the actual segment we want to remove.
            potential_segmentation.RemoveAt(best_segment_index + 2);

            // Now, replace the current segmentation with the potential segmentation.
            current_segmentation = potential_segmentation;
        }//end for

        return current_segmentation;
    }//end method SegmentByPLA

    // Helper function. Merges two segments. First segment given is assumed
    // to be before the second segment given
    private Segment MergeSegments(Segment first_segment, Segment second_segment)
    {
        List<DataPoint> new_segment_set = new List<DataPoint>();
        new_segment_set.AddRange(first_segment.segment_set);
        // When merging, don't double-add the end of the first segment and the beginning of the second.
        new_segment_set.AddRange(second_segment.segment_set.GetRange(1, second_segment.segment_set.Count - 1));
        int new_start_index = first_segment.start_index;
        int new_end_index = second_segment.end_index;
        Segment merged_segment = new Segment(new_start_index, new_end_index, new_segment_set);

        return merged_segment;
    }//end method MergeSegments
    // Helper function. Sums the score of each segment of a given segmentation.
    private double ScoreSegmentation(List<Segment> segmentation_in)
    {
        double score = 0;
        foreach (Segment segment in segmentation_in)
            score += segment.cost;
        return score;
    }//end method ScoreSegmentation

}//end class DataSegmenter