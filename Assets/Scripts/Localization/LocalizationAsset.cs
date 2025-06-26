using System;
using Localization.Enum;
using UnityEngine;

namespace Localization
{
    [Serializable]
    public class LocalizationAsset
    {
        public TextAsset textAsset;
        public TextFileFormat format = TextFileFormat.CSV;
    }
}