
function Plot_S(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{1794:2677,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{1794:2677,k};
    scatter(date_lable_t,data_t);
    datetick('x','dd-mm-yyyy','keepticks')
    grid on
    title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('S');
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('S_site_Graph_for_',columnname{1,k});
    print(printout,'-dpng');
    [date_lable_t,data_t] = prepareCurveData(date_lable_t,data_t);
    fitline = cftool(date_lable_t,data_t);
end
