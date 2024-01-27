﻿using Buisness_Logic.Interfaces;
using Domain.Interfaces;

namespace API.Modules
{
    public class SummarizationBasedEmbeddingHandler
    {
        private readonly ISummarizationBasedEmbeddingLogic _documentLogic;
        public SummarizationBasedEmbeddingHandler(ISummarizationBasedEmbeddingLogic documentLogic)
        {
            _documentLogic = documentLogic;
        }
        public async Task<string> DocumentToRag(IFormFileCollection files, string collection)
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

                //Sending to Buisness Logic
                if (await _documentLogic.DocumentToEmbedding(collection, fileInfoArray.ToArray()))
                {
                    return "Your Files have been Embedded";
                }
                return "File is Failed";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}