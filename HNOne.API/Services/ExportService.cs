using HNOne.API.Services.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace HNOne.API.Services
{
    public class ExportService : IExportService
    {
        public ExportService() { }

        //public async Task<File> ExportContract()
        //{
        //    try
        //    {
        //        string templatePath = Path.Combine("Templates", "Template.docx"); // Đường dẫn mẫu
        //        string outputPath = Path.Combine("wwwroot", "exports", "ExportedFile.docx"); // Đường dẫn file xuất

        //        // Copy file mẫu sang file xuất
        //        System.IO.File.Copy(templatePath, outputPath, true);

        //        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputPath, true))
        //        {
        //            // Lấy nội dung văn bản
        //            var body = wordDoc.MainDocumentPart.Document.Body;

        //            // Thay thế nội dung placeholder
        //            foreach (var text in body.Descendants<Text>())
        //            {
        //                if (text.Text.Contains("{{Name}}"))
        //                    text.Text = text.Text.Replace("{{Name}}", "Nguyễn Văn A");
        //                if (text.Text.Contains("{{BirthDate}}"))
        //                    text.Text = text.Text.Replace("{{BirthDate}}", "01/01/2000");
        //            }

        //            wordDoc.MainDocumentPart.Document.Save();
        //        }

        //        // Trả file về client
        //        var fileBytes = await System.IO.File.ReadAllBytesAsync(outputPath);
        //        return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "ExportedFile.docx");
        //    }
        //    catch
        //    {

        //    }
        //    return null;
        //}
    }
}
