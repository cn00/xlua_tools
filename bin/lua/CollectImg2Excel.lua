
local CS = CS
local System = CS.System
local NPOI = CS.NPOI
local Mono = CS.Mono
local HWorkbook = NPOI.HSSF.UserModel.HSSFWorkbook
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook

local util = require "util"
local lfs = require "lfs"

local ClientAnchor, RichTextString

--[[
find Resource -name "*.png" | while read l; do i=$(file "$l" |awk -F '[, ]' '{print $6}'); if (($i>=1024));then echo "$i == $l"; magick convert "$l" -scale $((96000/$i))% "$l"; file "$l";  fi; done
]]
local function CollectImg2Excel(path, wb, sheet, i)
    print(i, path)
    local stream =  System.IO.FileStream(path, System.IO.FileMode.Open);
    local size = NPOI.SS.Util.ImageUtils.GetImageDimension(stream)
    local row = sheet:GetRow(i) or sheet:CreateRow(i)
    row.Height = size.Height * 16
    print('', size)
    sheet:SetColumnWidth(0, math.max(size.Width * 32,16 * 1500))

    local icell = 0
    local cell = row:GetCell(icell) or row:CreateCell(icell)

    local imgType = 6 -- NPOI.SS.UserModel.PictureType.PNG
    local picInd = wb:AddPicture(stream, imgType)

    local helper = wb:GetCreationHelper();
    local drawing = sheet:CreateDrawingPatriarch()
    local anchor = helper:CreateClientAnchor()
    anchor.Col1 = 0;
    anchor.Col2 = 0;
    anchor.Row1 = i;
    local pict = drawing:CreatePicture(anchor, picInd)
    pict:Resize();

    -- if cell.CellComment == nil then
    --     local patriarch = sheet:CreateDrawingPatriarch()
    --     local anchor = ClientAnchor()
    --     local comment = patriarch:CreateCellComment(anchor)
    --     comment.String = RichTextString(str)
    --     cell.CellComment = comment
    --     print("comment", cell, anchor, str)
    -- end

    icell = icell + 1
    cell = row:GetCell(icell) or row:CreateCell(icell)
    icell = icell + 1
    cell = row:GetCell(icell) or row:CreateCell(icell)
    cell:SetCellValue(path)
end

return CollectImg2Excel
