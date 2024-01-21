using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace ChatAPI.Modules
{
    public class SummarizationHandler
    {
        private readonly ISummarizationLogic _documentHandler;
        public SummarizationHandler( ISummarizationLogic documentLogic)
        {
            _documentHandler = documentLogic;
        }
        public async Task<string> DocumentToSummarization(IFormFileCollection files, string collection)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return "No files uploaded.";
                if (collection == "")
                {
                    return "Please provide a valid collection name";
                }
                var allowedExtensions = new[] { ".txt", ".pdf", ".docx" };

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileInfoArray = new List<FileInfo>();

                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                        return "Only files with extensions .txt, .pdf, or .docx are allowed.";

                    var filePath = Path.Combine(uploadsFolder, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileInfo = new FileInfo(filePath);
                    fileInfoArray.Add(fileInfo);
                }

                //Sending to Logic
                return (await _documentHandler.DocumentToSummarization(collection, fileInfoArray.ToArray()))
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}

