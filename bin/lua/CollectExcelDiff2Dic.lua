local CS = CS
local System = CS.System

local util = require "util"

local count = 0
local function CollectExcelDiff2Dic(t, db, language, version)
    language = language or "zh"
    version = version or "new"
    for i,v in ipairs(t) do
        print(i, v.a)
        for kk,vv in pairs(v.sheets) do
            print(kk, #vv.cells)
            local src, jp, trans, currentRow
            local values = {"INSERT OR IGNORE INTO dic (s,".. language ..",src, v) VALUES "}
            for iii,vvv in ipairs(vv.cells) do
                src = v.a .. ":" .. kk .. "@" .. vvv.x .. "," .. vvv.y 
                -- print(iii,vvv)
                if util.JpMatch(vvv.a).Count >0 then
                    jp = vvv.a:gsub("'", "''")
                    trans = vvv.b:gsub("'", "''")
                    currentRow= "(" 
                        .. "'"  .. jp .. "'" -- jp
                        .. ",'" .. trans .. "'"-- trans
                        .. ",'" .. src .."'" -- src
                        .. ",'" .. version .."'" -- v
                        ..")"
                    values[1+#values] = currentRow .. ","
                end
            end
            if currentRow ~= nil then 
                values[#values] = currentRow
                    .. " ON CONFLICT(s) DO UPDATE SET "
                    ..language.." = CASE WHEN "..language.." ISNULL OR "..language.." = '' THEN excluded."..language.. " ELSE " .. language .. " END"
                    -- ..", src = '{s=\"'||excluded.src||\'\",t=\"'||excluded.tr||'\",'||char(13)||'n='||src||'}' WHERE tr <> excluded.tr;" -- too many C levels (limit is 200)
                    ..", src = '{"..language.."=\"'||excluded."..language.."||'\",src=\"'||excluded.src||\'\",v=\""..version.."\"},'||char(13)||src"
                    .." WHERE "..language.." <> excluded."..language
                    ..";"
            end
            count = count + #values - 1
            if #values > 1 then
                local sql = table.concat(values, "\n")
                print(sql)
                local cmd = db:CreateCommand();
                cmd.CommandText = sql;
                local reader = cmd:ExecuteReader();
                reader:Dispose()
            end
        end
    end
    print("total count:", count)
end
return CollectExcelDiff2Dic