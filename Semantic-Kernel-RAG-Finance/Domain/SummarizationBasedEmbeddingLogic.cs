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
                var convertedFiles = await Task.WhenAll(textFiles.Select(async textFile =>
                {
                    if (textFile.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        return textFile;
                    }
                    else if (textFile.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                             textFile.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                    {
                        FileInfo textContent =  Filecoverter.ConvertToText(textFile, _logger);

                        if (textContent != null)
                        {
                            return textContent;
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
                }));

                var importResults = await Task.WhenAll(convertedFiles.Select(async convertedFile =>
                {
                    string summarieseData = await _summaryService.SummarizeAsync(convertedFile);
                    string filePath = convertedFile.FullName;

                    // Use async methods for file operations
                    if (File.Exists(filePath))
                    {
                        await File.WriteAllTextAsync(filePath, string.Empty);
                        await File.WriteAllTextAsync(filePath, summarieseData);
                    }

                    return await _loadMemoryService.ImportFileAsync(collection, convertedFile);
                }));

                // Check if at least one import operation was successful
                if (importResults.Any(result => result != "Import Done"))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during DocumentToEmbedding.");
                return false;
            }
        }
    }

}
