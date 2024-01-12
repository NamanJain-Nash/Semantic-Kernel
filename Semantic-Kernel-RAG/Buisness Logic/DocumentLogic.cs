using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Xceed.Words.NET;

namespace Buisness_Logic
{
    public class DocumentLogic : IDocumentLogic
    {
        private readonly ILoadMemoryService _loadMemoryService;
        public DocumentLogic(ILoadMemoryService loadMemoryService) 
        {
            _loadMemoryService = loadMemoryService;
        }
        public async Task<bool> DocumentToEmbedding(params FileInfo[] textFile) {
            try {
                //Convert to Type
                var convertedFiles = new List<FileInfo>();
                foreach (var file in textFile)
                {
                    if (file.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                       convertedFiles.Add(file);
                    }
                    else if (file.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                             file.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                    {
                        // Convert PDF and DOCX to TXT and then import
                        FileInfo textContent = ConvertToText(file);

                        if (textContent != null)
                        {
                            convertedFiles.Add(textContent);
                        }
                        else
                        {
                            return false; // Conversion failed
                        }
                    }
                    else
                    {
                        return false; // Unsupported file type
                    }
                }
                string result =await _loadMemoryService.ImportFile("test", convertedFiles.ToArray());

                if (result == "Import Done")
                {

                    return true;
                }
                return false;
            }
            catch(Exception ex) {
                return false;
            }
        }
        private FileInfo ConvertToText(FileInfo inputFile)
        {
            string resultText;
            try
            {
                if (inputFile.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    // Convert PDF to text using PdfSharp
                    using (PdfDocument pdfDocument = PdfDocument.Open(inputFile.FullName))
                    {
                        StringWriter textWriter = new StringWriter();
                        foreach (Page page in pdfDocument.GetPages())
                        {
                            string text= ContentOrderTextExtractor.GetText(page);
                            textWriter.WriteLine(text);
                        }
                        resultText= textWriter.ToString();
                    }
                }
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
                return null;
            }
            var uploadsFolder = Path.Combine("", "uploadsTemp");
            Directory.CreateDirectory(uploadsFolder);
            // Specify the file path
            string filePath = Path.Combine(uploadsFolder, (inputFile.Name.Replace(".pdf", "").Replace(".docx", "")+".txt"));
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
