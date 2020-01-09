local CS = CS
local System = CS.System
local NPOI = CS.NPOI
local Mono = CS.Mono
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook

local util = require "util"
local dump = require "dump"

local luasql = require "luasql.mysql"
-- for k,v in pairs(luasql) do
-- 	print(k,v)
-- end

local function test()
	local conn, err = luasql.mysql():connect("a3_350_u", "a3", "654123", "10.23.22.233")
	if err ~= nil then print(err) return end
	-- local sql = "show tables;"
	local sql = "select s.jp_name, c.* from a3_350_m.m_card c left join a3_350_m.m_string_item s on c.card_name_id = s.string_id limit 20;"
	local res, err = conn:execute(sql)
	if err ~= nil then print(err) return end
	print(dump(res:getcolnames()), res:numrows())
	for i=0,res:numrows()-1 do
		local t = {res:fetch()}
		-- local t = {}
		-- res:fetch(t, "a")
		print(i, res:fetch())
		-- print(i, dump(t))
	end
	-- print("close", res:close())
end

local function Mysql2Excel(source, tables, user, pward, host, excelPath)
	local conn, err = luasql.mysql():connect(source, user, pward, host)
	if err ~= nil then print(err) return end

	if tables == nil then
		tables = {}
		local sql = "show tables;"
		local res, err = conn:execute(sql)
		if err ~= nil then print(err) return end
		for i=0,res:numrows()-1 do
			tables[1+#tables] = res:fetch()
		end
		res:close()
	end
	print(source, dump(tables))

	local wb = XWorkbook()
	for it,tab in ipairs(tables) do
		local sheet = wb:CreateSheet()
		sheet.SheetName = tab
		local sql = "select * from " .. tab ..";"
		local res, err = conn:execute(sql)
		if err ~= nil then print(err) return end

		print(tab, dump(res:getcolnames()), res:numrows())
	    local row = sheet:GetRow(0) or sheet:CreateRow(0)
		for k,v in pairs(res:getcolnames()) do
		    local cell = row:GetCell(k-1) or row:CreateCell(k-1)
			cell:SetCellValue(v)
		end

		for i=0,res:numrows()-1 do
			local t = {res:fetch()}
			local row = sheet:GetRow(i+1) or sheet:CreateRow(i+1)
			for ii, vv in ipairs(t) do
				local cell = row:GetCell(ii-1) or row:CreateCell(ii-1)
				cell:SetCellValue(vv)
			end
		end
		res:close()
	end

	print("saving ...")
	System.IO.File.Delete(excelPath)
	local outStream = System.IO.FileStream(excelPath, System.IO.FileMode.CreateNew);
	outStream.Position = 0;
	wb:Write(outStream);
	outStream:Close();
end

local function Excel2Sql(source, user, pward, host, excelPath)
	local values = {}
	local sql = "create database if not exists " .. source .. ";"
	values[1+#values] = sql

	values[1+#values] = "use ".. source .. ";"

	local wb = XWorkbook(excelPath)
	local tables = {}
	for i=0, wb.NumberOfSheets-1 do
		local sheet = wb:GetSheetAt(i)
		local tab = sheet.SheetName

		local head  = sheet:GetRow(0)
		local keys = {}
		local last
		for ii=0, head.LastCellNum - 1 do
			local c = head:GetCell(ii).SValue
			if c == "id" then
				last = c .. " VARCHAR(32)"
			else
				last = c .. " text"
			end
			keys[1+#keys] = last .. ","
		end
		keys[#keys] = last

		sql = "drop table if exists `" .. tab .. "`; create table `" .. tab .. "` (" 
			.. table.concat(keys, "\n")
			.. ", PRIMARY KEY (`id`)"
			.. ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
		print(sql)

		-- values
		values[1+#values] = sql
		values[1+#values] =  "insert into `" .. tab .. "` values "
		local last
		for ii = 1, sheet.LastRowNum - 1 do
			local row  = sheet:GetRow(ii)
			local vrow = {"("}
			local lastv
			for iii=0, #keys - 1 do
				local cell = row:GetCell(iii) or row:CreateCell(iii)
				lastv = "'" .. cell.SValue:gsub("'", "''") .. "'"
				vrow[1+#vrow] = lastv .. ","
			end
			vrow[#vrow] = lastv .. ")"

			last = table.concat(vrow, '')
			values[1+#values] = last .. ","
		end
		values[#values] = last .. ";"

	end

	local conn, err = assert(luasql.mysql():connect("", user, pward, host))
	if err ~= nil then print("sql err", err) return end
	local sql = table.concat(values, "\n")
	local res, err = assert.(conn:execute(sql))
	-- if err ~= nil then error("\27[31m".. err .. "\27[0m") return end

    local f = io.open(excelPath .. ".sql", "w")
    f:write(sql)
    f:close()
	
end

-- t = {fetch()}
--[[ t =
{
	[1] = "1",
	[2] = "352",
	[3] = "1",
	[4] = "1",
	[5] = "1",
	[6] = "1",
	[7] = "1",
	[8] = "1",
	[9] = "861",
	[10] = "984",
	[11] = "615",
	[12] = "1",
	[13] = "1",
	[14] = "0",
	[15] = "723",
	[16] = "1298",
	[18] = "1",
	[19] = "1",
	[20] = "1",
	[21] = "2016-11-01 00:00:00",
	[22] = "0",
	[23] = "2016-11-21 16:09:18",
	[24] = "2017-07-27 20:04:07",
	[25] = "liber_takahira",
	[26] = "1",
	[27] = "2017-07-28 15:56:38",
	[28] = "liber_takahira",
	[29] = "",
}
]]

-- fetch(t, "a")
--[[ t = 
{
	["variation_no"] = "1",
	["rf_flag"] = "1",
	["rarity"] = "1",
	["comedy"] = "984",
	["up_date"] = "2017-07-27 20:04:07",
	["attr_type"] = "1",
	["skill_adlib_id"] = "1",
	["action"] = "861",
	["album_flag"] = "1",
	["rf_date"] = "2017-07-28 15:56:38",
	["remarks"] = "",
	["rf_admin"] = "liber_takahira",
	["sd_variation_no"] = "1",
	["up_admin"] = "liber_takahira",
	["profile_id"] = "1",
	["in_date"] = "2016-11-21 16:09:18",
	["backstage_name_id"] = "723",
	["serious"] = "615",
	["bloom_group_id"] = "1",
	["chara_id"] = "1",
	["card_id"] = "1",
	["section_id"] = "1",
	["flavor_id"] = "1298",
	["op_date"] = "2016-11-01 00:00:00",
	["card_name_id"] = "352",
	["bloom"] = "0",
	["skill_star_id"] = "1",
	["dl_flag"] = "0",
}
]]

return {
	test = test,
	Mysql2Excel = Mysql2Excel,
	Excel2Sql = Excel2Sql,
}