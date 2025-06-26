using Localization.Enum;
using TMPro;
//using UI.Components.UIBind;
using UnityEngine;

namespace Localization.ILocalizeAndMono
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTextMeshPro : LocalizedTextComponent<TMP_Text>
#if UNITY_EDITOR
    //, ITextLocalizeIdentifier
#endif
    {
        protected override void SetText(TMP_Text component, string value)
        {
            if (text == null)
            {
                Debug.LogWarning("Missing Text Component on " + gameObject, gameObject);
                return;
            }
            text.text = value;
        }

        protected override void UpdateAlignment(TMP_Text component, LanguageDirection direction)
        {

        }

#if UNITY_EDITOR
        public string GetLanguageKey()
        {
            return key;
        }

        public string GetReferenceContent()
        {
            if (text == null)
            {
                return string.Empty;
            }

            return text.text;
        }

        public void ReDesignateLanguageKey(string k)
        {
            key = k;
        }
#endif
    }
}