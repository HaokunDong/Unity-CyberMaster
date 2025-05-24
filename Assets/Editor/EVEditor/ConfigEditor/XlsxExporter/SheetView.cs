using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EverlastingEditor.Config.Export
{
    // 一个数据视图, 用来代替Sheet. 毕竟sheet还是可读写的, 我们这里只读就行
    // 这里做剔除空行, 注释行, 注释列, 定义头等
    // 某些组合列, 需要分解成多列

    public abstract class SheetView
    {        
        public string Name { get; }
        public string Location { get; }
        public int MaxColumn { get; }
        public int MaxRow { get; }

        public SheetView(string name, string location, int maxRow, int maxColumn)
        {
            this.Name = name;
            this.Location = location;
            this.MaxColumn = maxColumn;
            this.MaxRow = maxRow;
        }

        public struct Row
        {
            private SheetView sv;
            private int rowNum;

            public Row(SheetView sv, int rowNum)
            {
                this.sv = sv;
                this.rowNum = rowNum;
            }

            public string this[int colNum] => sv[rowNum, colNum];

            public int RowNum => rowNum;

            public bool IsCommentRow()
            {
                var c = this[1];
                return c.Length > 0 && (c[0] == '-' || c[0] == '~');
            }
            
        }

        public struct Column
        {
            private SheetView sv;
            private int columnNumber;

            public Column(SheetView sv, int columnNumber)
            {
                this.sv = sv;
                this.columnNumber = columnNumber;
            }

            public string this[int rowNum] => sv[rowNum, columnNumber];
            public int ColumnNumber => columnNumber;
            
            public bool IsCommentColumn()
            {
                var c = this[1];
                return c.Length > 0 && (c[0] == '-' || c[0] == '~');
            }
        }
		
        public bool IsStopColumn(int colNum, int offset)
        {
            return string.IsNullOrEmpty(this[1 + offset, colNum]) &&
                string.IsNullOrEmpty(this[2 + offset, colNum]) &&
                string.IsNullOrEmpty(this[3 + offset, colNum]);
        }

        public class Rows : IEnumerable<Row>
        {
            private SheetView sv;
            private int startRow, endRow;

            public Rows(SheetView sv, int startRow, int endRow)
            {
                this.sv = sv;
                this.startRow = startRow;
                this.endRow = endRow;
            }

            public IEnumerator<Row> GetEnumerator()
            {
                for (int rowNum = startRow; rowNum <= endRow; ++rowNum)
                {
                    var row = sv.GetRow(rowNum);
                    if (IsStopRow(row) || row.IsCommentRow())
                        continue;
                    yield return row;
                }
            }

            private bool IsStopRow(Row row)
            {
                return string.IsNullOrEmpty(row[1]);
            }

	        IEnumerator IEnumerable.GetEnumerator()
	        {
		        return GetEnumerator();
	        }
        }

        public abstract string this[int row, int col] { get; }
        
        public Rows GetRows(int startRow, int endRow)
        {
            return new Rows(this, startRow, endRow);
        }

        public Row GetRow(int rowNum)
        {
            return new Row(this, rowNum);
        }

        public Column GetColumn(int colNum)
        {
            return new Column(this, colNum);
        }

    }
    
    public class ExcelSheetView : SheetView
    {
        private ExcelWorksheet st;

        public ExcelSheetView(ExcelWorksheet st, string location) : 
            base(st.Name, location, st.Dimension.Rows, st.Dimension.Columns)
        {
            this.st = st;
        }
        
        public override string this[int rowNum, int colNum]
        {
            get
            {
                var text = st.GetValue<string>(rowNum, colNum);
                if (string.IsNullOrEmpty(text)) return string.Empty;
                return text.Trim();
            }
        }

        public string GetString(int rowNum, int colNum)
        {
            try
            {
                var text = this[rowNum, colNum];
                // 第一行是描述, 把注释也带上
                if (rowNum == 1)
                {
                    var comment = st.Cells[rowNum, colNum].Comment;
                    if (comment != null)
                        text += '\n' + comment.Text;
                }
                return text;
            }
            catch(Exception e)
            {
                throw new Exception($"{st.Name} [{Utils.ColumnNum2Label(colNum)}{rowNum}] 取值出错", e);
            }
        }

        public bool IsHiddenRow(int rowNum)
        {
            return st.Row(rowNum).Hidden;
        }

        public bool IsHiddenCol(int colNum)
        {
            return st.Column(colNum).Hidden;
        }
    }
}
