function [start,tend] = rapid_down(x,y)
    min = 0;
    start = 0;
    tend = 0;
    for i= 1:size(y)
        for j = i+1:size(y)
            if (y(j)-y(i))/(x(j)-x(i)) < min
                min  = (y(j)-y(i))/(x(j)-x(i));
                start = i;
                tend = j;
            end
        end
    end
    return
end