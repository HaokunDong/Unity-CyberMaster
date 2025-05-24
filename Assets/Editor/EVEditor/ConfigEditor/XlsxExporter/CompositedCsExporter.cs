using System;
using System.Collections.Generic;
using System.IO;

namespace EverlastingEditor.Config.Export
{
    public class CompositedCsExporter : IExporter
    {
        private readonly List<string> _filePaths;
        private readonly string _outFileName, _csOutputPath, _binaryOutPutPath;
        private CsExporter _baseExporter;
        
        public CompositedCsExporter(string name, List<string> filePaths, string csOutputPath, string binaryOutputPath)
        {
            (_outFileName, _filePaths, _csOutputPath, _binaryOutPutPath) = (name, filePaths, csOutputPath, binaryOutputPath);
        }
        
        public void Export()
        {
            if (_filePaths == null || _filePaths.Count == 0)
            {			   
                throw new Exception($"合并表格输出失败，缺少有效的源表");
            }
            
            _baseExporter = new CsExporter(_outFileName, _filePaths[0], _csOutputPath, _binaryOutPutPath);
            for (var i = 1; i < _filePaths.Count; i++)
            {
                var name = Path.GetFileNameWithoutExtension(_filePaths[i]);
                name = char.ToUpper(name[0]) + name.Substring(1);
                var csExporter = new CsExporter(name, _filePaths[i], _csOutputPath, _binaryOutPutPath);
                csExporter.MergeTo(_baseExporter);
            }

            _baseExporter.Export();
        }
    }
}