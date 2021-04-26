
local function dump(obj, breakline)
    breakline = breakline == nil or breakline == true
    local getIndent, quoteStr, wrapKey, wrapVal, dumpObj
    getIndent = function(level)
        if breakline then
            return string.rep("\t", level)
        else
            return ""
        end
    end
    quoteStr = function(str)
        return '"' .. string.gsub(str, '"', '\"') .. '"'
    end
    wrapKey = function(val, breakline)
        if breakline then
            if type(val) == "number" then
                return "" -- "[" .. val .. "] = "
            elseif type(val) == "string" then
                if string.match(val, "[][-.,%%}{)(]\"'") ~= nil then
                    return "[" .. quoteStr(val) .. "] = "
                else
                    return val .. " = "
                end
            else
                return "[" .. tostring(val) .. "] = "
            end
        else
            if type(val) == "string" then
                if string.match(val, "[][-.,%%}{)(]\"'") ~= nil then
                    return quoteStr(val) .. " = "
                else
                    return val .. " = "
                end
            else
                return ""
            end
        end
    end
    wrapVal = function(val, level, ishead)
        if type(val) == "table" then
            if(val.____already_dumped____)then return quoteStr("nested_table") end
            return dumpObj(val, level, ishead)
        elseif type(val) == "number" then
            return val
        elseif type(val) == "string" then
            return quoteStr(val)
        else
            return tostring(val)
        end
    end
    dumpObj = function(obj, level, ishead)
        if type(obj) ~= "table" then
            return wrapVal(obj)
        end
        level = level + 1

        local tokens = {}
        tokens[#tokens + 1] = "{"
        
        obj.____already_dumped____ = true
        if level < 9 then
            for k, v in pairs(obj) do
                local head = false
                if k == "head" then head = true end
                if(k ~= "____already_dumped____")then
                    local vs = getIndent(level) .. wrapKey(k) .. wrapVal(v, level, head) .. ","
                    if type(k) == "string" and type(v) == "table" and #v > 5 then vs = vs .. " -- " .. k end
                    tokens[#tokens + 1] = vs
                end
            end
            local meta = getmetatable(obj)
            if meta ~= nil then tokens[#tokens + 1] = getIndent(level) .. "__meta = " .. wrapVal(meta, level) .. "," end
        else
            tokens[#tokens + 1] = getIndent(level) .. "..."
        end
        tokens[#tokens + 1] = getIndent(level - 1) .. "}"
        if breakline then
            if #tokens < 6 or ishead then
                local st = table.concat(tokens, " "):gsub("%s%s+", " ")
                if #st < 50 or ishead  then return st end
            end
            return table.concat(tokens, "\n")
        else
            return table.concat(tokens, " ")
        end
    end
    return dumpObj(obj, 0)
end
table.dump = dump
return dump