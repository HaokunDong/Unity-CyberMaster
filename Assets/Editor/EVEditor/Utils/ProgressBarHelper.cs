using System;
using UnityEditor;

namespace EverlastingEditor.Utils
{
    public class ProgressBarHelper : IDisposable
    {
        private string title;
        private string content;
        private float current;
        private float total;

        public float Total
        {
            get { return total; }
            set
            {
                total = value;
                UpdateProgressBar();
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                UpdateProgressBar();
            }
        }

        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                UpdateProgressBar();
            }
        }

        public float Current
        {
            get { return current; }
            set
            {
                current = value;
                UpdateProgressBar();
            }
        }

        public ProgressBarHelper(string title, float total)
        {
            this.title = title;
            this.content = "";
            this.total = total;
            this.current = 0;
        }

        public void Update(string content, float value)
        {
            this.content = content;
            this.current = value;
            UpdateProgressBar();
        }

        private void UpdateProgressBar()
        {
            EditorUtility.DisplayProgressBar(Title, Content, Total == 0 ? 0 : Current / Total);
        }

        public void Dispose()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}