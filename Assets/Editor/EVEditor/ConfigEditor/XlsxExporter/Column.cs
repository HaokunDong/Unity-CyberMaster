using System;

namespace EverlastingEditor.Config.Export
{
    public class Column
    {
        public int ColumnNum { get; }
        public string Name { get; }
	    public string DefaultValue { get; }
		public string ColumnTypeString { get; }
        public IColumnType ColumnType { get; }
        public ConfigTypes ConfigTypes { get; }
        private IData defaultData = null;
        public IData DefaultData
        {
            get
            {
                if (defaultData == null)
                    defaultData = ColumnType.Parse(DefaultValue);
                return defaultData;
            }
        }
        
        public Column(DataView dataView, SheetView sheetView, int colNum)
        {
            ColumnNum = colNum;
	        var sheetColumn = sheetView.GetColumn(colNum);
            Name = sheetColumn[2];
	        ColumnTypeString = sheetColumn[4];
	        try
			{
				ColumnType = dataView.ColumnTypeParser.ParseColumnType(ColumnTypeString, Name);
			}
	        catch (Exception e)
	        {
		        throw new Exception($"类型{ColumnTypeString}解析失败", e);
	        }
            ConfigTypes = Utils.ParseConfigTypes(sheetColumn[5]);
            DefaultValue = sheetColumn[3] == "" ? ColumnType.DefaultValue : sheetColumn[3];
			//ID检查
	        if (colNum == 1)
	        {
		        if (Name != "Id")
		        {
			        throw new Exception("表中第一列名称必须为Id！");
		        }
		        if (!(ColumnType is IIdColumnType))
		        {
			        throw new Exception("表中第一列类型必须为Id类型！");
				}
			}
		}

        public IData ParseRow(string excelString)
        {
            return ColumnType.Parse(excelString);
        }
    }
}
