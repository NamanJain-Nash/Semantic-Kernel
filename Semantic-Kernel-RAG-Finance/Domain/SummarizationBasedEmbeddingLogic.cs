using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buisness_Logic.Interfaces;
using Buisness_Logic.Utils;
using Domain;
using Microsoft.Extensions.Logging;
using Services;
using Buisness_Logic.Utils;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
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
using Services.IServices;
using System.IO;
using Microsoft.AspNetCore.Http.HttpResults;
using Services.IService;
namespace Buisness_Logic
{
    public class SummarizationBasedEmbeddingLogic : ISummarizationBasedEmbeddingLogic
    {
        private readonly ILoadMemoryService _loadMemoryService;
        private readonly ILogger<DocumentLogic> _logger;
        private readonly ISumarizationLLMService _summaryService;

        public SummarizationBasedEmbeddingLogic(ILoadMemoryService loadMemoryService, ISumarizationLLMService summaryService, ILogger<DocumentLogic> logger)
        {
            _loadMemoryService = loadMemoryService;
            _logger = logger;
            _summaryService = summaryService;
        }
        public async Task<bool> DocumentToEmbedding(string collection, params FileInfo[] textFiles)
        {
            try
            {
                // Convert to Type
                var convertedFiles = new List<FileInfo>();
                Parallel.ForEach(textFiles, textFile =>
        {
            if (textFile.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                convertedFiles.Add(textFile);
            }
            else if (textFile.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                     textFile.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                FileInfo textContent = Filecoverter.ConvertToText(textFile, _logger);

                if (textContent != null)
                {
                    convertedFiles.Add(textContent);
                }
                else
                {
                    // Handle conversion failure
                    throw new InvalidOperationException("Conversion failed for file: " + textFile.FullName);
                }
            }
            else
            {
                // Handle unsupported file type
                throw new NotSupportedException("Unsupported file type: " + textFile.Extension);
            }
        });
                //Summarize and modify
                foreach (var convertedFile in convertedFiles)
                {
                    string summarieseData = await _summaryService.SummarizeAsync(convertedFile);
                    string filePath = convertedFile.FullName;
                    if (File.Exists(filePath))
                    {
                        // Clear the existing content of the file
                        File.WriteAllText(filePath, string.Empty);

                        // Write the new content to the file
                        File.WriteAllText(filePath, summarieseData);
                    }
                }
                string result = await _loadMemoryService.ImportFileAsync(collection, convertedFiles.ToArray());

                if (result == "Import Done")
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during DocumentToEmbedding.");
                return false;
            }
        }
    }
}
