%
function Plot_BB(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{5457:5951,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{5457:5951,k};
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
    figure;
    %hold off
    u2 = [];
    v2 = [];
    [dim, length] = size(u);
    %{
    %The function to smooth.
    for t = 1:(length-9)
        %u2(t) = (u(t)+u(t+1)+u(t+2)+u(t+3)+u(t+4)+u(t+5))/6;
        u2(t) = u(t);
        v2(t) = v(t);
        for k = 1:8
            u2(t) = u2(t) + u(t+k);
        end
        u2(t) = u2(t)/9
        
        for k = 1:8
            v2(t) = v2(t) + v(t+k);
        end
        v2(t) = v2(t)/9
        %v2(t) = (v(t)+v(t+2)+v(t+1)+v(t+3)+v(t+4)+v(t+5))/6;
    end
  %}
    u2 = u;
    v2 = smooth(u,v,0.2,'loess');
        
    [u2,v2] = prepareCurveData(u2,v2);
    fitline = fit(u2,v2,'pchip')
    yfitted = feval(fitline,u2);
    %[ypk,idx] = findpeaks(yfitted,'NPeaks',5,'SortStr','descend');
    [ypk,idx,~,prm] = findpeaks(yfitted);
    [~,i] = sort(prm,'descend');
    
    [ypk2,idx2] = findpeaks(-yfitted);
    plot(fitline,u,v);
    xpk = u2(idx);
    xpk2 = u2(idx2);
    hold on
    plot(u2(idx(i(1:5))),v2(idx(i(1:5))),'o ')
    %plot(xpk,v2(idx),'o')
    hold on
    %plot(xpk2,v2(idx2),'o')
    uppeak = v(idx(i(1:5)))
    %downpeak = v2(idx2)
end