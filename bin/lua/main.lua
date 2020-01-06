--
-- Created by IntelliJ IDEA.
-- User: cn
-- Date: 2019-12-27
-- Time: 10:23
-- To change this template use File | Settings | File Templates.
--

local CS = CS
local System = CS.System
local NPOI = CS.NPOI
local Mono = CS.Mono
local Workbook = NPOI.XSSF.UserModel.XSSFWorkbook


local print = function(...)
    _G.print("main.lua", ...)
end

-- 更新 LuaCallCSharpTypes.cs
local LuaCallCSharpTypesSrc = {}
local types = CS.ExcelUtil.XLuaConfig.LuaCallCSharp
LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = [[
using System;
using System.Collections.Generic;

public class LuaCallCSharpTypes{
    public static List<Type> L = new List<Type>(){ //]] .. types.Count
for i = 0, types.Count - 1 do
    local n = types[i].FullName
    if n:match("`") == nil then
        LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = ("\t\ttypeof(" .. string.gsub(n, '+', '.') .. "),")
    end
end
LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = ("\t};")
LuaCallCSharpTypesSrc[1 + #LuaCallCSharpTypesSrc] = ("}")
local src = table.concat(LuaCallCSharpTypesSrc, "\n")
--print("src", src)
local f = io.open("../ExcelLua/LuaCallCSharpTypes.cs", "w")
f:write(src)
f:close()


-- types = CS.ExcelUtil.XLuaConfig.CSharpCallLua
-- print("types", types.Count)
-- for i = 0, types.Count - 1 do
--     print("CSharpCallLua", i, types[i].FullName)
-- end


local db = Mono.Data.Sqlite.SqliteConnection("URI=file:strings.sqlite3;version=3");
db:Open();


function sqlitetest()
    local cmd = db:CreateCommand();
    cmd.CommandText = "select * from strings_no_trans;";
    local reader = cmd:ExecuteReader();
    print(reader:GetName(0), reader:GetName(2), reader:GetName(3), reader:GetName(4), reader:GetName(5));
    print(reader:GetDataTypeName(0), reader:GetDataTypeName(2), reader:GetDataTypeName(3), reader:GetDataTypeName(4), reader:GetDataTypeName(5));
    while (reader:Read()) do
        print(reader:GetInt32(0), reader:GetTextReader(1):ReadToEnd(), reader:GetTextReader(3):ReadToEnd());
    end
    reader:Dispose()

    cmd.CommandText = [[
       CREATE TABLE IF NOT EXISTS "dic" (
           "id"	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
           "s"	text,
           "zh"	text,
           "tr"	text,
           "src"	text
       );

       CREATE UNIQUE INDEX "index_dic" ON "dic" (
           "s"
       );
        -- insert in to dic (s,zh,tr)
    ]]
    reader = cmd:ExecuteReader();
end

-- 从 Excel 字典并入
-- local CollectExcelDic =  require "CollectExcelDic"
-- -- 从 (jp, tr, src) Excel 导入字典
-- local dump = require "dump"
-- local rootPath = "ExcelData.trans"
-- local util = require "util"
-- util.GetFiles(rootPath, function ( path)
--     CollectExcelDic(path, db, "zh", "v110" )
-- end)


-- -- 从差异提取的字典 110
-- local t = require "exceldata-trans-zh-110"
-- local CollectExcelDiff2Dic = require "CollectExcelDiff2Dic"
-- CollectExcelDiff2Dic(t, db, "zh", "v110.0")

-- 从差异提取的字典 210
local t = require "exceldata-trans-tr-210"
local CollectExcelDiff2Dic = require "CollectExcelDiff2Dic"
CollectExcelDiff2Dic(t, db, "tr", "v210.0")


-- -- 翻译
-- local TransExcel = require "TransExcel"
-- local jpRootPath = "ExcelData.trans/Digest"
-- util.GetFiles(jpRootPath, function ( path )
--     TransExcel(path, db, "zh")
-- end)

db:Close()