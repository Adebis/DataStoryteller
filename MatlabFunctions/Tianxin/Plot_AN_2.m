%for test
function Plot_AN_2(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{1:48,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{1:48,k};
    scatter(date_lable_t,data_t);
    datetick('x','dd-mm-yyyy','keepticks');
  
    
    %findpeaks(data_t,date_lable_t,'MinPeakDistance',6)
    grid on
    title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('AN');
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('AN_site_Graph_for_',columnname{1,k});
    print(printout,'-dpng');
end