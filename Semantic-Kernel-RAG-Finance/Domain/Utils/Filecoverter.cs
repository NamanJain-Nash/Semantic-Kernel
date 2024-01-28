using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Xceed.Words.NET;

namespace Buisness_Logic.Utils
{
    public static class Filecoverter
    {
        public static FileInfo ConvertToText(FileInfo inputFile, ILogger _logger)
        {
            string resultText;
            try
            {
                //Convert pdf
                if (inputFile.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    // Convert PDF to text using PdfPig
                    using (PdfDocument pdfDocument = PdfDocument.Open(inputFile.FullName))
                    {
                        StringWriter textWriter = new StringWriter();
                        foreach (Page page in pdfDocument.GetPages())
                        {
                            string text = ContentOrderTextExtractor.GetText(page);
                            textWriter.WriteLine(text);
                        }
                        resultText = textWriter.ToString();
                    }
                }
                //Convert docx
                else if (inputFile.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    // Convert DOCX to text using DocX
                    using (DocX doc = DocX.Load(inputFile.FullName))
                    {
                        resultText = doc.Text;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during ConvertToText.");
                return null;
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploadsTemp");
            Directory.CreateDirectory(uploadsFolder);

            // Specify the file path
            string filePath = Path.Combine(uploadsFolder, $"{Path.GetFileNameWithoutExtension(inputFile.Name)}.txt");

            // Create a FileInfo object
            FileInfo fileInfo = new FileInfo(filePath);

            using (StreamWriter writer = fileInfo.CreateText())
            {
                // Write the string to the file
                writer.Write(resultText);
            }

            return fileInfo;
        }
    }
}
