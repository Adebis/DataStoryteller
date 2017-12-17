function Json_BB(kt,lakedata_desend)
    %Set the variable that want to analyze.
    k = kt; 
    %Get the chemistry name
    columnname = lakedata_desend.Properties.VariableNames;
    %5457-5951 is the range for Site BB
    x = lakedata_desend{5457:5951,3};
    %Transform the date to the type that matlab can use.
    %Get the x values and y values.
    date_lable_t=datenum(x);
    data_t = lakedata_desend{5457:5951,kt}
    %Get the real max and min
    [M,I] = max(data_t)
    [M2,I2] = min(data_t)
    %Input the real max, min, std, mean to Json format
    output.max = M;
    output.max_date = x(I);
    output.min = M2;
    output.max_date = x(I2);
    output.std = nanstd(data_t);
    output.mean = nanmean(data_t);
    
    %Get the u,v which unique the x value.
    [a,~] = size(x);

    u = [];

    v = [];

    new_y = 0;

    y_c = 0;

    k = 0;

    temp = date_lable_t(1);

    figure
    %Process to unique the x values.
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
    %End of the unique process
    datetick('x','dd-mm-yyyy','keepticks')
    grid on
    %title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('BB');
    ylabel('mg/l');
    xlabel('Date')
  
    %Print out the dot graph with u and v
    scatter(u,v);
    figure;
    %printout = strcat('BB_site_Graph_for_',columnname{1,k});
    %Begin the smooth and curve fitting process.
    u2 = [];
    v2 = [];
    [dim, length] = size(u);
    %Smooth the data with Loess mode and 0.2 scale.
    u2 = u;
    %Smooth function
    v2 = smooth(u,v,0.2,'loess');
    %Prepare for the curve fitting    
    [u2,v2] = prepareCurveData(u2,v2);
    fitline = fit(u2,v2,'pchip')
    yfitted = feval(fitline,u2);
    %[ypk,idx] = findpeaks(yfitted,'NPeaks',5,'SortStr','descend');
    [ypk,idx,~,prm] = findpeaks(yfitted);
    [ypk2,idx2,~,prm2] = findpeaks(-yfitted);
    [~,i] = sort(prm,'descend');
    [~,i2] = sort(prm2,'descend');
    
    %Print out the fitline on the dots graph.
    plot(fitline,u,v);
    xpk = u2(idx);
    xpk2 = u2(idx2);
    hold on
    plot(u2(idx(i(1:5))),v2(idx(i(1:5))),'o ')
    plot(u2(idx2(i2(1:5))),v2(idx2(i2(1:5))),'*')
    
    uppeak = v(idx(i(1:5)))
    downpeak = v(idx2(i2(1:5)))
    [maxstart,maxend] = rapid_grow(date_lable_t,data_t);
    [minstart,minend] = rapid_down(date_lable_t,data_t);
    output.rapid_grow_start = x(maxstart);
    output.rapid_grow_end = x(maxend);
    output.rapid_grow_start_data = data_t(maxstart);
    output.rapid_grow_end_data = data_t(maxend);
    output.rapid_down_start = x(minstart);
    output.rapid_down_end = x(minend);
    output.rapid_down_start_data = data_t(minstart);
    output.rapid_down_end_data = data_t(minend);
    output.major_up_peaks = uppeak;
    output.major_down_peaks = downpeak;
    
    savejson('',output, sprintf('Site_BB_%s.json',columnname{1,kt} ));
    %saveJSONfile(output, sprintf('Site_BB_%s.json',columnname{1,kt} ));
   
end