
local sqlutil = require "util.sqlutil"

-- local sql = [[
-- SELECT 
-- 	s.id
-- 	, s.jp  jp305
-- 	, s2.jp jp210
-- 	, s2.zh zh210
-- 	, s.zh zh305
-- FROM
-- -- UPDATE
-- 	strings s
-- 	LEFT JOIN a3_m_210.strings s2 USING(`unique_string`)
-- -- SET s.zh = CONCAT("v210:", s.zh)
-- WHERE 
-- 	 (s.jp <> s2.jp OR s.zh IS NULL) AND NOT (s.jp LIKE '%ダミー%')
-- ]]
local sql = [[
    select id, unique_string, jp, zh, explanation,provisional, category from strings;
]]
sqlutil.Mysql2Excel("dark_text"
, { "kv_text_not_null", "kv_text_uniq","file_list" }
, "dark", "654123", "cn.local"
, "dark-workspace-jp-kv-text-"..os.date("%Y%m%d%H%M%S")..".xlsx")


-- sql = [[
-- SELECT
--     s.id,
-- 	s2.jp jp305,
--     s.zh zh210,
--     s.category,
--     s.unique_string,
--     s.explanation
-- FROM
-- 	strings s
-- 	LEFT JOIN a3_m_308.strings s2 ON s.id=s2.id
-- WHERE
-- 	s.jpn LIKE '%代替%' 
--     AND s2.jp NOT LIKE '%ダミー%'
-- ]]
-- sqlutil.Mysql2Excel("a3_m_210", sql, "strings", "a3", "654123", "10.23.24.239", "a3-strings-merge-305-patch.xlsx")

-- sqlutil.MysqlTab2Excel("a3_m_308", {"strings"}, "a3", "654123", "10.23.24.239", "a3-strings-merge-311-"..System.DateTime.Now:ToString("yyyyMMddHHmm")..".xlsx")
