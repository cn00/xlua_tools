
local CS = CS
local System = CS.System
local util = require "util"
-- local lfs = require "lfs"
util.loadNPIO()
local NPOI = CS.NPOI
local Mono = CS.Mono
local HWorkbook = NPOI.HSSF.UserModel.HSSFWorkbook
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook


local ClientAnchor, RichTextString

--[[ shell
find Resource -name "*.png" | while read l; do 
    i=$(file "$l" | awk -F '[, ]' '{print $6}'); 
    if (($i>=1024));then 
        echo "$i == $l"; 
        magick convert "$l" -scale $((96000/$i))% "$l"; 
        file "$l";
    fi;
done
]]
local function CollectImg2Excel(path, wb, sheet, i)
    local stream =  System.IO.FileStream(path, System.IO.FileMode.Open);
    print(stream, path)
    local size = NPOI.SS.Util.ImageUtils.GetImageDimension(stream)
    local row = sheet:GetRow(i) or sheet:CreateRow(i)
    row.Height = size.Height * 16
    print(i, size, path, stream)
    sheet:SetColumnWidth(0, math.max(size.Width * 32,16 * 1500))

    local icell = 0
    local cell = row:GetCell(icell) or row:CreateCell(icell)

    local imgType = 6 -- NPOI.SS.UserModel.PictureType.PNG
    local picInd = wb:AddPicture(stream, imgType)

    local helper = wb:GetCreationHelper();
    local drawing = sheet:CreateDrawingPatriarch()
    local anchor = NPOI.XSSF.UserModel.XSSFClientAnchor(500, 200, 0, 0, 2, 2, 4, 7)
    anchor.Col1 = 0;
    anchor.Col2 = 1;
    anchor.Row1 = i;
    anchor.Row2 = i+1;
    local pict = drawing:CreatePicture(anchor, picInd)
    pict.LineStyle = 8 -- LineStyle.DashDotGel
     pict:Resize(); --//Note: Resize will reset client anchor you set.

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
function Img2Excel(jpRootPath)
    jpRootPath = jpRootPath or "/Users/cn/dark/client/Assets"
    -- local CollectImg2Excel = require "CollectImg2Excel"
    local i = 1
    local wb = XWorkbook()
    local sheet = wb:CreateSheet()
    sheet.SheetName = "dark-client-img-jp"
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
    --System.IO.File.Delete("dark-client-img-jp-1.2.1.xlsx")
    local outStream = System.IO.FileStream("dark-client-img-jp-1.2.1"..os.date("%Y%m%d%H%M%S")..".xlsx", System.IO.FileMode.CreateNew);
    outStream.Position = 0;
    wb:Write(outStream);
    outStream:Close();
end
Img2Excel()

return CollectImg2Excel
