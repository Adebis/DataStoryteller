
function Plot_A10(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{6126:6508,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{6126:6508,k};
    scatter(date_lable_t,data_t);
    datetick('x','dd-mm-yyyy','keepticks')
    grid on
    title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('A10');
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('A10_site_Graph_for_',columnname{1,k});
    print(printout,'-dpng');
end