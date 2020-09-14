local CS = CS
local System = CS.System

local lfs = require "lfs"
local function GetFiles(root, fileAct, filter)
    -- print("GetFiles", root)
    for entry in lfs.dir(root) do
        if entry ~= '.' and entry ~= '..' then
            local traverpath = root .. "/" .. entry
            local attr = lfs.attributes(traverpath)
            if (type(attr) ~= "table") then --如果获取不到属性表则报错
                print('ERROR:' .. traverpath .. 'is not a path')

                goto continue
            end
            -- print(traverpath)
            if (attr.mode == "directory") then
                GetFiles(traverpath, fileAct, filter)
            elseif attr.mode == "file" then
                if fileAct then
                    -- print("filter", filter)
                    if filter ~= nil and type(filter) == "function" then
                        if  filter(traverpath) then fileAct(traverpath) end
                    else
                        fileAct(traverpath)
                    end
                end
            end
        end
        :: continue :: 
    end
end

local function SaveWorkbook( path, wb )
    print("saving ...")
	if(System.IO.File.Exists(path))then System.IO.File.Delete(path)end
	local outStream = System.IO.FileStream(path, System.IO.FileMode.CreateNew);
	outStream.Position = 0;
	wb:Write(outStream);
	outStream:Close();
    print("save done", path)
end

local regular_jp = [[.*[\u3040-\u3126]+.*]] -- jp
local regular_zh = [[.*[\u4e00-\u9fa5]+.*]] -- zh
local regular_jpzh = [[.*[\u3021-\u3126\u4e00-\u9fa5]+.*]] -- zh
local function JpMatch( s )
	return System.Text.RegularExpressions.Regex.Matches(s, regular_jp)
end

local function loadNPIO()
    xlua.load_assembly("NPOI")
    xlua.load_assembly("NPOI.OOXML")
end


local function OpenExcel( path )
    if  true
    and path:sub(-5) ~= ".xlsx" 
    and path:sub(-4) ~= ".xls" 
    then
        print("not a excel file", path)
        return
    end

    loadNPIO()
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

return {
    OpenExcel = OpenExcel,
    SaveWorkbook = SaveWorkbook,
    loadNPIO = loadNPIO,
	JpMatch = JpMatch,
	GetFiles = GetFiles
}
