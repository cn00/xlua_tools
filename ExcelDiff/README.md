### Usage

* diffExcel(sh)
```bash
mono path/to/ExcelDiff.exe $2 $1 | less -N
```
* .gitconfig
```ini
[alias]
	dfex = difftool -x diffExcel
```
* gitbash
```bash
git dfex commit_id  -- path/to/file
```
