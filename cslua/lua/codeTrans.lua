
local CS = CS
local System = CS.System
local NPOI = CS.NPOI
local Mono = CS.Mono
local HWorkbook = NPOI.HSSF.UserModel.HSSFWorkbook
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook
local Regex = System.Text.RegularExpressions.Regex
-- local util = require "util"
-- local dump = require "dump"
local sqlite3 = require "lsqlite3"
local lfs = require "lfs"
local util = require "util"
local table = require("util.linq")

local bf=CS.Baidu.Fanyi.Do
local GroupBy = CS.xlua.Util.GroupBy
local Select = CS.xlua.Util.Select
local ToList = CS.xlua.Util.ToList
local First = CS.xlua.Util.First

local unpack = unpack or table.unpack

local print = function(...)
    _G.print("codeTrans", ...)
end

local dicdbpath = "strings-all.sqlite3"
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
local function TransOne( fpath, regular_jp, t )
	local f = io.open(fpath)
	local s = f:read('*a')
	f:close()

	local ms = Regex.Matches(s, regular_jp, System.Text.RegularExpressions.RegexOptions.Compiled)
	if ms.Count < 1 then return end
	-- print("ms", ms, ms.Count)
	local gp = GroupBy(ms, function(i) 
		-- print("msi", i.Value) 
		return i.Value 
	end)
	-- print("gp", gp, gp.Count)
	local sl = Select(gp, function(g) 
		-- print("gps", First(g).Value) 
		return First(g) 
	end)
	-- print("sl", sl, sl.Count)
	print("ms-gp-sl", ms.Count, gp.Count, sl.Count)
	gmsc, ggpc, gslc = gmsc + ms.Count, ggpc + gp.Count, gslc + sl.Count

	ms = sl
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
	for i=0,ms.Count-1 do
		jp = "'" .. trim(ms[i].Value, trimt) .. "'"
		-- print("ms", i, jp)
		selectdic[1+#selectdic] = '\t' .. jp .. ','
		insertdic[1+#insertdic] = '\t(' .. jp .. ', \'{src="'.. fpath ..'"}\', '..time..'),'
	end
	selectdic[#selectdic] = '\t' .. jp
	selectdic[1+#selectdic] = ') and ((zh NOTNULL and zh <> \'\') or ( tr NOTNULL and tr <> \'\'))) ORDER BY length(s) desc;'
	insertdic[#insertdic] = '\t(' .. jp .. ', \'{src="'.. fpath ..'"}\', '..time..')'
	local language = "s"
	insertdic[1+#insertdic] = "ON CONFLICT(s) DO UPDATE SET \n\t"
		..language.." = CASE WHEN "..language.." ISNULL OR "..language.." = '' THEN excluded."..language.. " ELSE " .. language .. " END"
		.."\n\t, src = excluded.src||','||char(13)||src"
		-- .." WHERE "..language.." <> excluded."..language
		.. ";"

	lfs.mkdir("tmp-sel-sql")
	lfs.mkdir("tmp-ins-sql")
	local sql = table.concat( insertdic, "\n")
	local f = io.open("tmp-ins-sql/" .. fpath:gsub("/", "_") .. "-insert.sql", "w")
	f:write(sql)
	f:close()
	local err = db:exec(sql)
	if err ~= sqlite3.OK then print("insert err", db:errmsg():gsub("\n", "\\n"), sql:gsub("\n", "\\n")) end

	local sql = table.concat( selectdic, "\n")
	local f = io.open("tmp-sel-sql/" .. fpath:gsub("/", "_") .. "-select.sql", "w")
	f:write(sql)
	f:close()

	print('--->', fpath)
	-- if true then return end

	-- check dic
	local dic = {}
	local head = nil
	for row in db:nrows(sql) do
		dic [1+#dic] = row
	end
	if #dic < 1 then
		print("no trans find.")
		return
	end
	--sethead(dic, head)

	-- trans replace
	for i,v in ipairs(dic) do
		if v.zh == nil or v.zh == '' then v.zh = 'tr:' .. v.tr end
		if v.v and (v.v:match("bdfy") or v.v:match("bf")) then v.zh = "<bf:" .. v.zh .. ":fb>" end
		 print('replace', regular_jp, i, v.s, v.zh)
		if t == 'sh' then
			s = CS.xlua.Util.Replace(s, ("'" .. v.s .. "'"), ("'" .. v.zh .. "'"))
		elseif t == 'c' then
			--s = CS.xlua.Util.Replace(s, ('"' .. v.s .. '"'), ('"' .. v.zh .. '"'))
			s = CS.xlua.Util.Replace(s, v.s, v.zh)
		elseif t == 'xml' then
			s = CS.xlua.Util.Replace(s, ( v.s ), ( v.zh ))
		end
	end
	f = io.open(fpath, "w")
	f:write(s)
	f:close()

end


local function TransOneV2( fpath, regular_jp, t )
	local f = io.open(fpath)
	local s = f:read('*a')
	f:close()

	local ms = string.gmatch(s, regular_jp)
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
	for si in ms do
		--jp = "'" .. trim(si, trimt) .. "'"
		jp = "'" .. si .. "'"
		-- print("ms", i, jp)
		selectdic[1+#selectdic] = '\t' .. jp .. ','
		insertdic[1+#insertdic] = '\t(' .. jp .. ', \'{src="'.. fpath ..'"}\', '..time..'),'
	end
	if #selectdic < 2 and #insertdic < 2 then return end

	selectdic[#selectdic] = '\t' .. jp
	selectdic[1+#selectdic] = ') and ((zh NOTNULL and zh <> \'\') or ( tr NOTNULL and tr <> \'\'))) ORDER BY length(s) desc;'
	insertdic[#insertdic] = '\t(' .. jp .. ', \'{src="'.. fpath ..'"}\', '..time..')'
	local language = "s"
	insertdic[1+#insertdic] = "ON CONFLICT(s) DO UPDATE SET \n\t"
			..language.." = CASE WHEN "..language.." ISNULL OR "..language.." = '' THEN excluded."..language.. " ELSE " .. language .. " END"
			.."\n\t, src = excluded.src||','||char(13)||src"
			-- .." WHERE "..language.." <> excluded."..language
			.. ";"

	lfs.mkdir("tmp-sel-sql")
	lfs.mkdir("tmp-ins-sql")
	local sql = table.concat( insertdic, "\n")
	local f = io.open("tmp-ins-sql/" .. fpath:gsub("/", "_") .. "-insert.sql", "w")
	f:write(sql)
	f:close()
	local err = db:exec(sql)
	if err ~= sqlite3.OK then print("insert err", db:errmsg():gsub("\n", "\\n"), sql:gsub("\n", "\\n")) end

	local sql = table.concat( selectdic, "\n")
	local f = io.open("tmp-sel-sql/" .. fpath:gsub("/", "_") .. "-select.sql", "w")
	f:write(sql)
	f:close()

	print('--->', fpath)
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
		if v.zh == nil or v.zh == '' then v.zh = 'tr:' .. v.tr end
		if v.v and (v.v:match("bdfy") or v.v:match("bf")) then v.zh = "<bf:" .. v.zh .. ":fb>" end
		print('replace', regular_jp, i, v.s, v.zh)
		if t == 'sh' then
			s = CS.xlua.Util.Replace(s, ("'" .. v.s .. "'"), ("'" .. v.zh .. "'"))
		elseif t == 'c' then
			--s = CS.xlua.Util.Replace(s, ('"' .. v.s .. '"'), ('"' .. v.zh .. '"'))
			s = CS.xlua.Util.Replace(s, v.s, v.zh)
		elseif t == 'xml' then
			s = CS.xlua.Util.Replace(s, ( v.s ), ( v.zh ))
		end
	end
	f = io.open(fpath, "w")
	f:write(s)
	f:close()

end

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
local luaregular_jp   = '[^\0-\127]*\227[\128-\132][\128-\191][^\0-\127]*' -- 包含一个日文字母 \227[\128-\132][\128-\191]
--local luaregular_jpzh = '[\0-\127\194-\253][\128-\191]+[\0-\253]*'

--local regular_jpzh= '[\\u3021-\\u3126\\u4e00-\\u9fa5]+'; --jp+zh

local csregular_jpzh= '[\\u4e00-\\u9fa5]*[\\u3021-\\u3126]+[\\u3021-\\u3126\\u4e00-\\u9fa5]*'; --jp
-- local regular_jp  = [['[^'"\n]*[\u3040-\u3126]+[^'"\n]*']] 		-- c   'jp'
-- local regular_jp2 = [["[^'"\n]*[\u3040-\u3126]+[^'"\n]*"]] 	 	-- c   "jp"
-- local regular_jp3 = [[>[^-><'";]*[\u3040-\u3126]+[^-><'";]*<]]  -- xml >jp<
util.GetFiles(
    "."
    , function ( fp )
        -- TransOne(fp, regular_jp,  'sh')
        -- TransOne(fp, regular_jp2, 'c')
        TransOneV2(fp, luaregular_jp, 'c')
        -- TransOne(fp, regular_jp3, 'xml')
    end
	 , "md|kt|go|java|scala|puml"
	--, "puml"
)
print("gmsc, ggpc, gslc", gmsc, ggpc, gslc)
assert(db:exec("VACUUM;"))
db:close()