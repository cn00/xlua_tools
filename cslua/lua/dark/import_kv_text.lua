
require("util.stringx")
local luasql = require "luasql.mysql"
--for k,v in pairs(luasql) do print(k,v) end
local util = require("util")

local db = assert(luasql.mysql():connect("dark_text", "dark", "654123", "cn"))

local kv_text_tab = "kv_text_v133"
local res = assert(db:exec("show tables;"))
print(util.dump(res:fetch()))

local function DoOneFile(fpath)
    local f = assert(io.open(fpath), fpath .. " not found")
    
    fpath = string.gsub(fpath,"^/Users/cn/dark/client/", ""):gsub("^/Users/cn/dark/client-jp/", "")
            :gsub("/cn/", "/ja/")
            :gsub("/en/", "/ja/")
            :gsub("/ko/", "/ja/")
            :gsub("/tha/", "/ja/")
    local res = assert(db:exec(string.format([[select id from file_list where p = '%s';]], fpath)))
    local fid = res:fetch()
    if fid == nil then
        local res2 = assert(db:exec(string.format([[insert into file_list (p) values ('%s');]], fpath)))
        print(res2:fetch())
        res = assert(db:exec(string.format([[select id from file_list where p = '%s';]], fpath)))
        fid = res:fetch()
    end
    print(fid, fpath)
    
    local t = {"-- "..fpath, "\ninsert ignore into "..kv_text_tab.." (l, k, v, c, f) values "}
    local ln = 0
    local last = ""
    for l in f:lines("*l") do
        ln = ln + 1
        --l = string.gsub(l, "^%s+*", ""):gsub("%s+$", "")
        l = string.gsub(l, "'", "''"):gsub("\\", "\\\\")
        local k, v, c = "", "", ""
        --if #l > 0 then
            if string.sub(l, 1,2) == "//" then -- comment
                k, c ="//empty_"..ln, l
            else
                local pos = l:find(":")
                if pos then
                    k,v = l:sub(1, pos-1), l:sub(pos+1)
                else
                    k,v = "//empty_"..ln, l
                end
            end
        --end
        last = string.format("\t(%d, '%s', '%s', '%s', '%d')", ln ,k, v, c, fid)
        table.insert(t, last..",")
    end
    t[#t] = last
    table.insert(t, ("ON DUPLICATE KEY UPDATE v = VALUES(v);")) --  where k = VALUES(k) and f = VALUES(f)
    
    local sql = table.concat(t, "\n")
    local tmpf = io.open("tmp.sql", "w")
    tmpf:write(sql)
    tmpf:close()
    if ln > 0 then assert(db:exec(sql)) end
end
--DoOneFile("/Users/cn/dark/client/dark-client/Assets/workspace/text/ja/library/movie.txt")


local function DoOneTrans(fpath, lang)
    local f = assert(io.open(fpath), fpath .. " not found")

    fpath = string.gsub(fpath,"^/Users/cn/dark/client/", ""):gsub("^/Users/cn/dark/client-jp/", "")
                  :gsub("/"..lang.."/", "/ja/")
                  --:gsub("/en/", "/ja/")
                  --:gsub("/ko/", "/ja/")
                  --:gsub("/tha/", "/ja/")
    local res = assert(db:exec(string.format([[select id from file_list where p = '%s';]], fpath)))
    local fid = res:fetch()
    if fid == nil then
        assert(db:exec(string.format([[insert into file_list (p) values ('%s');]], fpath)))
        res = assert(db:exec(string.format([[select id from file_list where p = '%s';]], fpath)))
        fid = res:fetch()
    end
    print(fid, fpath)

    local t = {"-- "..fpath, "\ninsert into "..kv_text_tab.." (l, k, "..lang..", f) VALUES "}
    local ln = 0
    local last = ""
    for l in f:lines("*l") do
        ln = ln + 1
        l = string.gsub(l, "'", "''"):gsub("\\", "\\\\")
        local k, v, c = "//empty_"..ln, l, ""
        --l = string.gsub(l, "^%s+*", ""):gsub("%s+$", "")
        if #l > 0 then
            if string.sub(l, 1,2) == "//" then -- comment
                c = l
            else
                local pos = l:find(":")
                if pos then 
                    k,v = l:sub(1, pos-1), l:sub(pos+1) 
                end
            end
        end
        if #k > 0 and #v > 0 then
            last = string.format("\t(%d, '%s', '%s', '%d')", ln, k, v, fid)
            table.insert(t, last..",")
        end
    end
    t[#t] = last;
    table.insert(t, ("ON DUPLICATE KEY UPDATE "..lang.." = VALUES("..lang..") ;")) -- where k = VALUES(k) and f = VALUES(f)
    
    local sql = table.concat(t, "\n")
    local tmpf = io.open("tmp.sql", "w")
    tmpf:write(sql)
    tmpf:close()
    if ln > 0 then assert(db:exec(sql)) end
end

util.GetFiles(
    {
        "dark-client/Assets/Resources/text/ja",
        "dark-client/Assets/workspace/text/ja",
    }
    ,function(fpath)
                DoOneFile(fpath)
            end
    , "txt"
)

util.GetFiles(
    {
        "dark-client/Assets/Resources/text/en",
        "dark-client/Assets/workspace/text/en",
    }
    ,function(fpath)
            --DoOneFile(fpath)
            DoOneTrans(fpath, "en")
        end
    , "txt"
)

util.GetFiles(
        {
            "dark-client/Assets/Resources/text/cn",
            "dark-client/Assets/workspace/text/cn",
        }
,function(fpath)
            --DoOneFile(fpath)
            DoOneTrans(fpath, "cn")
        end
, "txt"
)

util.GetFiles(
        {
            "dark-client/Assets/Resources/text/ko",
            "dark-client/Assets/workspace/text/ko",
        }
,function(fpath)
            --DoOneFile(fpath)
            DoOneTrans(fpath, "ko")
        end
, "txt"
)


local function CollectAnsciiString(fpath)
    local f = assert(io.open(fpath))
    
    fpath = string.gsub(fpath, "^/Users/cn/dark/client/", "")
    assert(db:exec(string.format([[insert into file_list (p) values ('%s');]], fpath)))
    local res = assert(db:exec(string.format([[select id from file_list where p = '%s';]], fpath)))
    local fid = res:fetch()
    print(fid, fpath)

    local t = {"insert into kv_text (l, k, v, c, f) values "}
    local s = f:read("*a")
    local last
    for l in s:gmatch('"([^"]*\\u[^"]*)"') do 
        local k, v, c = "", "", "", ""
        k = l
        l = string.gsub(l, "'", "''")
        v = string.gsub(l, '\\u....', function(ii)
            return utf8.char(tonumber((ii:gsub('\\u', '0x'))));
        end)
        last = string.format("\t(%d, '%s', '%s', '%s', '%d')", 0 ,k, v, c, fid)
        table.insert(t, last..",")
    end
    t[#t] = last
    --table.insert(";")
    local sql = table.concat(t, "\n")
    local tmpf = io.open("tmp.sql", "w")
    tmpf:write(sql)
    tmpf:close()
    --assert(db:exec(sql))
end

--util.GetFiles(
--    ""
--    ,function(fpath) 
--        
--    end
--    , "prefab"
--)