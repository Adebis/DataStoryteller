
function Plot_T(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{1:1299,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{1:1299,k};
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
    %title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('BB');
    ylabel('mg/l');
    xlabel('Date')
    %printout = strcat('BB_site_Graph_for_',columnname{1,k});
    scatter(u,v);
    figure
    histogram2(u,v)
end
