using System;
using System.Collections.Generic;
using OfficeOpenXml;

namespace EverlastingEditor.Config.Export
{
    public class DataView
    {
	    private const string MainDataSheetName = "data";
		public string Name { get; }
        public SheetView MainSheetView { get; }
		public List<SheetView> SubSheetViews { get; } = new List<SheetView>();
		public ExcelPackage ExcelPackage { get; }
		public ColumnTypeParser ColumnTypeParser { get; } = new ColumnTypeParser();
		public List<Column> ColumnHeaders { get; private set; }


		public SheetView SettingSheetView { get; } = null;
		private List<List<Column>> SubColumnHeaders { get; set; } = new List<List<Column>>();

		public DataView(string name, string filePath, ExcelPackage excel)
	    {
		    Name = name;
			ExcelPackage = excel;
		    ExcelWorksheet worksheet = excel.Workbook.Worksheets[MainDataSheetName];
		    if (worksheet != null)
		    {
			    MainSheetView = new ExcelSheetView(worksheet, filePath);
		    }
		    foreach (var ws in excel.Workbook.Worksheets)
		    {
			    if (ws.Name != MainDataSheetName && !ws.Name.StartsWith("~"))
			    {
				    if (ws.Name != "Setting")
				    {
					    SubSheetViews.Add(new ExcelSheetView(ws, filePath));
				    }
				    else
				    {
					    SettingSheetView = new ExcelSheetView(ws, filePath);
				    }
			    }
		    }
		    if (SettingSheetView == null && MainSheetView == null)
		    {
			    throw new Exception($"没有找到Worksheet");
		    }
	    }

	    private Tuple<List<Column>, Dictionary<string, Column>> ReadMainHeader(SheetView sheetView)
	    {
		    try
		    {
			    List<Column> columns = new List<Column>();
			    for (int i = 1; i <= sheetView.MaxColumn; i++)
			    {
				    if (sheetView.IsStopColumn(i, 0))
					    break;

				    if (sheetView.GetColumn(i).IsCommentColumn())
				    {
					    continue;
				    }
				    
				    Column column;
				    try
				    {
						column = new Column(this, sheetView, i);
					}
				    catch (Exception e)
				    {
					    throw new Exception($"{e.Message}\n处理列{Utils.ColumnNum2Label(i)}失败", e);
				    }
					columns.Add(column);
			    }
			    Dictionary<string, Column> columnsDic = new Dictionary<string, Column>();
			    foreach (var column in columns)
			    {
				    if (columnsDic.ContainsKey(column.Name))
				    {
					    throw new Exception($"列{column.ColumnNum}和{columnsDic[column.Name].ColumnNum}有重复的键名{column.Name}");
				    }
				    columnsDic.Add(column.Name, column);
			    }

			    return new Tuple<List<Column>, Dictionary<string, Column>>(columns, columnsDic);
			}
		    catch (Exception e)
		    {
			    throw new Exception($"{e.Message}\n处理sheet {sheetView.Name}失败", e);
		    }
	    }

	    public void ReadMainHeader()
	    {
		    if (MainSheetView == null)
			    return;
		    var mainTuple = ReadMainHeader(MainSheetView);
		    ColumnHeaders = mainTuple.Item1;
		    var mainHeadersDic = mainTuple.Item2;
		    SubColumnHeaders.Clear();
		    foreach (var sheetView in SubSheetViews)
		    {
			    var subTuple = ReadMainHeader(sheetView);
			    //检查多余字段
			    foreach (var header in subTuple.Item1)
			    {
				    Column column;
				    if (mainHeadersDic.TryGetValue(header.Name, out column))
				    {
					    if (header.ColumnType.TypeName != column.ColumnType.TypeName ||
					        header.ColumnTypeString != column.ColumnTypeString)
					    {
						    throw new Exception(
							    $"字表{sheetView.Name}第{Utils.ColumnNum2Label(header.ColumnNum)}字段{header.Name}类型{header.ColumnTypeString}和主表类型{column.ColumnTypeString}不符");
					    }
				    }
				    else
				    {
					    throw new Exception(
						    $"字表{sheetView.Name}第{Utils.ColumnNum2Label(header.ColumnNum)}列存在多余的字段{header.Name}");
				    }
			    }

			    List<Column> subColumnHeader = new List<Column>();
			    //重新排列顺序
			    foreach (var columnHeader in ColumnHeaders)
			    {
				    Column column;
				    subColumnHeader.Add(subTuple.Item2.TryGetValue(columnHeader.Name, out column) ? column : null);
			    }

			    SubColumnHeaders.Add(subColumnHeader);
		    }
	    }

	    public List<IData[]> ReadMainData()
	    {
		    if (MainSheetView == null)
			    return null;
			List<IData[]> dataList = new List<IData[]>();
		    foreach (var row in MainSheetView.GetRows(6, MainSheetView.MaxRow))
		    {
			    if(row.IsCommentRow())
					continue;
			    var data = new IData[ColumnHeaders.Count];
				dataList.Add(data);
			    for (var index = 0; index < ColumnHeaders.Count; index++)
			    {
				    var columnHeader = ColumnHeaders[index];
				    try
				    {
					    string excelString = row[columnHeader.ColumnNum];
					    if (excelString.Length == 0)
					    {
						    data[index] = columnHeader.DefaultData;
					    }
					    else
					    {
						    data[index] = columnHeader.ParseRow(excelString);
						}
				    }
				    catch (Exception e)
				    {
					    throw new Exception($"{e.Message}\n在读取表{MainSheetView.Name}中的第{row.RowNum}行第{columnHeader.ColumnNum}列时, path: {MainSheetView.Location}", e);
				    }
			    }
		    }

		    for (var i = 0; i < SubSheetViews.Count; i++)
		    {
			    var subSheetView = SubSheetViews[i];
			    var columnHeaders = SubColumnHeaders[i];
			    foreach (var row in subSheetView.GetRows(6, subSheetView.MaxRow))
			    {
				    if (row.IsCommentRow())
					    continue;
				    var data = new IData[columnHeaders.Count];
				    dataList.Add(data);
				    for (var index = 0; index < columnHeaders.Count; index++)
				    {
					    var columnHeader = columnHeaders[index];
					    try
					    {
						    if (columnHeader == null)
						    {
							    data[index] = ColumnHeaders[index].DefaultData;
						    }
						    else
							{
								string excelString = row[columnHeader.ColumnNum];
								if (excelString.Length == 0)
								{
									data[index] = columnHeader.DefaultData;
								}
								else
								{
									data[index] = columnHeader.ParseRow(excelString);
								}
							}
					    }
					    catch (Exception e)
					    {
						    throw new Exception($"{e.Message}\n在读取子表{subSheetView.Name}中的第{row.RowNum}行第{columnHeader?.ColumnNum}列时", e);
					    }
				    }
			    }
		    }

		    return dataList;
	    }

	    public List<SettingEntry> ReadSettingData()
	    {
		    if (SettingSheetView == null)
			    return null;
		    Dictionary<string, int> columnNameToIndex = new Dictionary<string, int>();
		    for (int i = 1; i <= SettingSheetView.MaxColumn; i++)
		    {
			    if (SettingSheetView.IsStopColumn(i, 0))
				    break;
			    var sheetColumn = SettingSheetView.GetColumn(i);
			    string name = sheetColumn[2];
			    if (columnNameToIndex.ContainsKey(name))
			    {
				    throw new Exception($"列{i}和{columnNameToIndex[name]}有重复的键名{name}");
			    }

			    columnNameToIndex.Add(name, i);
		    }

		    int nameIndex;
		    int typeIndex;
		    int valueIndex;
		    int filterIndex;
		    if (!columnNameToIndex.TryGetValue("Name", out nameIndex))
		    {
			    throw new Exception("未找到Setting表需要的Name列");
		    }

		    if (!columnNameToIndex.TryGetValue("Type", out typeIndex))
		    {
			    throw new Exception("未找到Setting表需要的Type列");
		    }

		    if (!columnNameToIndex.TryGetValue("Value", out valueIndex))
		    {
			    throw new Exception("未找到Setting表需要的Value列");
		    }

		    if (!columnNameToIndex.TryGetValue("Filter", out filterIndex))
		    {
			    throw new Exception("未找到Setting表需要的Filter列");
		    }

		    List<SettingEntry> dataList = new List<SettingEntry>();
		    foreach (var row in SettingSheetView.GetRows(3, SettingSheetView.MaxRow))
		    {
			    if (row.IsCommentRow())
				    continue;
			    try
			    {
				    SettingEntry entry = new SettingEntry(this, row, nameIndex, typeIndex, valueIndex, filterIndex);
				    dataList.Add(entry);
			    }
			    catch (Exception e)
			    {
				    throw new Exception(
					    $"{e.Message}\n在读取表{SettingSheetView.Name}中的第{row.RowNum}行时", e);
			    }
		    }

		    return dataList;
	    }
    }
}
