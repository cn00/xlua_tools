
create table if not exists file_list(
    id integer primary key auto_increment,
    p  text not null unique -- file path
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- k:v
create table if not exists kv_text(
    id bigint primary key auto_increment ,
    l integer not null, -- line
    k text, -- key
    v text, -- jp text
    c text, -- comment
    f integer, -- file id
    z text, -- jianti zhongwen
    t text, -- fanti
    b text, -- baidu fanyi
    KEY k (k(32)),
    KEY v (v(128)),
    KEY z (z(128)),
    KEY t (t(128))
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

create view if not exists kv_test_view as
select t1.*, ktv.p path
from kv_text t1
left join file_list ktv on t1.f = ktv.id;

# insert ignore into file_list (p) values ('')