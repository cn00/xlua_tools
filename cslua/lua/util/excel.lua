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


---OpenExcel
---@param path string
---@return IWorkbook
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

---EnumerateSheetInBook
---@param book IWorkbook
---@param callback function
local function EnumerateSheetInBook(book, callback)
    if type(book) == "string" then
        if File.Exists(book) then
            book = OpenExcel(book)
        else
            print(book .. " not found")
            return
        end
    end
    
    for sheetidx = 0, book.NumberOfSheets - 1 do
        local sheet = book:GetSheetAt(sheetidx)
        callback(sheet)
    end
end

---EnumerateRowInSheet
---@param sheet ISheet
---@param firstRow number
---@param callback function
local function EnumerateRowInSheet(sheet, callback, firstRow, lastRow)
    if firstRow == nil then firstRow = sheet.FirstRowNum end
    if lastRow == nil then lastRow = math.min(MaxRowNum, sheet.LastRowNum-1) end
    for i = firstRow, lastRow do
        local row = sheet:GetRow(i);
        if row == nil then goto continue end
        callback(i, row)
        ::continue::
    end
end

local function sethead(t, h)
    --print(table.unpack(h))
    local hh = {}
    for i,v in pairs(h) do hh[v] = i end
    --print("head", dump(hh, false))
    local mt = {
        --__gc = function ( o )
        --    --print("__gc#o:", #o)
        --end,
        __index = function(tt, kk, vv)
            if hh[kk] ~= nil then
                return tt[hh[kk]]
            else
                return nil
            end
        end
    }
    for i, v in ipairs(t) do
        if(type(v) == "table")then
            -- print("set-meta", i, v.id, v[2], v[5])
            setmetatable(v, mt)
        end
    end
end

local function GetRowAsTable(row, first, last)
    if first == nil then first = row.FirstCellNum end
    if last  == nil then last  = row.LastCellNum end
    if first < 0 then first = 0 end
    if last < 0 then last = 0 end
    if last > 255 then last = 255 end
    local t = {}
    for i = first, last do
        t[1+#t] = string.gsub(row:Cell(i).SValue, "'", "''") or ""
    end
    return t
end


---GetSheetAsTable
---@param sheet ISheet
---@param first number
---@param last number
local function GetSheetAsTable(sheet, first, last)
    if first == nil then first = sheet.FirstRowNum end
    if last  == nil then last  = sheet.LastRowNum end
    if first < 0 then first = 0 end
    if last < 0 then last = 0 end
    if last > 2000 then last = 2000 end
    local head = GetRowAsTable(sheet:Row(sheet.FirstRowNum))
    local content = {}
    for i = first, last do
        local row = sheet:GetRow(i)
        if row == nil then goto continue end
        local rowt = GetRowAsTable(row)
        content[1+#content] = rowt
        ::continue::
    end
    sethead(content, head)
    return content
end

return {
    LoadNPIO = LoadNPIO,
    OpenExcel = OpenExcel,
    EnumerateSheetInBook = EnumerateSheetInBook,
    EnumerateRowInSheet = EnumerateRowInSheet,
    GetSheetAsTable = GetSheetAsTable,
}