
local util = require "util"
local sqlite3 = require "lsqlite3"

-- local dicdbpath = "/Users/cn/a3/c3/client/Unity/Tools/excel/strings.sqlite3"
local dicdbpath = "/Users/cn/dark/strings-dic.sqlite3"
local db = sqlite3.open(dicdbpath);
local function checkdberr( errno, sql )
    if errno ~= sqlite3.OK then print("sqlerr", db:errmsg():gsub("\n", "\\n"), sql) end
end


local sql = "select s from dic where (zh is null or zh = '') and (tr is null or tr = '');"
local t = {}
local h
local errno = db:exec(sql, function ( hd, n, vs, ns )
    h = ns
    local r = {table.unpack(vs)}
    t[1+#t] = r[1]
    return sqlite3.OK
end)
if errno ~= sqlite3.OK then print("sqlerr", db:errmsg():gsub("\n", "\\n"), sql) end

local translated = util.BF(t, function(batchi, batch)
    for _, v in ipairs(batch) do
        print("batchi", batchi, _, v.src, v.dst)
        local sql = "update dic set zh = '" .. v.dst:gsub("嗯嗯嗯嗯嗯", "\n") .. "', v = 'bf-zh|' || v where s = '" .. v.src:gsub("嗯嗯嗯嗯嗯", "\n") .. "';"
        checkdberr(db:exec(sql), sql:gsub("\n", "\\n"))
    end
end)
--for i = 1, #translated do
--    for _,v in ipairs(translated[i]) do
--        print(i, _, v.src, v.dst)
--        local sql = "update dic set zh = '" .. v.dst:gsub("嗯嗯嗯嗯嗯", "\n") .. "', v = 'bf-zh|' || v where s = '" .. v.src:gsub("嗯嗯嗯嗯嗯", "\n") .. "';"
--        checkdberr(db:exec(sql), sql:gsub("\n", "\\n"))
--    end
--end

assert(db:exec("VACUUM;"))
db:close()
