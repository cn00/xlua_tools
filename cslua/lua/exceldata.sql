



-- create table `story_sound`(
--   `id`        int(16) unsigned NOT NULL AUTO_INCREMENT,
--   `story_id`  char(64), -- main_001_001, main_001_002, storymini084_016_003
--   `version`   int(8),   -- 210, 305, 308

--   `label`     char(32), -- 004_st034
--   `type`      char(32), -- Sound004, ...
--   `cuesheet`  char(32), -- sound004_002_004 
--   `cuename`   char(32), -- 004_st034
--   `streaming` char(32), -- 004_st034
--   `ud`        timestamp NULL DEFAULT NULL ON UPDATE current_timestamp(),
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4

-- create table `story_layer`( -- Arg3
--   `id`    int(16) unsigned NOT NULL AUTO_INCREMENT,
--   `story_id`  char(64), -- main_001_001, main_001_002, storymini084_016_003
--   `version`   int(8),   -- 210, 305, 308
-- 
--   `label`     char(32),
--   `x`         Float,
--   `y`         Float,
--   `z`         Float,
--   `ud`        timestamp NULL DEFAULT NULL ON UPDATE current_timestamp(),
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4

create table if NOT EXISTS `story_scenario`(
  `id`        int(16) unsigned NOT NULL AUTO_INCREMENT,
  `story_id`  char(64), -- main_001_001, main_001_002, storymini084_016_003
  `type`      char(16), -- main, mini, event, cross, ...
  `version`   int(8),   -- 210, 305, 308

  -- scenario
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
  KEY `story` (`story_id`, `type`, `command`, `arg1`, `text`, `voice`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
PARTITION BY LINEAR HASH (`id`)