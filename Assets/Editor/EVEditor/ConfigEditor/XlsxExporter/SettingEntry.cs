using System;

namespace EverlastingEditor.Config.Export
{
    public class SettingEntry
    {
        public string Name { get; }
        public IColumnType ColumnType { get; }
        public ConfigTypes ConfigTypes { get; }
        public IData Data { get; }

        public SettingEntry(DataView dataView, SheetView.Row row, int nameIndex, int typeIndex, int valueIndex, int filterIndex)
        {
            Name = row[nameIndex];
            ColumnType = dataView.ColumnTypeParser.ParseColumnType(row[typeIndex], Name);
            string valueStr = row[valueIndex];
            if(valueStr == "")
                throw new Exception("Setting的Value不应该为空");
            Data = ColumnType.Parse(row[valueIndex]);
            ConfigTypes = Utils.ParseConfigTypes(row[filterIndex]);
        }
    }
}