# How to use CSV files in your configurations

You can use a CSV as part of a Loop process. 


```
!Loop
  For: !CSV
    CSVFilePath: C:/Documents/Searches.csv
    InjectColumns:
      SearchTerm:
        Property: SearchTerm
      Tag:
        Property: Tag
    Delimiter: ','
    HasFieldsEnclosedInQuotes: false
  RunProcess: !NuixSearchAndTag
    CasePath: *CasePath
```