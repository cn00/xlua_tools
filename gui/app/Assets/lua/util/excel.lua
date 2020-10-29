local CS = CS
local System = CS.System
local File = System.IO.File

local print = function(...)
    _G.print("excel", ...)
end

local function LoadNPIO()
    xlua.load_assembly("NPOI") -- xls
    xlua.load_assembly("NPOI.OOXML") -- xlsx
end

local MaxRowNum = 100000;
local MaxColuNum = 300;


local function OpenExcel( path )
    if  true
        and path:sub(-5) ~= ".xlsx"
        and path:sub(-4) ~= ".xls"
    then
        print("not a excel file", path)
        return
    end

    LoadNPIO()
    local NPOI = CS.NPOI
    local HWorkbook = NPOI.HSSF.UserModel.HSSFWorkbook
    local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook

    local wb
    if      path:sub(-5) == ".xlsx" then
        wb = XWorkbook(path)
    elseif path:sub(-4) == ".xls" then
        local inStream = System.IO.FileStream(path, System.IO.FileMode.Open);
        wb = HWorkbook(inStream)
        inStream:Close()
    end
    return wb
end

local function EnumerateSheetInBook(book, callback)
    if type(book) == "string" then
        if File.Exists(book) then
            book = OpenExcel(book)
        else
            print(book .. " not found")
            return
        end
    end
    
    for sheetidx = 0, book1.NumberOfSheets - 1 do
        local sheet = book:GetSheetAt(sheetidx)
        callback(sheet)
    end
end

local function EnumerateRowInSheet(sheet, callback)
    for i = sheet.FirstRowNum, Math.Min(MaxRowNum, Math.Max(sheet.LastRowNum, sheet.LastRowNum)) do
        local row = sheet:Row(i);
        callback(row)
    end
end

return {
    LoadNPIO = LoadNPIO,
    OpenExcel = OpenExcel,
    EnumerateSheetInBook = EnumerateSheetInBook,
    EnumerateRowInSheet = EnumerateRowInSheet,
}