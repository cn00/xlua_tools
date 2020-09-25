
local bf=CS.Baidu.Fanyi.Do;
local sqlite3 = require "lsqlite3"
local json = require "json"
local dicdbpath = "/Volumes/Data/a3/c3/client/Unity/Tools/excel/strings.sqlite3"
local db = sqlite3.open(dicdbpath);

function checkdberr( errno, sql )
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

local ppage = 15
local page = #t
local limitl = 1000
local batchi = 1
local batchl = 0
local batch = {}
for i = 1, #t do
    local si = t[i]:gsub("\n", "嗯嗯嗯嗯嗯")
    batchl = batchl + #si
    if(batchl > limitl)then
        local src = table.concat( batch, "\n" )
        local js = bf(src)
        print("js", i, js)
        local transt = json.decode(js)
        for _,v in ipairs(transt.trans_result) do
            print(page, i, batchl, v.src, v.dst)
            local sql = "update dic set zh = '" .. v.dst:gsub("嗯嗯嗯嗯嗯", "\n") .. "', v = 'bf-zh|' || v where s = '" .. v.src:gsub("嗯嗯嗯嗯嗯", "\n") .. "';"
            checkdberr(db:exec(sql), sql:gsub("\n", "\\n"))
        end

        batchl = #si
        batchi = 1
        batch = {}
    end
    batch[batchi] = si
    batchi = batchi + 1
end

assert(db:exec("VACUUM;"))
db:close()