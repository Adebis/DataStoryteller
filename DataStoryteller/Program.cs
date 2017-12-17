using System;

namespace DataStoryteller
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            NarrativeGenerator generator = new NarrativeGenerator();
            string data_file_name = "ca_bb_segments.csv";
            string info_file_name = "ca_bb_meta.json";
            double starting_year = 1992;
            generator.GenerateNarrative(data_file_name, info_file_name, starting_year);
        }
    }
}
