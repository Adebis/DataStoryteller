% Read in CSV data as a table
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\A10_CL_csv.csv';
% NOTE: NO PLA FOR A10 CL
% BB CL (Basin Bay Chlorine)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\BB_CL_csv.csv';
% D COND (Dome Island Conductivity)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\D_COND_csv.csv';
% T Ca (Tea Island Calcium)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\T_Ca_csv.csv';
% SD K (Sabbath Day Point Potassium)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\SD_K_csv.csv';
% R SI (Rogers Rock Silicon)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\R_SI_csv.csv';
% F SO4 (French Point SO4)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\F_SO4_csv.csv';
% AN Mg (Anthonys Nose Magnesium)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\AN_Mg_csv.csv';
% S DOsat (Smith Bay Dissolved Oxygen (saturated))
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\S_DOsat_csv.csv';
% A10 Na (Northwest Bay Sodium)
%filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\A10_Na_csv.csv';
% S CHLA (Smith Bay Chlorophyll A)
% filename = 'Z:\Dropbox\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\S_CHLA_csv.csv';

% ca bb (Calcium at Basin Bay)
filename = 'C:\Users\Zev\Dropbox\Research\DataStoryteller\study_matching_2017\matlab\study_files_12_17\bb_ca\bb_ca_csv.csv';
T = readtable(filename,...
    'Delimiter',',','Format','%{dd-MMMM-yyyy}D%f');
%'Format','%{dd/MMM/yyyy}D%f'
dates = T.Date;
values = T.Ca;
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
number_of_segments = 14
XI = linspace(min(x), max(x), number_of_segments + 1)
YI = lsq_lut_piecewise(x, y, XI);

x_tick_array = [];
x_tick_labels = [];
years_per_tick = 4
for n = 1:number_of_segments + 1
    x_tick_array(n) = min(x) + 365 * years_per_tick * (n - 1);
    x_tick_label(n) = 1980 + floor(x_tick_array(n) / 365);
end
x_tick_array
x_tick_label
plot(x,y,'.',XI,YI,'+-')
%plot(x,y,'.')
%plot(XI, YI, '+-')
xticks(x_tick_array)
xticklabels(x_tick_label)
ylim([8, 14])
ylabel('mg/l')
xlabel('Year')
legend('Measured Data','Piecewise Linear Approximation')
%title_text = 'Chemical C at Site B';
title_text = 'Calcium Levels at Basin Bay';
title(title_text)

