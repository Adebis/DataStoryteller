% Read in CSV data as a table
% bb ca (Calcium at Basin Bay)
%filename = 'C:\Users\Zev\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\study_files_12_17\bb_ca\bb_ca_csv.csv';
% bb cl (Chlorine at Basin Bay)
%filename = 'C:\Users\Zev\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\study_files_12_17\bb_cl\bb_cl_csv.csv';
% an mg (Magnesium at Anthony's Nose)
filename = 'C:\Users\Zev\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\study_files_12_17\an_mg\an_mg_csv.csv';

T = readtable(filename,...
    'Delimiter',',','Format','%{dd-MMMM-yyyy}D%f');
%'Format','%{dd/MMM/yyyy}D%f'
dates = T.Date;
values = T.Mg;
%date_nums = [];

%for n = 1:size(dates)
    % Subtract 1/1/1980, the start year, from each datetime.
    % This gets us a list of durations since 1/1/1980, which
    % we will convert to the number of days since 1/1/1980.
%    date_nums(n,1) = days(dates(n) - datetime(1980,1,1));
%end

date_nums = days(dates - datetime(1980, 1, 1))

%date_nums

x = date_nums;
y = values;
number_of_segments = 7
XI = linspace(min(x), max(x), number_of_segments + 1)
YI = lsq_lut_piecewise(x, y, XI);

x_tick_array = [];
x_tick_labels = [];
years_per_tick = 1
for n = 1:number_of_segments + 1
    x_tick_array(n) = min(x) + 365 * years_per_tick * (n - 1);
    x_tick_label(n) = 1980 + floor(x_tick_array(n) / 365);
end
x_tick_array
x_tick_label
%plot(x,y,'.',XI,YI,'+-')
plot(x,y,'.')
%plot(XI, YI, '+-')
xticks(x_tick_array)
xticklabels(x_tick_label)
ylim([1.8, 2.8])
ylabel('mg/l')
xlabel('Year')
%legend('Measured Data','Piecewise Linear Approximation')
title_text = 'Chemical M at Site T';
%title_text = 'Magnesium Levels at Anthonys Nose';
title(title_text)

