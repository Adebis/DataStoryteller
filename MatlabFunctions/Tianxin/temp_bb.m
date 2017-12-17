
function Plot_BB(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{5457:5951,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{5457:5951,k};
    %scatter(date_lable_t,data_t);
    %{
    datetick('x','dd-mm-yyyy','keepticks')
    grid on
    title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('BB');
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('BB_site_Graph_for_',columnname{1,k});
    %}
    %print(printout,'-dpng');
    
    %Unique the x
    
    [a,~] = size(x);

    u = [];

    v = [];

    new_y = 0;

    y_c = 0;

    k = 0;

    temp = date_lable_t(1);

    figure

    
    for i=1:a

        if (date_lable_t(i)==temp)

            y_c = y_c+1;

            new_y = new_y + data_t(i);

        else

             k = k+1;

             u(k) = temp-date_lable_t(1);

             v(k) = new_y/y_c;

             temp = date_lable_t(i);

             y_c = 0;

             new_y = 0;

        end

    end
    
    datetick('x','dd-mm-yyyy','keepticks')
    grid on
    title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('BB');
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('BB_site_Graph_for_',columnname{1,k});
    scatter(u,v);
    
    
    [date_lable_t,data_t] = prepareCurveData(date_lable_t,data_t);
    fitline = fit(date_lable_t,data_t,'cubic')
    yfitted = feval(fitline,date_lable_t);
    [ypk,idx] = findpeaks(yfitted)
    plot(fitline,date_lable_t,data_t);
    xpk = date_lable_t(idx);
    hold on
    plot(xpk,ypk,'o')
