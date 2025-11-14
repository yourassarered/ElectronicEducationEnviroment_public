using ElectronicEducationEnviroment.Admin;
using Markdig;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Tasi.Ground.MarkDownEditor;

namespace ElectronicEducationEnviroment.Teacher
{
    /// <summary>
    /// Логика взаимодействия для LectureViewPage.xaml
    /// </summary>
    public partial class LectureViewPage : Page
    {
        private readonly int _lectureId;
        private readonly MarkdownPipeline _pipeline;

        public LectureViewPage(int id)
        {
            _lectureId = id;

            // Инициализация пайплайна Markdig с расширениями
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            InitializeComponent();
            InitializeWebView2();
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

                RenderPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка инициализации WebView2: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenderPreview()
        {
            try
            {
                var lecture = App.Context.Lectures
                    .FirstOrDefault(l => l.IdLecture == _lectureId);

                if (lecture == null)
                {
                    MessageBox.Show("Лекция не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string md = lecture.LectureText ?? string.Empty;
                string html = Markdig.Markdown.ToHtml(md, _pipeline);

                string template =
                    "<!doctype html><html><head><meta charset='utf-8'/>" +
                    "<style>" +
                    "body{font-family:'Segoe UI',sans-serif;margin:20px;color:#222}" +
                    "pre,code{background:#f6f6f6;padding:8px;border-radius:6px}" +
                    "img{max-width:100%;height:auto}" +
                    "table{border-collapse:collapse;width:100%}" +
                    "td,th{border:1px solid #ccc;padding:6px}" +
                    "</style></head><body>" +
                    html + "</body></html>";

                if (PreviewWebView.CoreWebView2 != null)
                    PreviewWebView.NavigateToString(template);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка рендеринга Markdown: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет предыдущей страницы для возврата.",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
