local CS = CS
local System = CS.System
local Mono = CS.Mono

xlua.load_assembly("NPOI.OOXML")
local NPOI = CS.NPOI
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook

local util = require "util"
local dump = util.dump

local sqlite3 = require "lsqlite3"
local luasql = require "luasql.mysql"
-- for k,v in pairs(luasql) do
-- 	print(k,v)
-- end

local function _MysqlSQL2Sheet(conn, sql, sheet, offset_row)
    
    if offset_row == nil then offset_row = 0 end
    
	local res, err = conn:execute(sql)
	if err ~= nil then print(err) return end

	print("_MysqlSQL2Sheet", dump(res:getcolnames(), false), res:numrows())
	local row = sheet:GetRow(0) or sheet:CreateRow(0)
	for k,v in pairs(res:getcolnames()) do
        --print(k,v)
		local cell = row:GetCell(k-1) or row:CreateCell(k-1)
		cell:SetCellValue(v)
	end

	for i=0,res:numrows()-1 do
		local t = {res:fetch()}
		local row = sheet:GetRow(offset_row+i+1) or sheet:CreateRow(offset_row+i+1)
		for ii, vv in pairs(t) do
			local cell = row:GetCell(ii-1) or row:CreateCell(ii-1)
			cell:SetCellValue(vv)
		end
	end
	res:close()
end

local function MysqlSQL2Excel(source, SheetName, sql, user, pward, host, excelPath)
	local conn, err = luasql.mysql():connect(source, user, pward, host)
	if err ~= nil then print(err) return end

	local wb = XWorkbook()
	local sheet = wb:CreateSheet()
	sheet.SheetName = SheetName
	_MysqlSQL2Sheet(conn, sql, sheet)

	print("saving ...")
	System.IO.File.Delete(excelPath)
	local outStream = System.IO.FileStream(excelPath, System.IO.FileMode.CreateNew);
	outStream.Position = 0;
	wb:Write(outStream);
	outStream:Close();
end

local function Mysql2Excel(source, tables, user, pward, host, excelPath)
	print(user, pward, host, excelPath)
    local conn, err = assert(luasql.mysql():connect(source, user, pward, host))

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
		print("Mysql2Excel", tab)
		local sheet = wb:CreateSheet()
		sheet.SheetName = tab
		local sql = "select * from " .. tab ..";"
		_MysqlSQL2Sheet(conn, sql, sheet)
	end
	conn:close()

	print("saving ...")
	System.IO.File.Delete(excelPath)
	local outStream = System.IO.FileStream(excelPath, System.IO.FileMode.CreateNew);
	outStream.Position = 0;
	wb:Write(outStream);
	outStream:Close();
end

local function _SqliteSQL2Sheet( db, sql, sheet )
	local numrows = 0
	local contents = {}
	local head = nil
	local ok = db:execute(sql, function ( hd, n, vs, ns )
		numrows = numrows + 1
		head = ns
		-- print("callback", hd, n, dump(vs), dump(ns))
		local res =  {table.unpack(vs)}
		contents[1+#contents] = res
		return sqlite3.OK
	end)
	if ok ~= sqlite3.OK then print(ok, db:errmsg(), sql) return end
	print("_SqliteSQL2Sheet", dump(head), numrows)

	-- head
	local row = sheet:GetRow(0) or sheet:CreateRow(0)
	for k,v in pairs(head) do
		local cell = row:GetCell(k-1) or row:CreateCell(k-1)
		cell:SetCellValue(v)
	end

	--content
	for i=0,numrows-1 do
		local t = contents[i+1]
		-- print("numrows", i, dump(t))
		local row = sheet:GetRow(i+1) or sheet:CreateRow(i+1)
		for ii, vv in pairs(t) do
			local cell = row:GetCell(ii-1) or row:CreateCell(ii-1)
			cell:SetCellValue(vv)
		end
	end
end

local function SqliteSQL2Excel(dbpath, SheetName, sql, excelPath )
	local db, err = sqlite3.open(dbpath)
	if(err ~= sqlite3.OK)then 
		print("open error", db:errmsg()) 
		return 
	end
	local wb = XWorkbook()
	local sheet = wb:CreateSheet()
	sheet.SheetName = SheetName
	_SqliteSQL2Sheet( db, sql, sheet )

	print("saving ...")
	System.IO.File.Delete(excelPath)
	local outStream = System.IO.FileStream(excelPath, System.IO.FileMode.CreateNew);
	outStream.Position = 0;
	wb:Write(outStream);
	outStream:Close();
	print("done")
end

local function Sqlite2Excel(dbpath, tables, excelPath )
	local db, err = sqlite3.open(dbpath)
	if(err ~= sqlite3.OK)then 
		print("open error", db:errmsg()) 
		return 
	end

	if tables == nil then
		print("export all tables")
		tables = {}
		local sql = 'select name from sqlite_master where type = "table";'
		local ok = db:execute(sql, function (hd, n, vs, ns )
			local res = {table.unpack(vs)}
			tables[1+#tables] = res[1]
			return sqlite3.OK
		end)
		if err ~= sqlite3.OK then print(db:errmsg()) return end
	end
	print("Sqlite2Excel", dump(tables))

	local wb = XWorkbook()
	for it,tab in ipairs(tables) do
		local sheet = wb:CreateSheet()
		sheet.SheetName = tab
		local sql = "select * from " .. tab ..";"
		_SqliteSQL2Sheet( db, sql, sheet )
	end

	print("saving ...")
	System.IO.File.Delete(excelPath)
	local outStream = System.IO.FileStream(excelPath, System.IO.FileMode.CreateNew);
	outStream.Position = 0;
	wb:Write(outStream);
	outStream:Close();
	print("done")
end

local function Excel2Sql(source, user, pward, host, excelPath)
	local values = {}
	local sql = "create database if not exists `" .. source .. "`;"
	values[1+#values] = sql

	values[1+#values] = "use `".. source .. "`;"

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
		keys[1+#keys] = "PRIMARY KEY (`id`)"

		values[1+#values] = "drop table if exists `" .. tab .. "`;"
		sql = "create table `" .. tab .. "` (\n\t" 
			.. table.concat(keys, "\n\t")
			.. "\n) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;"
		print(sql)
		values[1+#values] = sql

		-- values
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

	local sql = table.concat(values, "\n")
    local f = io.open(excelPath .. ".sql", "w")
    f:write(sql)
    f:close()

	local conn, err = assert(luasql.mysql():connect("", user, pward, host))
	if err ~= nil then print("sql err", err) return end
	local res, err = assert(conn:execute(sql))
	-- if err ~= nil then error("\27[31m".. err .. "\27[0m") return end
	
end


local function testMysql()
    local conn, err = luasql.mysql():connect("a3_350_u", "a3", "654123", "10.23.24.239")
    if err ~= nil then print(err) return end
    -- local sql = "show tables;"
    local sql = "select s.jp_name, c.* from a3_350_m.m_card c left join a3_350_m.m_string_item s on c.card_name_id = s.string_id limit 20;"
    local res, err = conn:execute(sql)
    if err ~= nil then print(err) return end
    print(dump(res:getcolnames()), res:numrows())
    for i=0,res:numrows()-1 do
        local t = {res:fetch()}
        -- t = {fetch()}
        --[[ t =
        {
            "1",
            "352",
            "1",
            ...
        }
        ]]

        -- t = fetch(t, "a")
        --[[ t = 
        {
            ["variation_no"] = "1",
            ["rf_flag"] = "1",
            ["rarity"] = "1",
            ...
        }
        ]]
        -- local t = {}
        -- res:fetch(t, "a")
        print(i, res:fetch())
        -- print(i, dump(t))
    end
    -- print("close", res:close())
end

-------------------------------------
return {
    testMysql = testMysql,
	Sqlite2Excel = Sqlite2Excel,
	SqliteSQL2Excel = SqliteSQL2Excel,
	Mysql2Excel = Mysql2Excel,
	MysqlSQL2Excel = MysqlSQL2Excel,
	Excel2Sql = Excel2Sql,
}