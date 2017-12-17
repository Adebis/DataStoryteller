function Plot(kt,lakedata_desend)

    k = kt; %set the variable that want to analyze.

    columnname = lakedata_desend.Properties.VariableNames;

    x = lakedata_desend{5457:5951,3};

    date_lable_t=datenum(x);

    data_t = lakedata_desend{5457:5951,k};

    scatter(date_lable_t,data_t);

    hold on
    datetick('x','dd-mm-yyyy','keepticks')

    grid on

    title(['Graph of offshore chemistry data for', columnname{1,k}]);

    legend('BB');

    ylabel('mg/l');

    xlabel('Date')

    printout = strcat('Tsite_Graph_for_',columnname{1,k});

    print(printout,'-dpng');
    [date_lable_t,data_t] = prepareCurveData(date_lable_t,data_t);

    fitline = fit(date_lable_t,data_t,'pchipinterp');

    plot(fitline,date_lable_t,data_t);

end