local CS = CS
local System = CS.System

print(package.path)
print(package.cpath)

local util = require "util"
local excel = require "util.excel"
local luasql = require "luasql.mysql"

---Excel2Sqlite
---@param path string
---@param db luasql.mysql
---@param fields string
---@param version string int
---@param suffix string
local function ExcelStory2Mysql(path, db, story_id, type, version, suffix)
    if (path:sub(-5) == ".xlsx" and nil == path:match("~")) then
        return
    end

    if story_id == nil or story_id == "" then error("story_id can not be nil or empty") return end
    version = version or "308"
    suffix = suffix or ""

    local createsql = string.format([[
        create table if NOT EXISTS `story_%s%s`(
          `id`        int(16) unsigned NOT NULL AUTO_INCREMENT,
          `type`      char(16), -- main, mini, event, cross, ...
          `story_id`  char(64), -- main_001_001, main_001_002, storymini084_016_003
          `version`   int(8),   -- 210, 305, 308
        
          -- scenario
          `row`       int(16) unsigned NOT NULL,
          `command`   char(32), -- Bg, Title, Bgm, Face, Anime, Emotion
          `arg1`      char(32), -- char: StoryBg01_028_01, bgm004, sakuya05, ...
          `arg2`      char(32), -- charname: <User>, 2, 3, Jump01
          `arg3`      char(32), -- layer: LayerBg1, LayerChrRight, ...
          `arg4`      char(32), -- easeOutQuad
          `arg5`      char(32),
          `arg6`      char(32),
          `arg7`      char(32),
          `text`      tinytext, 
          `pagectrl`  char(32),
          `voice`     char(32), -- sound_label
          `memo`      char(32),
          `english`   char(32),
          
          -- layer
          `layer_type` char(32),
          `layer_x`    Float,
          `layer_y`    Float,
          `layer_z`    Float,
        
          -- sound
          -- `sound_label`     char(32), -- 004_st034
          `sound_type`      char(32), -- Sound004, ...
          `cuesheet`        char(32), -- sound004_002_004
          `cuename`         char(32), -- 004_st034
          `streaming`       char(32), -- 004_st034
        
          `ud`        timestamp NULL DEFAULT NULL ON UPDATE current_timestamp(),
          PRIMARY KEY (`id`),
          UNIQUE KEY `story_id_row` (`story_id`,`row`),
          KEY `story` (`story_id`, `type`, `command`, `arg1`, `text`, `voice`),
          KEY `text` (`text`(32)) KEY_BLOCK_SIZE=32
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        -- PARTITION BY LINEAR HASH (`story_id`) PARTITIONS 16
    ]], version, suffix)
    local res, err = db:execute(createsql);
    if err ~= nil then
        error(err)
    end

    local wb = excel.OpenExcel(path)
    local sound = excel.GetSheetAsTable(wb:GetSheet('Sound'))
    local layer = excel.GetSheetAsTable(wb:GetSheet('Layer'))

    local getLayerInfo = function(label)
        for i, v in ipairs(layer) do
            if v.Label == label then
                return v
            end
        end
        return nil
    end
    local getSoundInfo = function(label)
        for i, v in ipairs(sound) do
            if v.Label == label then
                return v
            end
        end
        return nil
    end

    local sheet = wb:GetSheet('Scenario')
    local scenario = excel.GetSheetAsTable(sheet, sheet.FirstRowNum + 2)
    local values = { "INSERT ignore INTO story_".. version .. "_jp (story_id, type, version, row, command, arg1, arg2, arg3, arg4, arg5, arg6, arg7, text, pagectrl, voice, memo, english, layer_type, layer_x, layer_y, layer_z, sound_type, cuesheet, cuename, streaming) VALUES " }
    local currentRow
    for i, v in ipairs(scenario) do
        local l = getLayerInfo(v.Arg3) or {Type = "", X = 0, Y = 0, Z = 0}
        local s = getSoundInfo(v.Voice) or {Type = "", CueSheet = "", CueName = "", Streaming = ""}
        currentRow = string.format("('%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s','%s')"
        , story_id, type, version, i
        , v.Command, v.Arg1, v.Arg2, v.Arg3, v.Arg4, v.Arg5, v.Arg6, v.Arg7, v.Text, v.PageCtrl, v.Voice, v.Memo, v.English
        , l.Type, l.X, l.Y, l.Z, s.Type, s.CueSheet, s.CueName, s.Streaming
        )
        values[1 + #values] = currentRow .. ","
    end
    values[#values] = currentRow -- replace last ','
        .. " ON DUPLICATE KEY UPDATE text = VALUES(text)" -- mysql
        --.. " ON CONFLICT DO NOTHING" -- postgresql, sqlite
        --.. " ON CONFLICT(`story_id`,`row`) DO UPDATE SET "
        --.. language .. " = CASE WHEN " .. language .. " ISNULL OR " .. language .. " = '' THEN excluded." .. language .. " ELSE " .. language .. " END"
        --.. ", src = '{" .. language .. "=\"'||excluded." .. language .. "||'\",src=\"'||excluded.src||\'\",v=\"" .. version .. "\"},'||char(13)||src"
        --.. " WHERE " .. language .. " <> excluded." .. language
        .. ";"
    -- values[1+#values] = "COMMIT;"

    local sql = table.concat(values, "\n")

    local f = io.open(path .. ".sql", "w")
    f:write(sql)
    f:close()
    
    local res, err = db:execute(sql);
    if err ~= nil then
        error(err)
    end
end

local function Story2MysqlTest()
    -- TODO: import master excelpath:find("/Master.xls")
    -- local excelpath = "/Users/cn/a3/cjp/res-jp/ExcelData/Story/back/back_663_067.xls"
    local count  = 0
    util.GetFiles("path/to/ExcelData/Story", function(excelpath)
        count = count + 1
        local matchs = string.gmatch(excelpath, ".*/Story/([^/]*)/(.*)%.xls")
        local type, id = matchs()
        print(count, id, type, excelpath)
        if type ~= nil and id ~= nil then
            local db, err = luasql.mysql():connect("a3_excel_data", "a3", "***", "10.x.x.x")
            local suffix = ""
            ExcelStory2Mysql(excelpath, db, id, type, "308", suffix)
        end
    end, "xls|xlsx")
end
-- Story2MysqlTest()

return ExcelStory2Mysql