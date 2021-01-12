local CS = CS
local System = CS.System
local StringBuilder = System.Text.StringBuilder

local util = require "lua.util"
local excel = require "lua.util.excel"
local dump = require "lua.dump"

local TotalCellCount = 0;
local DiffCellCount = 0;
local MaxRowNum = 100000;
local MaxColuNum = 300;

local Math = math
Math.Min = Math.min
Math.Max = Math.max

local function quites(s)
    return s and s:gsub("\"", "\\\""):gsub("\n", "\\n"):gsub("\r", "\\r")
end

local function Diff(filePath1, filePath2)
    local book1 = util.OpenExcel(filePath1)
    if book1 == nil then print("-- left file no exist", filePath1); return end
    
    local book2 = util.OpenExcel(filePath2)
    if book2 == nil then print("-- right file no exist", filePath2); return end
    
    print("--", filePath1, filePath2)
    
    local allcollect = {booksummory = {}}
    local bookcellcount = 0
    local bookdiffcount = 0
    local sheetcount = 0
    for sheetidx = 0, book1.NumberOfSheets - 1 do
        local collect = {
            summory = {},
            head = {},
            cells = {},
        }
        local diffcells = collect.cells
        
        local sheetL = book1:GetSheetAt(sheetidx)
        local sheetR = book2:GetSheet(sheetL.SheetName)
        if (sheetR == null) then 
            diffcells[1+#diffcells] = sheetL.FirstRowNum .. " not exists in right excel" 
            allcollect[1+#allcollect] = collect 
            goto continue 
        end
        sheetcount = 1 + sheetcount;

        -- head
        local headL = sheetL:Row(sheetL.FirstRowNum);
        local headt = {}
        collect.head = headt
        for j = headL.FirstCellNum, Math.Min(headL.LastCellNum, MaxColuNum) do
            headt[1+#headt] = quites(headL:Cell(j).SValue)
        end

        -- body
        local sheetcellcount = 0;
        local sheetdiffcount = 0

        -- rows
        for i = sheetL.FirstRowNum, Math.Min(MaxRowNum, Math.Max(sheetL.LastRowNum, sheetR.LastRowNum)) do
            local rowL = sheetL:Row(i);
            local rowR = sheetR:Row(i);
            for j = headL.FirstCellNum,Math.Min(headL.LastCellNum, MaxColuNum) do
                sheetcellcount = 1 + sheetcellcount;
                local cL = rowL:Cell(j);
                local cR = rowR:Cell(j);
                local vL = cL.SValue;
                local vR = cR.SValue;

                if(vL ~= vR)then
                    --print(sheetL.SheetName, i, j, "\n", quites(vL), "\n", quites(vR))
                    sheetdiffcount = 1 + sheetdiffcount;
                    diffcells[1+#diffcells] = {xyh ={j+1, i+1, headt[j+1]}, a = quites(vL), b = quites(vR)}
                end
            end

        end -- rows
        bookdiffcount = bookdiffcount + sheetdiffcount
        bookcellcount = bookcellcount + sheetcellcount
        
        if(#diffcells > 0)then
            collect.summory = {SheetName = sheetL.SheetName, compared = sheetcellcount, diffcount = sheetdiffcount}
            allcollect[1+#allcollect] = collect
            --allcollect[sheetL.SheetName] = collect
        end
        ::continue::
    end -- sheetidx
    allcollect.booksummory = {
        a = filePath1,
        b = filePath2,
        compared = bookcellcount, 
        diffcount = bookdiffcount,
    }
    print("]]--")
    if bookdiffcount > 0 then print(dump(allcollect), ",") end
    print("--[[")
end
Diff(argv[1], argv[2])
