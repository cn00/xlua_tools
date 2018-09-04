### Usage

* diffExcel(sh)
```bash
mono path/to/ExcelDiff.exe $1 $2
```

* .gitconfig
```ini
[diff "excel"]
	command = diffExcel $LOCAL $REMOTE
```

* ${HOME}/.config/git/attributes
```
# excel
*.xls  diff=excel
*.xlsx diff=excel
```

* gitbash
```bash
git diff [commit_id]  [-- path/to/excel_file]
```
