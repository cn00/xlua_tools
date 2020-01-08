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

local regular_jp = [[.*[\u3040-\u3126]+.*]] -- jp
local regular_zh = [[.*[\u4e00-\u9fa5]+.*]] -- zh
local regular_jpzh = [[.*[\u3021-\u3126\u4e00-\u9fa5]+.*]] -- zh
local function JpMatch( s )
	return System.Text.RegularExpressions.Regex.Matches(s, regular_jp)
end

return {
	JpMatch = JpMatch,
	GetFiles = GetFiles
}
