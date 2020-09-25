local CS = CS
local System = CS.System

local util = require "lua/util"
util.loadNPIO()

local NPOI = CS.NPOI
local Workbook = NPOI.XSSF.UserModel.XSSFWorkbook

local function trim(s, t)
    -- if t == nil then t = {"^\s*", "\s*$" } end
    for i,v in ipairs(t) do
        s = s:gsub(v, '')
    end
    return s
end
function fixquot( sheet, db )

    for i = 1, math.min(sheet.LastRowNum, 10000) do
        local row = sheet:Row(i)
        for j = 0, row.LastCellNum - 1 do
            local cell = row:Cell(j)
            local tt = {
                "'",
                "'$",
            }
            local os = cell:ToString()
            -- fixed
            local s = trim(os, tt)
            s = s:gsub("\\n", "\n"):gsub("\\r", "\r"):gsub("\\t", "\t")
            print(os, s)
            cell:SetCellValue(s)
        end
    end
end

function merge( sheet, db )

    for i = 1, math.min(sheet.LastRowNum, 10000) do
        local row = sheet:Row(i)
        local id = row:Cell(0):ToString()
        local jp = row:Cell(2):ToString():gsub("\\n", "\n"):gsub("\\r", ""):gsub("\\t", "\t")
        local zh = row:Cell(3):ToString():gsub("\\n", "\n"):gsub("\\r", ""):gsub("\\t", "\t")

        local sql = string.format("update m_voice set serif = '%s' where voice_id = %s;", zh, id)
        local res, err = db:execute(sql)
        print(res, err, sql:gsub("\n", "\\n"))
    end
end

function CollectExcelDic(path, db, language, version)
    if(path:sub(-5) == ".xlsx" and nil == path:match("~") )then return end

    language = language or "zh"
    version  = version or "default"

    print("CollectOne",path)
    local wb = Workbook(path)
    local sheet = wb:GetSheet('jp')
    local values = {
        "INSERT OR IGNORE INTO dic (s," .. language .. ",src) VALUES "
    }
    local currentRow
    for i = 1, math.min(sheet.LastRowNum, 10000) do
        local row = sheet:GetRow(i)
        if row ~= nil then
            -- for j = 0, row.LastCellNum - 1 do
            --     local cell = row:GetCell(j)
            --     if cell ~= nil then 
            --         print(i, j, row:GetCell(j).SValue)
            --     end
            -- end
            local cjp = row:GetCell(0)
            if cjp == nil then goto continue end
            local jp = cjp.SValue
            jp=jp:gsub("'", "''")

            local ctrans = row:GetCell(1)
            if ctrans == nil then goto continue end
            local trans = ctrans.SValue
            trans=trans:gsub("'", "''")
            if trans ~= '译文' and trans ~= '' then
                currentRow= "(" 
                    .. "'".. jp .."'," -- jp
                    .. "'".. trans .."'," -- trans
                    .. "'".. path .. ":" ..row:GetCell(5).SValue.."'" -- src
                    ..")"
                values[1+#values] = currentRow .. ","
            else
                print(jp:gsub("\n", "\\n"), trans)
            end
        end
        ::continue::
    end
    values[#values] = currentRow
        .. " ON CONFLICT(s) DO UPDATE SET "
        ..language.." = CASE WHEN "..language.." ISNULL OR "..language.." = '' THEN excluded."..language.. " ELSE " .. language .. " END"
        ..", src = '{"..language.."=\"'||excluded."..language.."||'\",src=\"'||excluded.src||\'\",v=\""..version.."\"},'||char(13)||src"
        .." WHERE "..language.." <> excluded."..language
        .. ";"
    -- values[1+#values] = "COMMIT;"

    local sql = table.concat(values, "\n")
    local cmd = db:CreateCommand();
    cmd.CommandText = sql;
    local reader = cmd:ExecuteReader();
    reader:Dispose()


    local f = io.open(path .. ".sql", "w")
    f:write(sql)
    f:close()
end

local luasql = require "luasql.mysql"
local db, err = luasql.mysql():connect("a3_m_305", "a3", "654123", "10.23.22.233")
print(db, err)

local path = '/Volumes/Data/a3/tools/bin/voice_305.xlsx'
local wb = Workbook(path)
local sheet = wb:GetSheet("voice")
-- fixquot(sheet, db)
merge(sheet, db)

util.SaveWorkbook(path, wb)


-- return CollectExcelDic