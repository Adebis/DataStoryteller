
function Plot_AN(kt,lakedata_desend)
    k = kt; %set the variable that want to analyze.
    columnname = lakedata_desend.Properties.VariableNames;
    x = lakedata_desend{5952:6125,3};
    date_lable_t=datenum(x);
    data_t = lakedata_desend{5952:6125,k};
    scatter(date_lable_t,data_t);
    m = mean(data_t)
    ipt = findchangepts(data_t)
    size(data_t);
    [n,c] = hist3([date_lable_t(:), data_t(:)]);
    values = hist3([date_lable_t(:), data_t(:)],[51 51]);
    contour(c{1},c{2},n);
    imagesc(values.');
    colorbar;
    axis equal;
    axis xy;
    datetick('x','dd-mm-yyyy','keepticks');
    [peaks,idx]=findpeaks(data_t);
    plot(date_lable_t,data_t,date_lable_t(67),data_t(67),'o');
    grid on
    title(['Density Graph of offshore chemistry data for ', columnname{1,k}]);
    %legend('AN');
    ylabel('mg/l');
    xlabel('Date')
    printout = strcat('AN_site_Graph_for_',columnname{1,k});
    print(printout,'-dpng');
end