using ElectronicEducationEnviroment.Admin;
using ICSharpCode.AvalonEdit;
using Markdig;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ElectronicEducationEnviroment.Teacher
{
    public partial class LectureEditorWindow : Window
    {
        private readonly MarkdownPipeline _pipeline;
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly Lecture _lecture;
        private readonly int _courseId;
        private readonly bool _isEditMode;

        public LectureEditorWindow(int courceId, Lecture lecture = null)
        {
            InitializeComponent();

            _context = App.Context;
            _lecture = lecture ?? new Lecture();
            _isEditMode = lecture != null;
            _courseId = courceId;

            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            if (_isEditMode)
            {
                Title = "Редактирование лекции";
                TitleTextBox.Text = _lecture.LectureTitle ?? string.Empty;
                Editor.Text = _lecture.LectureText ?? string.Empty;
            }
            else
            {
                Title = "Новая лекция";
                TitleTextBox.Text = string.Empty;
                Editor.Text = string.Empty;
            }

            InitializeWebView2();
            RenderPreview();
        }

        private async void InitializeWebView2()
        {
            try
            {
                await PreviewWebView.EnsureCoreWebView2Async();
                if (PreviewWebView.CoreWebView2 != null)
                {
                    PreviewWebView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка инициализации WebView2: " + ex.Message);
            }
            RenderPreview();
        }

        private void RenderPreview()
        {
            try
            {
                string md = Editor.Text ?? string.Empty;
                string html = Markdig.Markdown.ToHtml(md, _pipeline);
                string template =
                    "<!doctype html><html><head><meta charset='utf-8'/>" +
                    "<style>body{font-family:'Segoe UI',sans-serif;margin:20px;color:#222}" +
                    "pre,code{background:#f6f6f6;padding:8px;border-radius:6px}" +
                    "img{max-width:100%;height:auto}" +
                    "table{border-collapse:collapse;width:100%}" +
                    "td,th{border:1px solid #ccc;padding:6px}</style></head><body>" +
                    html + "</body></html>";

                if (PreviewWebView.CoreWebView2 != null)
                    PreviewWebView.NavigateToString(template);

                StatusText.Text = "Символов: " + md.Length;
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка рендеринга: " + ex.Message;
            }
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название лекции.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _lecture.LectureTitle = title;
            _lecture.LectureText = Editor.Text;
            _lecture.idCource = _courseId;
            if (!_isEditMode)
                _context.Lectures.Add(_lecture);

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Лекция успешно сохранена.", "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);

                
                if (IsVisible && IsLoaded)
                {
                    try { DialogResult = true; }
                    catch {}
                }

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении в базу данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            RenderPreview();
        }

        private void Editor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string input = e.Text;
            string autoClose = null;

            if (input == "(") autoClose = ")";
            else if (input == "[") autoClose = "]";
            else if (input == "{") autoClose = "}";
            else if (input == "\"") autoClose = "\"";
            else if (input == "'") autoClose = "'";

            if (autoClose != null)
            {
                int caret = Editor.CaretOffset;
                Editor.Document.Insert(caret, input + autoClose);
                Editor.CaretOffset = caret + 1;
                e.Handled = true;
            }
        }

        private void BtnBold_Click(object sender, RoutedEventArgs e)
        {
            WrapSelection("**");
        }

        private void BtnItalic_Click(object sender, RoutedEventArgs e)
        {
            WrapSelection("*");
        }

        private void BtnCode_Click(object sender, RoutedEventArgs e)
        {
            WrapSelection("`");
        }

        private void BtnHeader_Click(object sender, RoutedEventArgs e)
        {
            var selected = Editor.SelectedText;
            Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, "# " + selected);
        }

        private void WrapSelection(string wrapper)
        {
            var selected = Editor.SelectedText;
            Editor.Document.Replace(Editor.SelectionStart, Editor.SelectionLength, wrapper + selected + wrapper);
        }
    }
}
