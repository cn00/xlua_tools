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

function BFTrans( strt )
    local bf=CS.Baidu.Fanyi.Do
    local json = require "json"
    local t = strt
    print("strt", t, #t)
    local limitl = 1000
    local batchi = 1
    local batchl = 0
    local batch = {}
    local result = {}
    for i = 1, #t do
        local si = t[i]:gsub("\n", "嗯嗯嗯嗯嗯")
        batchl = batchl + #si
        batch[batchi] = si
        batchi = batchi + 1
        -- print("for", i, si)
        if(batchl > limitl or i == #t)then
            local src = table.concat( batch, "\n" )
            local js = bf(src)
            print("js", i, js)
            -- {"from":"jp","to":"zh","trans_result":[{"src":"リンクスキル","dst":"链接技能"},{"src":"グループID","dst":"组ID"},{"src":"※グループIDが同じ場合高価値の大きいほうが発動","dst":"※群ID相同时，高值的一方发动"}]}	
            local transt = json.decode(js)
            table.insert( result, transt.trans_result)
    
            batchl = #si
            batchi = 1
            batch = {}
        end
    end
    return result
end

local function dump(obj, breakline)
    breakline = breakline == nil or breakline == true
    local getIndent, quoteStr, wrapKey, wrapVal, dumpObj
    getIndent = function(level)
        if breakline then
            return string.rep("\t", level)
        else
            return ""
        end
    end
    quoteStr = function(str)
        return '"' .. string.gsub(str, '"', '\"') .. '"'
    end
    wrapKey = function(val, breakline)
        if breakline then
            if type(val) == "number" then
                return "" -- "[" .. val .. "] = "
            elseif type(val) == "string" then
                if string.match(val, "[][-.,%%}{)(]\"'") ~= nil then
                    return "[" .. quoteStr(val) .. "] = "
                else
                    return val .. " = "
                end
            else
                return "[" .. tostring(val) .. "] = "
            end
        else
            if type(val) == "string" then
                if string.match(val, "[][-.,%%}{)(]\"'") ~= nil then
                    return quoteStr(val) .. " = "
                else
                    return val .. " = "
                end
            else
                return ""
            end
        end
    end
    wrapVal = function(val, level, ishead)
        if type(val) == "table" then
            if(val.____already_dumped____)then return quoteStr("nested_table") end
            return dumpObj(val, level, ishead)
        elseif type(val) == "number" then
            return val
        elseif type(val) == "string" then
            return quoteStr(val)
        else
            return tostring(val)
        end
    end
    dumpObj = function(obj, level, ishead)
        if type(obj) ~= "table" then
            return wrapVal(obj)
        end
        level = level + 1

        local tokens = {}
        tokens[#tokens + 1] = "{"
        
        obj.____already_dumped____ = true
        if level < 9 then
            for k, v in pairs(obj) do
                local head = false
                if k == "head" then head = true end
                if(k ~= "____already_dumped____")then
                    local vs = getIndent(level) .. wrapKey(k) .. wrapVal(v, level, head) .. ","
                    if type(k) == "string" and type(v) == "table" and #v > 5 then vs = vs .. " -- " .. k end
                    tokens[#tokens + 1] = vs
                end
            end
            local meta = getmetatable(obj)
            if meta ~= nil then tokens[#tokens + 1] = getIndent(level) .. "__meta = " .. wrapVal(meta, level) .. "," end
        else
            tokens[#tokens + 1] = getIndent(level) .. "..."
        end
        tokens[#tokens + 1] = getIndent(level - 1) .. "}"
        if breakline then
            if #tokens < 6 or ishead then
                local st = table.concat(tokens, " "):gsub("%s%s+", " ")
                if #st < 50 or ishead  then return st end
            end
            return table.concat(tokens, "\n")
        else
            return table.concat(tokens, " ")
        end
    end
    return dumpObj(obj, 0)
end

return {
    OpenExcel = OpenExcel,
    SaveWorkbook = SaveWorkbook,
    loadNPIO = loadNPIO,
	JpMatch = JpMatch,
    GetFiles = GetFiles,
    BFTrans = BFTrans,
    dump = dump
}
