-- local util = require "util"
-- local dump = require "dump"
local sqlite3 = require "lsqlite3"
local lfs = require "lfs"
local util = require "util"
local table = require("util.linq")
local string = require("util.stringx")

local unpack = unpack or table.unpack

local print = function(...)
    _G.print(...)
end

local dicdbpath = "/Users/cn/dark/strings-dic.sqlite3"
local db = sqlite3.open(dicdbpath);
assert(db ~= nil, "db open failed")

local sql = [[
CREATE TABLE IF NOT EXISTS "dic"(
	"id"	INTEGER UNIQUE,
	"s"	text UNIQUE,
	"zh"	text,
	"tr"	text,
	"src"	text,
	"v"	INTEGER DEFAULT 0,
	"ut"	TEXT,
	PRIMARY KEY("id" AUTOINCREMENT)
)]]
assert(sqlite3.OK == db:exec(sql), db:errmsg():gsub("\n", "\\n"))

db:create_function("uniqsrc", 2, function(ctx, id, src)
    src = string.gsub(src,"^\n", "")
    print(id, string.sub(src, 1,5))
    local ss= string.split(src:gsub('/Users/cn/dark/', "./"), ',') -- load("return {"..src.."}")();
    local ust = table.uniqi(ss) -- table.where(table.uniqi(ss), function(i) return nil == string.match(i, "_Users_cn_") end)
    local us = table.concat(ust, ",")
    if #ss > #ust then
        print(id, #ss, #ust)
    end
    --ctx:result_number(#ss);
    ctx:result_text(us)
end)

local time = os.time()

local function trim( s, t )
    t = t or {"^%s+", "%s+$", "^//*", "^'", "'$", "^\"", "\"$"}
    for i,v in ipairs(t) do
        s = s:gsub(v, "")
        s = s:gsub("^%s+", "")
        s = s:gsub("%s+$", "")
    end
    return s
end

local function strip( s )
	local t = {
		{'%%', '‰'}, -- 必须放第一个
		{'%(', '%%('},
		{'%)', '%%)'},
		-- {'%[', '%%['},
		-- {'%]', '%%]'},
		{'%$', '%%$'},
		{'%?', '%%?'},
		-- {'%+', '%%+'},
		-- {'%-', '%%-'},
		{'%*', '%%*'},
		-- {'%:', '%%:'},
	}
	for i,v in ipairs(t) do
		s = s:gsub(v[1], v[2])
		-- print("strip", v[1], v[2], s)
	end
	return s
end

local gmsc, ggpc, gslc = 0,0,0
-- local onconflict = [[ ON CONFLICT(s) DO UPDATE SET s = CASE WHEN s ISNULL OR s = '' THEN excluded.s ELSE s END, src = excluded.src||','||char(13)||src;]]
local onconflict = [[ ON CONFLICT(s) DO UPDATE SET s = excluded.s, src = uniqsrc(excluded.src||','||char(10)||src), ut = excluded.ut;]]
local function TransOneV2( fpath, regular_jp, t )
	local f = io.open(fpath)
	local s = f:read('*a')
	f:close()

	local ms = string.gmatch(s, regular_jp)
	local mst = {}
	for si in ms do
		mst[1+#mst] = si
	end
	ms = table.uniqi(mst)

	local trimt = {
		"^>", "<$"
		, "^'", "'$"
		, "^\"", "\"$"
			-- , "^//*", "^'", "'$", "^\"", "\"$"
		, "\n"
	}
	local jp = nil
	local selectdic = {'select * from (select * from dic where s in ('}
	local insertdic = {'insert into dic (s, src, ut) VALUES '} -- no trans jp
	local i = 0
	for _, si in ipairs(ms) do
		if utf8.len(si) < 2 then goto continue end
		i = i + 1

		--jp = "'" .. trim(si, trimt) .. "'"
		jp = "'" .. si .. "'"
		-- print("ms", i, jp)
		selectdic[1+#selectdic] = '    ' .. jp .. ','

		if i % 100 == 0 then
			insertdic[#insertdic] = string.gsub(insertdic[#insertdic], ",$", onconflict .. " insert into dic (s, src, ut) VALUES ")
		end
		insertdic[1+#insertdic] = '    (' .. jp .. ', \''.. fpath ..'\', '..time..'),'

		::continue::
	end
	if #selectdic < 2 and #insertdic < 2 then return end

	print('--->', fpath, #selectdic, #insertdic)
	
	selectdic[#selectdic] = '    ' .. jp
	selectdic[1+#selectdic] = ') and ((zh NOTNULL and zh <> \'\') or ( tr NOTNULL and tr <> \'\'))) ORDER BY length(s) desc;'
	
	insertdic[#insertdic] = '    (' .. jp .. ', \''.. fpath ..'\', '..time..')'
	local language = "s"
	insertdic[1+#insertdic] = onconflict

	lfs.mkdir("/Users/cn/dark/tmp-sel-sql")
	lfs.mkdir("/Users/cn/dark/tmp-ins-sql")
	local sql = table.concat( insertdic, "\n")

		local fti = io.open("/Users/cn/dark/tmp-ins-sql/" .. fpath:gsub("/", "_") .. "-insert.sql", "w")
		assert(fti, fpath)
		if fti then
			fti:write(sql)
			fti:close()
		end
	local err = db:exec(sql)
	if err ~= sqlite3.OK then 
		print("error insert", err, db:errmsg():gsub("\n", "\\n")) 
	end

	local sql = table.concat( selectdic, "\n")
	local fts = io.open("/Users/cn/dark/tmp-sel-sql/" .. fpath:gsub("/", "_") .. "-select.sql", "w")
	fts:write(sql)
	fts:close()

	-- if true then return end

	-- check dic
	local dic = {}
	local head = nil
	for row in db:nrows(sql) do
		dic [1+#dic] = row
	end
	if #dic < 1 then
		print("no trans find.", "new_jp", #insertdic)
		return
	end
	--sethead(dic, head)

	-- trans replace
	for i,v in ipairs(dic) do
		if utf8.len(v.s) < 2 then goto continue2 end
		if v.zh == nil or v.zh == '' then v.zh = 'tr:' .. v.tr end
		if v.v and (v.v:match("bdfy") or v.v:match("bf")) then v.zh = v.zh end
		print('replace', i, v.s, v.zh)
		if t == 'sh' then
			s = CS.xlua.Util.Replace(s, ("'" .. v.s .. "'"), ("'" .. v.zh .. "'"))
		elseif t == 'c' then
			--s = CS.xlua.Util.Replace(s, ('"' .. v.s .. '"'), ('"' .. v.zh .. '"'))
			-- s = CS.xlua.Util.Replace(s, v.s, v.zh)
			-- s = string.gsub(s, "%%", "％")
			v.s = string.gsub(v.s, "%%", "％")
			v.zh = string.gsub(v.zh, "%%", "％")
			s = string.gsub(s, v.s, v.zh)
		elseif t == 'xml' then
			s = CS.xlua.Util.Replace(s, ( v.s ), ( v.zh ))
		end
		::continue2::
	end
	f = io.open(fpath, "w")
	f:write(s)
	f:close()

end

local function UniqueSrc(db)
	for row in db:nrows([[update dic set src = uniqsrc(id, src)]]) do
		print(row.id, row.usrc)
	end
end

--[[
f78719a | 2021-04-06 18:09:39 | cn | revert lowerCaseKatakanaDictionary
ac2d8b6 | 2021-04-06 18:03:24 | cn | bf 0.1
]]

-- local fpath = "/Volumes/Data/a3/s/v210/fuel/appadm/config/menu.php"
-- TransOne(fpath)

-- utf8.charpattern
local charpattern = '[\0-\127\194-\253][\128-\191]*'
-- 单字节字符首字节编码[0-127]   0x00-0x7f
-- 双字节字符首字节编码[194-223] 0x0080-0x07ff
-- 三字节字符首字节编码[224-239] 0x0800-0xffff
-- 四字节字符首字节编码[240-247] 0x10000-0x1fffff
-- 五字节字符首字节编码[248-251] 0x200000-0x3ffffff
-- 六字节字符首字节编码[252-253] 0x4000000-0x7fffffff
local luaregular_jp   = '[^\0-\127]*\227[\129-\131][\128-\191][^\0-\127]*' -- 包含一个日文字母 \227[\128-\132][\128-\191]
--local luaregular_jpzh = '[\0-\127\194-\253][\128-\191]+[\0-\253]*'

--local regular_jpzh= '[\\u3021-\\u3126\\u4e00-\\u9fa5]+'; --jp+zh

-- local csregular_jpzh= '[\\u4e00-\\u9fa5]*[\\u3021-\\u3126]+[\\u3021-\\u3126\\u4e00-\\u9fa5]*'; --jp
-- local regular_jp  = [['[^'"\n]*[\u3040-\u3126]+[^'"\n]*']] 		-- c   'jp'
-- local regular_jp2 = [["[^'"\n]*[\u3040-\u3126]+[^'"\n]*"]] 	 	-- c   "jp"
-- local regular_jp3 = [[>[^-><'";]*[\u3040-\u3126]+[^-><'";]*<]]  -- xml >jp<

--util.GetFiles(
--	-- "." -- server
--	-- "./dark-client/Assets/Scripts" -- client
--	-- "./dark-client/Assets/Resources/text/ja"
--	"./tools"
--	-- "dark-client/Assets/workspace/text/ja"
--    , function ( fp )
--        -- TransOne(fp, regular_jp,  'sh')
--        -- TransOne(fp, regular_jp2, 'c')
--        TransOneV2(fp, luaregular_jp, 'c')
--        -- TransOne(fp, regular_jp3, 'xml')
--    end
--	 , "adoc|md|kt|go|java|scala|puml|sh|bash|sql|csv|gradle|yml|js|jsx"
--	--  , "cs|md|adoc|txt"
--)

print("UniqueSrc")
UniqueSrc(db)
-- print("gmsc, ggpc, gslc", gmsc, ggpc, gslc)
assert(db:exec("VACUUM;"))
db:close()

