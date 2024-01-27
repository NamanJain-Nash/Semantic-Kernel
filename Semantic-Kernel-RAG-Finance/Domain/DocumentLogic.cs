using Buisness_Logic.Utils;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Services;
using Services.IService;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Xceed.Words.NET;

namespace Domain
{
    public class DocumentLogic : IDocumentLogic
    {
        private readonly ILoadMemoryService _loadMemoryService;
        private readonly ILogger<DocumentLogic> _logger;

        public DocumentLogic(ILoadMemoryService loadMemoryService, ILogger<DocumentLogic> logger)
        {
            _loadMemoryService = loadMemoryService;
            _logger = logger;
        }

        public async Task<bool> DocumentToEmbedding(string collection, params FileInfo[] textFiles)
        {
            try
            {
                // Filter supported files and convert asynchronously
                var convertedFiles = await Task.WhenAll(textFiles
                    .AsParallel()
                    .Select(async textFile =>
                    {
                        if (textFile.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                        {
                            return textFile;
                        }
                        else if (textFile.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                                 textFile.Extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                        {
                            // Convert PDF and DOCX to TXT asynchronously
                            return await Task.Run(() => Filecoverter.ConvertToText(textFile, _logger));
                        }
                        else
                        {
                            return null; // Unsupported file type
                        }
                    }));

                // Filter out null results (unsupported files)
                convertedFiles = convertedFiles.Where(file => file != null).ToArray();
                // Import files in parallel
                var importTasks = convertedFiles.Select(convertedFile =>
                    _loadMemoryService.ImportFileAsync(collection, convertedFile));

                // Wait for all import tasks to complete
                string[] importResults = await Task.WhenAll(importTasks);

                // Check import results
                if (importResults.All(result => result == "Import Done"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during DocumentToEmbedding.");
                return false;
            }
        }
    }
}
