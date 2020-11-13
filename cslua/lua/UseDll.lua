

local CS = CS
local System = CS.System

local ret = xlua.load_assembly("NPOI.OOXML")
print("load_assembly npoi=", ret)
local et = ret:GetExportedTypes()
-- for i=0, et.Length - 1 do
-- 	print(i, et[i])
-- end

local NPOI = CS.NPOI
local Mono = CS.Mono
local HWorkbook = NPOI.HSSF.UserModel.HSSFWorkbook
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook

as = CS.System.AppDomain.CurrentDomain:GetAssemblies(); for i = 0, as.Length-1 do print(i, as[i])end

-- local util = require "util"
-- local dump = require "dump"

local excelPath = "/Volumes/Data/a3/c/client/Unity/Assets/Application/Resource/Editor/strings/strings.xlsx"
local Workbook = XWorkbook
local wb = Workbook(excelPath)
local sheet = wb:GetSheet("Sheet1") or wb:CreateSheet("Sheet1")
print(wb, sheet)

-- for i=0, sheet.LastRowNum - 1 do
-- 	local row = sheet:GetRow(i) or sheet:CreatRow(o)
-- 	print(row:Cell(0).SValue, row:Cell(1).SValue, row:Cell(2).SValue, row:Cell(3).SValue, "end")
-- end

local sqlite = require "lsqlite3"
local db = sqlite.open("/Volumes/Data/a3/c/client/Unity/Assets/Application/Resource/Editor/stringdb/strings.sqlite3")

local r,e=db:exec("SELECT load_extension('/Volumes/Data/a3/tools/bin/icu_regex_replace.so', 'sqlite3_extension_init') from dicc;")
print("load_extension", r, e)

local c = 0
local head = nil
db:exec("SELECT load_extension('/Volumes/Data/a3/tools/bin/icu_regex_replace.so', 'sqlite3_extension_init'); select id, regex_replace('\n', jpn, '\\n') as jpn from strings", function ( nc, n, values, names )
	if c == 0 then
		head = names
	end
	c = c + 1
	local t = {table.unpack(values)}
	print(table.unpack(t))

	local row = sheet:CreateRow(c)
	for i,v in ipairs(t) do
		local cell = row:CreateCell(i)
		cell:SetCellValue(v)
	end
	
	return sqlite.OK
end)
print(table.unpack(head))

if true then return end

print("saving ... ")
System.IO.File.Delete(excelPath)
local outStream = System.IO.FileStream(excelPath, System.IO.FileMode.CreateNew);
outStream.Position = 0;
wb:Write(outStream);
outStream:Close();

print("all done")
