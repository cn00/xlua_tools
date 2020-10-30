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

* ${HOME}/.config/git/attributes or repository_root/.gitattributes
```
# excel
*.xls  diff=excel show=excel
*.xlsx diff=excel show=excel
```

* gitbash
```bash
git diff [commit_id]  [-- path/to/excel_file]
```
