//using Everlasting.UIBind;
//using TMPro;
//using UnityEngine;

//namespace Localization.ILocalizeAndMono
//{
//    [RequireComponent(typeof(TMP_Text))]
//    [RequireComponent(typeof(LocalizedTextMeshPro))]
//    public class UIBindLocalizationKeyForTMP : BaseUIBindData<string, LocalizedTextMeshPro>, IUIBindText
//    {
//        protected override void BindFunction(UIDataCell<string, LocalizedTextMeshPro> cell, string value)
//        {
//            cell.Component.key = value;
//            cell.Component.OnLocalize();
//        }
//    }
//}