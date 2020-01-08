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
	local conn = luasql.mysql():connect("a3_350_u", "a3", "654123", "10.23.22.233")
	-- local sql = "show tables;"
	local sql = "select s.jp_name, c.* from a3_350_m.m_card c left join a3_350_m.m_string_item s on c.card_name_id = s.string_id limit 20;"
	local res = conn:execute(sql)
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
	local conn = luasql.mysql():connect(source, user, pward, host)
	local wb = XWorkbook()

	if tables == nil then
		tables = {}
		local sql = "show tables;"
		local res = conn:execute(sql)
		for i=0,res:numrows()-1 do
			tables[1+#tables] = res:fetch()
		end
		res:close()
	end
	print(source, dump(tables))

	for it,tab in ipairs(tables) do
		local sheet = wb:CreateSheet()
		sheet.SheetName = tab
		local sql = "select * from " .. tab ..";"
		local res = conn:execute(sql)
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

-- t = {fetch()}
--[[
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
--[[
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
}