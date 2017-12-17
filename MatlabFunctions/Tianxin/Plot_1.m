
function Plot_1(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;

    x = lakedata_desend{1:1299,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{1:1299,k};

    x = lakedata_desend{1300:1793,3};
    date_lable_sd=datenum(x);
    data_sd = lakedata_desend{1300:1793,k};

    x = lakedata_desend{1794:2677,3};
    date_lable_s=datenum(x);
    data_s = lakedata_desend{1794:2677,k};

    x = lakedata_desend{2678:3497,3};
    date_lable_r=datenum(x);
    data_r = lakedata_desend{2678:3497,k};

    x = lakedata_desend{3498:3605,3};
    date_lable_n=datenum(x);
    data_n = lakedata_desend{3498:3605,k};

    x = lakedata_desend{3606:4425,3};
    date_lable_f=datenum(x);
    data_f = lakedata_desend{3606:4425,k};

    x = lakedata_desend{4426:5248,3};
    date_lable_d=datenum(x);
    data_d = lakedata_desend{4426:5248,k};

    x = lakedata_desend{5249:5456,3};
    date_lable_cp=datenum(x);
    data_cp = lakedata_desend{5249:5456,k};

    x = lakedata_desend{5457:5951,3};
    date_lable_bb=datenum(x);
    data_bb = lakedata_desend{5457:5951,k};

    x = lakedata_desend{5952:6125,3};
    date_lable_an=datenum(x);
    data_an = lakedata_desend{5952:6125,k};

    x = lakedata_desend{6126:6508,3};
    date_lable_a10=datenum(x);
    data_a10 = lakedata_desend{6126:6508,k};

    x = lakedata_desend{6509:6531,3};
    date_lable_a=datenum(x);
    data_a = lakedata_desend{6509:6531,k};

    plot(date_lable_t,data_t,date_lable_sd,data_sd,date_lable_s,data_s ,date_lable_r,data_r ,date_lable_n,data_n,date_lable_f,data_f,date_lable_d,data_d,date_lable_cp,data_cp ,date_lable_bb,data_bb ,date_lable_an,data_an,date_lable_a10,data_a10 ,date_lable_a,data_a);
    datetick('x','dd-mm-yyyy','keepticks')
    grid on
    title(['Graph of offshore chemistry data for', columnname{1,k}]);
    legend('T','SD','S','R','N','F','D','CP','BB','AN','A10','A');
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('Graph_for_',columnname{1,k})
    print(printout,'-dpng')
end
