using DinkToPdf.Contracts;
using DinkToPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.ViewModels;

namespace Services
{
    public interface IExportPDFservice
    {
        public string ExportPDF(ResultTestVM resultTestVM, out byte[] file);
    }
    public class ExportPDFservice : IExportPDFservice
    {
        private readonly IConverter _pdfConverter;
        public ExportPDFservice(IConverter pdfConverter)
        {
            _pdfConverter = pdfConverter;
        }

        public string ExportPDF(ResultTestVM resultTestVM, out byte[] file)
        {
            file = null;
            string content = @"
            <html>
            <head>
                <style>
                    body { font-family: Arial, sans-serif; }
                    .container { width: 80%; margin: auto; padding: 20px; border: 1px solid #ccc; }
                    .header { font-size: 24px; font-weight: bold; text-align: center; }
                    .highlight { font-size: 20px; font-weight: bold; text-align: center; color: red; }
                    .divider { border-top: 2px solid black; margin: 10px 0; }
                    .question-card { margin-bottom: 10px; padding: 10px; border: 1px solid #ddd; }
                    .question { font-weight: bold; }
                    .correct { color: green; font-weight: bold; }
                    .option { color: black; }
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>Kết quả</div>
                    <div class='highlight'>Bài kiểm tra</div>
                    <div class='divider'></div>
            ";
            int number = 0;
            // Thêm nội dung câu hỏi từ Dictionary
            foreach (var question in resultTestVM.optionResult)
            {
                number++;
                content += "<div class='question-card'>";
                content += $"<div class='question'>Câu {number}: {question.Key}</div>";

                foreach (var option in question.Value)
                {
                    if (option.IsCorrect == true && option.UserAnswer == true)
                    {
                        content += $"<div class='correct'>✔ {option.OptionContent}</div>";  // Đáp án đúng màu xanh
                    }
                    else if (option.IsCorrect == false && option.UserAnswer == true)
                    {
                        content += $"<div class='wrong'>✘ {option.OptionContent}</div>";  // Người dùng chọn sai -> màu đỏ
                    }
                    else
                    {
                        content += $"<div class='option'>{option.OptionContent}</div>";  // Đáp án sai không chọn -> màu đen
                    }
                }

                content += "</div>";
            }
            content += "<div class='divider'></div></div></body></html>";
            // Tạo PDF từ nội dung HTML
            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4
                },
                Objects =
            {
                new ObjectSettings()
                {
                    PagesCount = true,
                    HtmlContent = content,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
            };

            byte[] fileBytes = _pdfConverter.Convert(pdfDoc);
            file = fileBytes;
            return "";
        }
    }
}
