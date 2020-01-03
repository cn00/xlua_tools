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

-- sqlitetest()

function CollectOneExcel(path)
    if(path:sub(-5) == ".xlsx" and nil == path:match("~") )then return end

    print("CollectOne",path)
    local wb = Workbook(path)
    local sheet = wb:GetSheet('jp')
    local values = {
        "INSERT OR IGNORE INTO dic (s,zh,src) VALUES "
    }
    local currentRow
    for i = 1, math.min(sheet.LastRowNum, 10000) do
        local row = sheet:GetRow(i)
        if row ~= nil then
            -- for j = 0, row.LastCellNum - 1 do
            --     local cell = row:GetCell(j)
            --     if cell ~= nil then 
            --         print(i, j, row:GetCell(j).SValue)
            --     end
            -- end
            local cjp = row:GetCell(0)
            if cjp == nil then goto continue end
            local jp = cjp.SValue
            jp=jp:gsub("'", "''")

            local czh = row:GetCell(1)
            if czh == nil then goto continue end
            local zh = czh.SValue
            zh=zh:gsub("'", "''")
            if zh ~= '译文' and zh ~= '' then
                currentRow= "(" 
                    .. "'".. jp .."'," -- jp
                    .. "'".. zh .."'," -- zh
                    .. "'".. path .. ":" ..row:GetCell(5).SValue.."'" -- src
                    ..")"
                values[1+#values] = currentRow .. ","
            else
                print(jp:gsub("\n", "\\n"), zh)
            end
        end
        ::continue::
    end
    values[#values] = currentRow .. ";"
    -- values[1+#values] = "COMMIT;"

    local sql = table.concat(values, "\n")
    local cmd = db:CreateCommand();
    cmd.CommandText = sql;
    local reader = cmd:ExecuteReader();
    reader:Dispose()


    local f = io.open(path .. ".sql", "w")
    f:write(sql)
    f:close()
end
-- CollectOne("ExcelData.trans/Story/mini/StoryMini006_001_010.xlsx")

local dump = require "dump"
local rootPath = "ExcelData.trans"
local util = require "util"
-- util.GetFiles(rootPath, CollectOneExcel)

-- -- 从差异提取的字典
-- local t = require "exceldata-diff-210"
-- local CollectExcelDiffDic = require "CollectExcelDiffDic"
-- CollectExcelDiffDic(t, db, "tr", "v210.0")


-- 翻译
local TransExcel = require "TransExcel"
local jpRootPath = "ExcelData.trans/Digest"
util.GetFiles(jpRootPath, function ( path )
    TransExcel(path, db, "zh")
end)

db:Close()