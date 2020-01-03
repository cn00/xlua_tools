
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
    wrapKey = function(val)
        if breakline then
            if type(val) == "number" then
                return "[" .. val .. "] = "
            elseif type(val) == "string" then
                return "[" .. quoteStr(val) .. "] = "
            else
                return "[" .. tostring(val) .. "] = "
            end
        else
            if type(val) == "string" then
                return quoteStr(val) .. " = "
            else
                return ""
            end
        end
    end
    wrapVal = function(val, level)
        if type(val) == "table" then
            return dumpObj(val, level)
        elseif type(val) == "number" then
            return val
        elseif type(val) == "string" then
            return quoteStr(val)
        else
            return tostring(val)
        end
    end
    dumpObj = function(obj, level)
        if type(obj) ~= "table" then
            return wrapVal(obj)
        end
        level = level + 1

        local tokens = {}
        tokens[#tokens + 1] = "{"
        if level < 5 then 
            for k, v in pairs(obj) do
                tokens[#tokens + 1] = getIndent(level) .. wrapKey(k) .. wrapVal(v, level) .. ","
            end
            local meta = getmetatable(obj)
            if meta ~= nil then tokens[#tokens + 1] = getIndent(level) .. "__meta = " .. wrapVal(meta, level) .. "," end
        else
            tokens[#tokens + 1] = getIndent(level) .. "..."
        end
        tokens[#tokens + 1] = getIndent(level - 1) .. "}"
        if breakline then
            return table.concat(tokens, "\n")
        else
            return table.concat(tokens, " ")
        end
    end
    return dumpObj(obj, 0)
end
table.dump = dump
return dump