function Plot(kt,lakedata_desend,startp,endp)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    %% CP
%     x = lakedata_desend{5249:5456,3};
%     data_t = lakedata_desend{5249:5456,k};
    
    %% SD
    x = lakedata_desend{1300:1793,3};
    data_t = lakedata_desend{1300:1793,k};
    
    %% BB
%     x = lakedata_desend{5457:5951,3};
%     data_t = lakedata_desend{5457:5951,k};
    
    %% AN
%     x = lakedata_desend{5952:6125,3};
%     data_t = lakedata_desend{5952:6125,k};

    date_lable_t=datenum(x);
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
%     hold off
%     [date_lable_t,data_t] = prepareCurveData(date_lable_t,data_t);
%     
%     fitline = fit(date_lable_t,data_t,'pchip');
%     plot(fitline,date_lable_t,data_t);


    %% Normalize date in x axis, start from 0 in x axis instead of date
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
    
    
%% avg y values and eliminate
    c = 1;
    threshold = 50;
    sum = 0;
    sum_c = 0;
    tu = [];
    tv = [];
    for i=1:k
        if (u(i)<c*threshold)
            sum = sum+v(i);
            sum_c = sum_c+1;
        else
            tu(c) = (c-1/2)*threshold;
            tv(c) = sum/sum_c;
            c = c+1;
            sum = 0;
            sum_c=0;
        end
    end
    index1=find(tu>startp,1,'first');
    index2=find(tu<endp,1,'last');
    tu = tu;
    tv = tv;
    tu=tu(index1:index2);
    tv= tv(index1:index2);
    
    hold on
    plot(tu,tv,'.b');
    
  
    
    %% For using findchangepts function as segmentation, eliminate NaN value first.
    ind = ~isnan(tv);
    tvn = tv(ind);
    tun = tu(ind);
    
    
    %% Exclude outlier in the data
    TF = isoutlier(tvn,'gesd');
    exclude = tvn(~TF);
    excludeDate = tun(~TF);
    
    
    max_num_change = 3;
    findchangepts(exclude,'Statistic','linear','MaxNumChanges',max_num_change);
    
    %% 
    x = findchangepts(exclude,'Statistic','linear','MaxNumChanges',max_num_change)
    file = fopen('output.json','w');
    fprintf(file,'\n');
    index = 1;
    slopes = [];
        for i=1:length(x)
            tempx = excludeDate(index:x(i));
            tempy = exclude(index:x(i));
            fitline = polyfit(tempx,tempy,1);
            w1 = fitline(1);
            w2 = fitline(2);
            fprintf(file,jsonencode(["start",excludeDate(index);"starting value",exclude(index);"end",excludeDate(x(i));"ending value",exclude(x(i));"function","y="+w1+"x"+"+"+ w2]));
            index = x(i);
            fprintf(file,'\n');
        end

    hold off
    
% %% line seg
%     slope = 0;
%     set_x = [];
%     set_y = [];
%     sz = 1;
%     hold on
%     for i=1:k
%         if sz<3
%             set_x(sz) = u(i);
%             set_y(sz) = v(i);
%             sz = sz+1;
%         else
%             slope = (set_y(2) - set_y(1)) / (set_x(2)-set_x(1));
%         end
%         if (slope>0 && v(i)>v(i-1))
%             set_x(sz) = u(i);
%             set_y(sz) = v(i);
%             sz = sz+1;
%         elseif (slope<0 && v(i)<v(i-1))
%             set_x(sz) = u(i);
%             set_y(sz) = v(i);
%             sz = sz+1;
%         else
%             p = polyfit(set_x,set_y,1);
%             c = linspace(set_x(1),set_x(sz-1),10*(set_x(sz-1)-set_x(1)));
% %             plot(c,p(1)*c+p(2));
%             set_x(1) = u(i);
%             set_y(1) = v(i);
%             sz = 2;
%         end
%         plot(u(i),v(i),'-bx');
%     end 

%     findchangepts(u(i)','Statistic','linear','MaxNumChanges',6);
    
end