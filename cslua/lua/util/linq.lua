

local table = table
local string = string

function table:wherei(filter)
    assert(type(filter) == "function", "filter must by a function return a boolean")
    local t = {}
    for i, v in ipairs(self) do
        local by = filter(v)
        if by then
            table.insert(t, v)
        end
    end
    return t
end

function table:where(filter)
    assert(type(filter) == "function", "filter must by a function return a boolean")
    local t = {}
    for i, v in pairs(self) do
        local by = filter(v)
        if by then
            table.insert(t, v)
        end
    end
    return t
end

function table:selecti(filter)
    assert(type(filter) == "function", "filter must by a function return a sub obj")
    local t = {}
    for i, v in ipairs(self) do
        table.insert(t, filter(v))
    end
    return t
end

function table:select(filter)
    assert(type(filter) == "function", "filter must by a function return a sub obj")
    local t = {}
    for k, v in pairs(self) do
        table.insert(t, filter(v))
    end
    return t
end

function table:groupbyi(filter)
    assert(type(filter) == "function", "filter must by a function return group name")
    local t = {}
    for i, v in ipairs(self) do
        local by = filter(v)
        if by then
            t[by] = t[by] or {}
            table.insert(t[by], v)
        end
    end
    return t
end

function table:groupby(filter)
    assert(type(filter) == "function", "filter must by a function return group name")
    local t = {}
    for k, v in pairs(self) do
        local by = filter(v)
        if by then
            t[by] = t[by] or {}
            table.insert(t[by], v)
        end
    end
    return t
end

function table:uniqi(filter)
    filter = filter or function(i)return i end
    return table.select(table.groupbyi(self, filter), function(it)
        return it[1]
    end)
end

function table.linqtest()
    local t = {
        {k1="abc", k2 = 123},
        {k1="def", k2 = 456},
        {k1="abc", k2 = 789},
        {k1="abc", k2 = 789},
    }
    local dump = require("dump")
    local wh = t:wherei(function(i) return i.k2 <= 456 end)
    assert(#wh == 2, "wherei failed")
    local gr = t:groupbyi(function(i) return i.k1 end)
    assert(#gr["abc"] == 3, "groupbyi failed")
    assert(#gr["def"] == 1, "groupbyi failed")
    local sel=table.selecti(t,function(i)return i.k1 end)
    assert(#sel == 4, "selecti failed")
end

return table