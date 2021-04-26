
require("util.stringx")
local luasql = require "luasql.mysql"
--for k,v in pairs(luasql) do print(k,v) end
local util = require("util")

local db = assert(luasql.mysql():connect("dark_text", "dark", "654123", "cn.local"))

local res = assert(db:exec("show tables;"))
print(util.dump(res:fetch()))

local function DoOneFile(fpath)
    local f = assert(io.open(fpath), fpath .. " not found")
    
    fpath = string.gsub(fpath, "^/Users/cn/dark/client/", "")
    assert(db:exec(string.format([[insert into file_list (p) values ('%s');]], fpath)))
    local res = assert(db:exec(string.format([[select id from file_list where p = '%s';]], fpath)))
    local fid = res:fetch()
    print(fid, fpath)
    
    local t = {"insert into kv_text (l, k, v, c, f) values "}
    local ln, k, v, c = 0, "", "", "", ""
    local last = ""
    for l in f:lines("*l") do
        ln = ln + 1
        local k, v, c = "", "", "", ""
        --l = string.gsub(l, "^%s+*", ""):gsub("%s+$", "")
        l = string.gsub(l, "'", "''")
        if #l > 0 then
            if string.sub(l, 1,2) == "//" then -- comment
                c = l
            else
                k,v = table.unpack(string.split(l, ":"))
            end
        end
        last = string.format("\t(%d, '%s', '%s', '%s', '%d')", ln ,k, v, c, fid)
        table.insert(t, last..",")
    end
    t[#t] = last
    --table.insert(";")
    local sql = table.concat(t, "\n")
    local tmpf = io.open("tmp.sql", "w")
    tmpf:write(sql)
    tmpf:close()
    assert(db:exec(sql))
end
--DoOneFile("/Users/cn/dark/client/dark-client/Assets/workspace/text/ja/library/movie.txt")
util.GetFiles(
    --"/Users/cn/dark/client/dark-client/Assets/workspace/text/ja"
"/Users/cn/dark/client/dark-client/Assets/Resources/text/ja"    
    ,function(fpath) DoOneFile(fpath) end
    , "txt"
)
