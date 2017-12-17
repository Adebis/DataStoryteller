H = height(lakedata_desend)

site = lakedata_desend{1,1};
for x = 1:H
    temp = lakedata_desend{x,1};
    if site~=temp
        disp(x);
        disp(lakedata_desend{x,1});
        site = lakedata_desend{x,1};
    end
end