
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

local bf=CS.Baidu.Fanyi.Do
local GroupBy = CS.xlua.Util.GroupBy
local Select = CS.xlua.Util.Select
local ToList = CS.xlua.Util.ToList
local First = CS.xlua.Util.First

local unpack = unpack or table.unpack

local print = function(...)
    _G.print("codeTrans", ...)
end

local dicdbpath = "/Volumes/Data/a3/c3/client/Unity/Tools/excel/strings.sqlite3"
local db = sqlite3.open(dicdbpath);
local time = os.time()

function sethead(t, h)
	-- print(unpack(h))
	local hh = {}
	for i,v in ipairs(h) do hh[v] = i end
	local mt = {
		__index = function(tt, kk, vv)
			if hh[kk] ~= nil then
				return tt[hh[kk]]
			else
				return nil
			end
		end
	}
	for i, v in ipairs(t) do
		if(type(v) == "table")then
			-- print("set-meta", i, v.id, v[2], v[5])
			setmetatable(v, mt)
		end
	end
end

local function trim( s, t )
    t = t or {"^%s+", "%s+$", "^//*", "^'", "'$", "^\"", "\"$"}
    for i,v in ipairs(t) do
        s = s:gsub(v, "")
        s = s:gsub("^%s+", "")
        s = s:gsub("%s+$", "")
    end
    return s
end

function strip( s )
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
function TransOne( fpath, regular_jp, t )
	local f = io.open(fpath)
	local s = f:read('*a')
	f:close()

	-- local regular_zh = [[.*[\u4e00-\u9fa5]+.*]] -- zh
	-- local regular_jpzh = [[.*[\u3021-\u3126\u4e00-\u9fa5]+.*]] -- zh + jp
	-- local ms = Regex.Matches(s, regular_jp, System.Text.RegularExpressions.RegexOptions.Singleline)
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
	local selectdic = {'select * from dic where s in ('}
	local insertdic = {'insert into dic (s, src, ut) VALUES '} -- no trans jp
	for i=0,ms.Count-1 do
		jp = "'" .. trim(ms[i].Value, trimt) .. "'"
		-- print("ms", i, jp)
		selectdic[1+#selectdic] = '\t' .. jp .. ','
		insertdic[1+#insertdic] = '\t(' .. jp .. ', \'{src="'.. fpath ..'"}\', '..time..'),'
	end
	selectdic[#selectdic] = '\t' .. jp
	selectdic[1+#selectdic] = ') and ((zh NOTNULL and zh <> \'\') or ( tr NOTNULL and tr <> \'\'));'
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
	local err = db:exec(sql, function ( ud, ncols, values, names )
		-- print("check-dic-cb", unpack(values))
		head = names
		local r = {table.unpack(values)}
		dic [1+#dic] = r

		return sqlite3.OK
	end)
	if err ~= sqlite3.OK then print("check dic error", db:errmsg():gsub("\n", "\\n"), sql:gsub("\n", "\\n")) end
	if #dic < 1 then return end
	sethead(dic, head)

	-- trans replace
	for i,v in ipairs(dic) do
		if v.zh == nil or v.zh == '' then v.zh = 'tr:' .. v.tr end
		if v.v and (v.v:match("bdfy") or v.v:match("bf")) then v.zh = "bf:" .. v.zh .. ":fb" end
		-- print('replace', regular_jp, i, v.s, v.zh)
		if t == 'sh' then
			s = CS.cslua.Util.Replace(s, ("'" .. v.s .. "'"), ("'" .. v.zh .. "'"))
		elseif t == 'c' then
			s = CS.cslua.Util.Replace(s, ('"' .. v.s .. '"'), ('"' .. v.zh .. '"'))
		elseif t == 'xml' then
			s = CS.cslua.Util.Replace(s, ( v.s ), ( v.zh ))
		end
	end
	f = io.open(fpath, "w")
	f:write(s)
	f:close()

end

-- local fpath = "/Volumes/Data/a3/s/v210/fuel/appadm/config/menu.php"
-- TransOne(fpath)

local function GetFiles(root, fileAct, filter)
    -- print("GetFiles", root)
    for entry in lfs.dir(root) do
        if entry ~= '.' and entry ~= '..' then
            local traverpath = root .. "/" .. entry
            local attr = lfs.attributes(traverpath)
            if (type(attr) ~= "table") then --如果获取不到属性表则报错
                print('ERROR:' .. traverpath .. 'is not a path')

                goto continue
            end
            -- print(traverpath)
            if (attr.mode == "directory") then
                GetFiles(traverpath, fileAct, filter)
            elseif attr.mode == "file" then
                if fileAct then
                    -- print("filter", filter)
                    if filter ~= nil and type(filter) == "function" then
                        if  filter(traverpath) then fileAct(traverpath) end
                    else
                        fileAct(traverpath)
                    end
                end
            end
        end
        :: continue :: 
    end
end


local regular_jp  = [['[^'"\n]*[\u3040-\u3126]+[^'"\n]*']] 		-- c   'jp'
local regular_jp2 = [["[^'"\n]*[\u3040-\u3126]+[^'"\n]*"]] 	 	-- c   "jp"
local regular_jp3 = [[>[^-><'";]*[\u3040-\u3126]+[^-><'";]*<]]  -- xml >jp<
GetFiles(
    "fuel"
    , function ( fp )
        TransOne(fp, regular_jp,  'sh')
        TransOne(fp, regular_jp2, 'c')
        TransOne(fp, regular_jp3, 'xml')
    end
    , function ( fp )
        if (
              fp:sub(-3) == "php"
        )
        then return true end
        return false
    end
)
print("gmsc, ggpc, gslc", gmsc, ggpc, gslc)
assert(db:exec("VACUUM;"))
db:close()