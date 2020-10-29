--
-- Created by IntelliJ IDEA.
-- User: cn
-- Date: 2019-12-27
-- Time: 10:23
-- To change this template use File | Settings | File Templates.
--

local print = function(...)
    _G.print("main.lua", ...)
end

local CS = CS
local System = CS.System
local NPOI = CS.NPOI
local Mono = CS.Mono
local HWorkbook = NPOI.HSSF.UserModel.HSSFWorkbook
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook

local Environment = System.Environment
package.cpath = package.cpath .. ";" .. System.AppDomain.CurrentDomain.BaseDirectory .. "lib?.dylib;"
print("package.cpath=", package.cpath)
package.path = package.path .. ";" .. System.AppDomain.CurrentDomain.BaseDirectory .. "?.lua;"
	.. Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) .. "/lua/?.lua;"
print("package.path=", package.path)

-- local util = require "util"
local dump = require "dump"
local sqlite = require "lsqlite3"
print("sqlite", sqlite)
local luasql = require "luasql.mysql"
print("luasql", luasql)
local function testmysql()
	local db, err = luasql.mysql():connect("a3_m_305", "a3", "654123", "10.23.22.233")
	if err ~= nil then print(err) return end
	-- local sql = "show tables;"
	local sql = "select s.jp_name, c.* from m_card c left join m_string_item s on c.card_name_id = s.string_id limit 20;"
	local res, err = db:execute(sql)
	if err ~= nil then print(err) return end
	print(table.unpack(res:getcolnames()), res:numrows())
	for i=0,res:numrows()-1 do
		local t = {} -- res:fetch()
		res:fetch(t, "a")
		print(i, dump(t))
		-- print(i, dump(t))
	end
	-- print("close", res:close())
	db:close()
end
testmysql()


-- -- 更新 LuaCallCSharpTypes.cs
-- local LuaCallCSharpTypesSrc = {}
-- local types = CS.ExcelUtil.XLuaConfig.LuaCallCSharp
-- LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = [[
-- using System;
-- using System.Collections.Generic;

-- public class LuaCallCSharpTypes{
--     public static List<Type> L = new List<Type>(){ //]] .. types.Count
-- for i = 0, types.Count - 1 do
--     local n = types[i].FullName
--     if n:match("`") == nil then
--         LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = ("\t\ttypeof(" .. string.gsub(n, '+', '.') .. "),")
--     end
-- end
-- LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = ("\t};")
-- LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = ("}")
-- local src = table.concat(LuaCallCSharpTypesSrc, "\n")
-- --print("src", src)
-- local f = io.open("../ExcelLua/LuaCallCSharpTypes.cs", "w")
-- f:write(src)
-- f:close()


-- types = CS.ExcelUtil.XLuaConfig.CSharpCallLua
-- print("types", types.Count)
-- for i = 0, types.Count - 1 do
--     print("CSharpCallLua", i, types[i].FullName)
-- end

--
--local db = sqlite.open("/Volumes/Data/a3/c3/client/Unity/Assets/Application/Resource/Editor/stringdb/strings.sqlite3");
--print("sqlite3db", db)
--db:close()
--print("sqlite3db", db)

-- -- https://www.sqlitetutorial.net/
-- function sqlitetest()
--     local cmd = db:CreateCommand();
--     cmd.CommandText = "select * from strings_no_trans;";
--     local reader = cmd:ExecuteReader();
--     print(reader:GetName(0), reader:GetName(2), reader:GetName(3), reader:GetName(4), reader:GetName(5));
--     print(reader:GetDataTypeName(0), reader:GetDataTypeName(2), reader:GetDataTypeName(3), reader:GetDataTypeName(4), reader:GetDataTypeName(5));
--     while (reader:Read()) do
--         print(reader:GetInt32(0), reader:GetTextReader(1):ReadToEnd(), reader:GetTextReader(3):ReadToEnd());
--     end
--     reader:Dispose()

--     cmd.CommandText = [[
--        CREATE TABLE IF NOT EXISTS "dic" (
--            "id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
--            "s"	text,
--            "zh"	text,
--            "tr"	text,
--            "src"	text
--        );

--        CREATE UNIQUE INDEX "index_dic" ON "dic" (
--            "s"
--        );
--         -- insert in to dic (s,zh,tr)
--     ]]
--     reader = cmd:ExecuteReader();
-- end

-- 从 Excel 字典并入
-- local CollectExcelDic =  require "CollectExcelDic"
-- -- 从 (jp, tr, src) Excel 导入字典
-- local rootPath = "ExcelData.trans"
-- util.GetFiles(rootPath, function ( path)
--     CollectExcelDic(path, db, "zh", "v110" )
-- end)


-- -- 从差异提取的字典 110
-- local t = require "exceldata-trans-zh-110"
-- local CollectExcelDiff2Dic = require "CollectExcelDiff2Dic"
-- CollectExcelDiff2Dic(t, db, "zh", "v110.0")

-- -- 从差异提取的字典 210
-- local t = require "exceldata-trans-tr-210"
-- local CollectExcelDiff2Dic = require "CollectExcelDiff2Dic"
-- CollectExcelDiff2Dic(t, db, "tr", "v210.0")


-- -- 翻译 Excel
-- local TransExcel = require "TransExcel"
-- local jpRootPath = "ExcelData_c2ios"
-- util.GetFiles(jpRootPath, function ( path )
--     -- print(path)
--     TransExcel(path, db, "zh", "v202")
-- end)



-- local sqlutil = require "sqlutil"
-- local host = "10.23.22.233"

-- --Mysql2Excel(source, user, pward, host, excelPath)
-- sqlutil.Mysql2Excel("a3_350_m", --[[{"strings"}]]nil, "a3", "654123", host, "a3_350_m.xlsx")

-- -- Excel2Sql(source, user, pward, host, excelPath)
-- sqlutil.Excel2Sql("a3_350_string_luatest", "root", "456123", host, "a3_strings_350.strings.xlsx")

function Img2Excel()
	local jpRootPath = "Resource"
	local CollectImg2Excel = require "CollectImg2Excel"
	local i = 1
	local wb = XWorkbook()
	local sheet = wb:CreateSheet()
	sheet.SheetName = "jp-img-350"
	sheet:SetColumnWidth(0, 16 * 1500)
	sheet:SetColumnWidth(1, 16 * 256)
	sheet:SetColumnWidth(2, 16 * 256)
	sheet:SetColumnWidth(3, 16 * 256)
	util.GetFiles(jpRootPath, function ( path )
	    -- print(path)
	    CollectImg2Excel(path, wb, sheet, i)
	    i = 1 + i
	end
	,function ( path )
	    local filter = path:sub(-4) == ".png" 
	        or path:sub(-4) == ".jpg" 
	        or path:sub(-5) == ".jpeg" 
	    -- print(path, filter)
	    return filter
	end)
	print("saving ...")
	System.IO.File.Delete("jp-img-350.xlsx")
	local outStream = System.IO.FileStream("jp-img-350.xlsx", System.IO.FileMode.CreateNew);
	outStream.Position = 0;
	wb:Write(outStream);
	outStream:Close();
end
-- Img2Excel()
