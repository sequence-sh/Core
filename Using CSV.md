# How to use CSV files in your configurations

You can use a CSV file as part of a Loop process. 


```yaml
!Loop
  For: !CSV
    CSVFilePath: C:/Documents/Searches.csv
    InjectColumns:
      Search:
        Property: SearchTerm
      TagName:
        Property: Tag
    Delimiter: ','
    CommentToken: '#'
    HasFieldsEnclosedInQuotes: false
  RunProcess: !NuixSearchAndTag
    CasePath: *CasePath
```

You can either provide the CSVFilePath or the CSVText.

The loops will iterate once for each row in the CSV.

You can set the delimiter value in the CSV object. The default value is ','.

You can set a comment token in the CSV object. Any line which starts with the comment token will be ignored.

Use the InjectColumns property to inject data from your CSV file into your process.

The InjectColumns property is a dictionary - the column headers are keys and the values are injections.

You do not need to quote the fields in your CSV file. You can do so if you set the HasFieldsEnclosedInQuotes property to true. This allows you to have fields containing the delimiter character.