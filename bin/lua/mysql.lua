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
	test = test
}