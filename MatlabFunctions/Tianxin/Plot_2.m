
function Plot_2(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;

    x = lakedata_desend{:,3};
    date_label=datenum(x);
    data = lakedata_desend{:,k};
    scatter(date_label,data);
    datetick('x','dd-mm-yyyy','keepticks')
    grid on
    title(['Graph of offshore chemistry data for', columnname{1,k}]);
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('Whole_Graph_for_',columnname{1,k})
    print(printout,'-dpng')
end
