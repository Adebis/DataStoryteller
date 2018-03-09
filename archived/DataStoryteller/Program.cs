﻿using System;
using System.Collections.Generic;

namespace DataStoryteller
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            List<string> data_file_names = new List<string>();
            List<string> info_file_names = new List<string>();
            // Calcium at Basin Bay (a.k.a. Chemical C at Site B), w. Index 0
            data_file_names.Add("bb_ca/bb_ca_segments.csv");
            info_file_names.Add("bb_ca/bb_ca_meta.json");
            // Chlorine at Basin Bay (a.k.a. Chemical H at Site C), w. Index 1
            data_file_names.Add("bb_cl/bb_cl_segments.csv");
            info_file_names.Add("bb_cl/bb_cl_meta.json");
            // Magnesium at Anthony's Nose (a.k.a. Chemical M at Site T), v. Index 2
            data_file_names.Add("an_mg/an_mg_segments.csv");
            info_file_names.Add("an_mg/an_mg_meta.json");
            // SO4 at French Point (a.k.a. Chemical S at Site F), line. Index 3
            data_file_names.Add("f_so4/f_so4_segments.csv");
            info_file_names.Add("f_so4/f_so4_meta.json");
            // Sodium at Northwest Bay (a.k.a. Chemical A at Site N), line. Index 4
            data_file_names.Add("a10_na/a10_na_segments.csv");
            info_file_names.Add("a10_na/a10_na_meta.json");
            // Silicon at Roger's Rock (a.k.a. Chemical L at Site G), w. Index 5
            data_file_names.Add("r_si/r_si_segments.csv");
            info_file_names.Add("r_si/r_si_meta.json");
            // Conductivity at Dome Island (a.k.a. Chemical O at Site I), line, w, and v. Index 6
            data_file_names.Add("d_cond/d_cond_segments.csv");
            info_file_names.Add("d_cond/d_cond_meta.json");
            // Calcium at Tea Island (a.k.a. Chemical E at Site K), line. Index 7
            data_file_names.Add("t_ca/t_ca_segments.csv");
            info_file_names.Add("t_ca/t_ca_meta.json");

            int data_index = 0;
            int description_type = 0;
            NarrativeGenerator generator = new NarrativeGenerator();
            string data_file_name = data_file_names[data_index];
            string info_file_name = info_file_names[data_index];
            double starting_year = 1980;

            for (int i = 0; i < 3; i++)
                generator.GenerateNarrative(data_file_name, info_file_name, starting_year, i);
        }
    }
}