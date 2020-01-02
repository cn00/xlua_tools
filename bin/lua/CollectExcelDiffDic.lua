local CS = CS
local System = CS.System

local util = require "util"

local count = 0
local function CollectExcelDiffDic(t)
    local version = 2
    for i,v in ipairs(t) do
        print(i, v.a)
        for kk,vv in pairs(v.sheets) do
            print(kk, #vv.cells)
            local src, jp, zh, currentRow
            src = v.a .. ":" .. kk
            local values = {"INSERT OR IGNORE INTO dic (s,zh,src, v) VALUES "}
            for iii,vvv in ipairs(vv.cells) do
                -- print(iii,vvv)
                if util.JpMatch(vvv.a).Count >0 then
                    jp = vvv.a:gsub("'", "''")
                    zh = vvv.b:gsub("'", "''")
                    currentRow= "(" 
                        .. "'"  .. jp .. "'" -- jp
                        .. ",'" .. zh .. "'"-- zh
                        .. ",'" .. src .."'" -- src
                        .. "," .. version -- v
                        ..")"
                    values[1+#values] = currentRow .. ","
                end
            end
            if currentRow ~= nil then values[#values] = currentRow .. ";" end
            count = count + #values - 1
            if #values > 1 then
                local sql = table.concat(values, "\n")
                -- print(sql)
                local cmd = db:CreateCommand();
                cmd.CommandText = sql;
                local reader = cmd:ExecuteReader();
                reader:Dispose()
            end
        end
    end
    print("total count:", count)
end
return CollectExcelDiffDic