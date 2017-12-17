function Json_A(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{6509:6531,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{6509:6531,k};
    [M,I] = max(data_t)
    [M2,I2] = min(data_t)
    p = polyfit(date_lable_t,data_t,1)
    
    output.max = M;
    output.max_date = x(I);
    output.min = M2;
    output.max_date = x(I2);
    output.std = std(data_t);
    output.mean = mean(data_t);
    output.slope = p(1);
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
    saveJSONfile(output, sprintf('Site_A_%s.json',columnname{1,k} ));
    columnname{1,k}
end