local CS = CS
local System = CS.System

local util = require "util"

local count = 0
local function CollectExcelDiffDic(t, db, language, version)
    language = language or "zh"
    version = version or "new"
    for i,v in ipairs(t) do
        print(i, v.a)
        for kk,vv in pairs(v.sheets) do
            print(kk, #vv.cells)
            local src, jp, trans, currentRow
            src = v.a .. ":" .. kk
            local values = {"INSERT OR IGNORE INTO dic (s,".. language ..",src, v) VALUES "}
            for iii,vvv in ipairs(vv.cells) do
                -- print(iii,vvv)
                if util.JpMatch(vvv.a).Count >0 then
                    jp = vvv.a:gsub("'", "''")
                    trans = vvv.b:gsub("'", "''")
                    currentRow= "(" 
                        .. "'"  .. jp .. "'" -- jp
                        .. ",'" .. trans .. "'"-- trans
                        .. ",'" .. src .."'" -- src
                        .. "," .. version -- v
                        ..")"
                    values[1+#values] = currentRow .. ","
                end
            end
            if currentRow ~= nil then 
                values[#values] = currentRow .. " ON CONFLICT(s) DO UPDATE SET "
                ..language.." = excluded."..language 
                ..", src = src || char(13) || excluded.src;"
            end
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