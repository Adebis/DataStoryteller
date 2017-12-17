function Unique(kt,lakedata_desend)

    k = kt; %set the variable that want to analyze.

    columnname = lakedata_desend.Properties.VariableNames;

%     x = lakedata_desend{5249:5456,3};

%     data_t = lakedata_desend{5249:5456,k};

    x = lakedata_desend{5457:5951,3};

    data_t = lakedata_desend{5457:5951,k};

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

    hold off


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

%% avg

%     c = 1;

%     threshold = 100;

%     sum = 0;

%     sum_c = 0;

%     tu = [];

%     tv = [];

%     for i=1:k

%         if (u(i)<c*threshold)

%             sum = sum+v(i);

%             sum_c = sum_c+1;

%         else

%             tu(c) = (c-1/2)*threshold;

%             tv(c) = sum/sum_c;

%             c = c+1;

%             sum = 0;

%             sum_c=0;

%         end

%     end

%

%     hold on

%     for i=1:c-2

%         plot(tu(i),tv(i),'.b');

%         line([tu(i) tu(i+1)],[tv(i) tv(i+1)]);

%     end

%     hold off

% %% line seg

    slope = 0;

    set_x = [];

    set_y = [];

    sz = 1;

%{
    for i=1:k

        if sz<3

            set_x(sz) = u(i);
            set_y(sz) = v(i);

            sz = sz+1;
        else

            slope = (set_y(2) - set_y(1)) / (set_x(2)-set_x(1));

        end

        if (slope>0 && v(i)>v(i-1))

            set_x(sz) = u(i);

            set_y(sz) = v(i);

            sz = sz+1;

        elseif (slope<0 && v(i)<v(i-1))

            set_x(sz) = u(i);

            set_y(sz) = v(i);

            sz = sz+1;

        else

            p = polyfit(set_x,set_y,1);

            c = linspace(set_x(1),set_x(sz-1),10*(set_x(sz-1)-set_x(1)));

            %plot(c,p(1)*c+p(2));

            set_x(1) = u(i);

            set_y(1) = v(i);
            sz = 2;

        end
        scatter(u(i),v(i));
        %plot(u(i),v(i),'-bx');

    end
    %}
    scatter(u,v);
    
