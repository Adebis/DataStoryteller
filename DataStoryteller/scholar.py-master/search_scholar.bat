FOR /L %%A IN (1937,1,2017) DO py -3 scholar.py -p "machine learning" --after %%A --before %%A -c 100 >> %%A.txt

REM py -3 scholar.py -p "machine learning" --after 1940 --before 1940 -c 100 >> scholar_out_1940.txt