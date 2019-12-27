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
local NPOI = CS.NPOI
local Workbook = NPOI.XSSF.UserModel.XSSFWorkbook

--for k, v in ipairs(Workbook) do
--    print(k, v)
--end

local types = CS.ExcelUtil.XLuaConfig.LuaCallCSharp:ToList()
print("types", types.Count)
for i = 0, types.Count - 1 do
    print(i, types[i].FullName)
end

local wb = Workbook("Master.xlsx")
print ('wb', wb) 
local sheet = wb:GetSheet('jp')
print ("sheet", sheet)

for i = 1, math.min(sheet.LastRowNum, 10000) do
    local row = sheet:GetRow(i)
    if row ~= nil then
        for j = 0, row.LastCellNum - 1 do
            local cell = row:GetCell(j)
            if cell ~= nil then 
                print(i, j, NPOI.ExcelExtension.SValue(row:GetCell(j)))
            end
        end
    end
end