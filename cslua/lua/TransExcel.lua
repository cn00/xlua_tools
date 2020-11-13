
local CS = CS
local System = CS.System
local NPOI = CS.NPOI
local Mono = CS.Mono
local HWorkbook = NPOI.HSSF.UserModel.HSSFWorkbook
local XWorkbook = NPOI.XSSF.UserModel.XSSFWorkbook

local util = require "util"
local lfs = require "lfs"

local ClientAnchor, RichTextString

local function TransOneExcel(path, dicdb, language, version)
    if  true
    	and path:sub(-5) ~= ".xlsx" 
    	and path:sub(-4) ~= ".xls" 
    	or nil ~= path:match("~") 
    	or dicdb == nil 
    then return end

    language = language or "zh"
    version =  version or "v110"

    print("TransOne",path)
    local wb
    if      path:sub(-5) == ".xlsx" then
        ClientAnchor = NPOI.XSSF.UserModel.XSSFClientAnchor
        RichTextString = NPOI.XSSF.UserModel.XSSFRichTextString

    	wb = XWorkbook(path)
    elseif path:sub(-4) == ".xls" then
        ClientAnchor = NPOI.HSSF.UserModel.HSSFClientAnchor
        RichTextString = NPOI.HSSF.UserModel.HSSFRichTextString

    	local inStream = System.IO.FileStream(path, System.IO.FileMode.Open);
    	wb = HWorkbook(inStream)
    	inStream:Close()
    end

    local count = 0
    local cmd = dicdb:CreateCommand();
    local values = {"SELECT s, "..language.." FROM dic WHERE s in ("}
    local lastJpStr
    local cellCach = {
    	--[[
	    	[s] = {cell, ...},
    	]]
    }
    for i=0,wb.NumberOfSheets - 1 do
	    local sheet = wb:GetSheetAt(i)
	    for ii = sheet.FirstRowNum, math.min(sheet.LastRowNum, sheet.FirstRowNum+10000) do
	        local row = sheet:GetRow(ii)
	        if row ~= nil then
	            for jjj = row.FirstCellNum, math.min(row.LastCellNum - 1, row.FirstCellNum+256) do
	                local cell = row:GetCell(jjj)
	                if cell ~= nil then 
	                    local str = cell.SValue:gsub("'", "''")
	                    if util.JpMatch(str).Count > 0 then
                            -- if cell.CellComment == nil then
                            --     local patriarch = sheet:CreateDrawingPatriarch()
                            --     local anchor = ClientAnchor()
                            --     local comment = patriarch:CreateCellComment(anchor)
                            --     comment.String = RichTextString(str)
                            --     cell.CellComment = comment
                            --     print("comment", cell, anchor, str)
                            -- end

	                    	lastJpStr = str
		                    -- print(ii, jjj, lastJpStr)
	                    	values[1+#values] = "'" .. lastJpStr .. "',"
	                    	local cache = cellCach[lastJpStr] or {}
	                    	cache[1+#cache] = cell
	                    	cellCach[lastJpStr] = cache
					   end
	                end
	            end
	        end
	    end -- sheet

    end

    if #values > 1 then 
    	values[#values] = "'" .. lastJpStr .. "')" 
    else
    	goto skip
    end

    do 
        local jpTable = {"INSERT INTO alljp (s, src) VALUES "}
        local currentRow
        for k,v in pairs(cellCach) do
        	local src = path
        	for ii, vv in ipairs(v) do
        		if nil == src:match(":" .. vv.Sheet.SheetName) then 
        			src = src .. ":" .. vv.Sheet.SheetName
        		end
        	end
        	-- print(k,v)
            currentRow= "(" 
                .. "'"  .. k   .. "'"
                .. ",'" .. src .. "'"
                ..")"
            jpTable[1+#jpTable] = currentRow .. ","
        end
        jpTable[#jpTable] = currentRow 
            .. " ON CONFLICT(s) DO UPDATE SET "
            ..language.." = CASE WHEN "..language.." ISNULL OR "..language.." = '' THEN excluded."..language.. " ELSE " .. language .. " END"
            ..", src = '{"..language.."=\"'||excluded."..language.."||'\",src=\"'||excluded.src||\'\",v=\""..version.."\"},'||char(13)||src"
            ..";"
    	local sql = table.concat(jpTable, "\n")
    	-- print(sql)
    	cmd.CommandText = sql;
        local reader = cmd:ExecuteReader();
        reader:Dispose()


    	sql = table.concat(values, "\n")
        local f = io.open("tmp.sql", "w")
        f:write(sql)
        f:close()
        lfs.mkdir("sql-"..language)
        local fsql = "sql-"..language.."/".. path:gsub("/", "_") ..".sql"
        if(not System.IO.File.Exists(fsql))then 
        	System.IO.File.Copy("tmp.sql", fsql, false)
        end
    	-- print(sql)

        cmd.CommandText = sql;
        local reader = cmd:ExecuteReader();
        while (reader:Read()) do
        	local jp = reader:GetTextReader(0):ReadToEnd()
            local zh = reader:GetTextReader(1):ReadToEnd()
        	local cache = cellCach[jp]
        	if cache ~= nil and zh ~= nil and zh ~= "" then
                print(jp, "->", zh)
        		for i,v in ipairs(cache) do
        			-- print(i, v, v.CellComment)
    		        v:SetCellValue(zh)
    		        count = 1+count
        		end
    	    end
        end
        reader:Dispose()

        print("translate", count)
    end
    ::skip::

    System.IO.File.Delete(path..".x")
    local outStream = System.IO.FileStream(path .. ".x", System.IO.FileMode.CreateNew);
    outStream.Position = 0;
    wb:Write(outStream);
    outStream:Close();
    System.IO.File.Delete(path)
end

return TransOneExcel
