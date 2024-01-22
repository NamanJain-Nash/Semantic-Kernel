using BlingFire;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.VisualBasic;
using Services.IServices;
using Services.Service;
using Services.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Services
{
    public class SumarizationLLMService : ISumarizationLLMService
    {

        private readonly IConfiguration _config;
        private readonly ILogger<ChatService> _logger;
        private readonly IKernelBuilder _kernel;
        public readonly string _apiUrl;
        public readonly double _temprature;
        public readonly int _maxtoken;
        public readonly string _model;
        public readonly string _typeoftext;
        public readonly int _chunksize;
        private string ChatTemplate = @"You are a Summarization system that Summarize the text in Input for {{$type}} of data to minimum no of words as possible while keeping the all the key details.

```Input:{{$summarizetext}}```
Summary:";

        public SumarizationLLMService(IConfiguration config, ILogger<ChatService> logger)
        {
            _config = config;
            _chunksize = int.Parse(_config["Sumarrization:chunkingsize"] ?? "10");
            _typeoftext = _config["Sumarrization:type"] ?? "chat";
            LLMSelector.SetupLLMParameters(config, _config["LLMUsed"] ?? "", out _apiUrl, out _maxtoken, out _temprature, out _model);
            _logger = logger;
        }
        public async Task<string> SummarizeAsync(string fileName, params FileInfo[] textFile)
        {

            //Define the Kernel for LLM and its Function
            IKernelBuilder builder = Kernel.CreateBuilder();
            // Add your text generation service as a singleton instance of the Kernel
            if (_config["LLMUsed"] == "LMStudio")
            {
                builder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new LMStudioTextGenerationService(_apiUrl, _maxtoken, _temprature));
                // Add your text generation service as a factory method
                builder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new LMStudioTextGenerationService(_apiUrl, _maxtoken, _temprature));
            }
            if (_config["LLMUsed"] == "Ollama")
            {
                builder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new OllamaTextGeneration(_apiUrl, _maxtoken, _temprature, _model));
                // Add your text generation service as a factory method
                builder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new OllamaTextGeneration(_apiUrl, _maxtoken, _temprature, _model));
            }
            //Build the Kernel
            Kernel kernel = builder.Build();
            //Function Defined
            var summarizationFunction = kernel.CreateFunctionFromPrompt(ChatTemplate);

            // Import the text files.

            int fileCount = 0;

            string summarizedText = "";
            //Load Into the Memory
            foreach (FileInfo fileInfo in textFile)
            {
                fileCount++;
                // Read the text file.
                string text = File.ReadAllText(fileInfo.FullName);
                // Split the text into sentences.
                string[] sentences = BlingFireUtils.GetSentences(text).ToArray();
                //Chunking the sentences to desired size
                sentences = ChunkingTheArray(sentences, _chunksize);
                string pagetext = "";
                int sentenceCount = 0;

                foreach (string sentence in sentences)
                {
                    //Logging
                    ++sentenceCount;
                    if (sentenceCount % 10 == 0)
                    {
                        // Log progress every 10 sentences.
                        _logger.LogInformation($"[{fileCount}/{fileInfo.Length}] {fileInfo.FullName}: {sentenceCount}/{sentences.Length}");
                    }
                    //LLM Call
                    try
                    {
                        var result = await summarizationFunction.InvokeAsync(kernel, new() { ["input"] = sentence, ["type"] = _typeoftext });
                        pagetext += result.ToString();
                    }
                    catch (Exception e)
                    {
                        var k = e.Message;
                    }
                }
                summarizedText += $"{fileCount}:" + pagetext + "\n";
            }
            return summarizedText;
        }
        private static string[] ChunkingTheArray(string[] inputArray, int chunkSize)
        {
            int totalChunks = (int)Math.Ceiling((double)inputArray.Length / chunkSize);
            string[] chunks = new string[totalChunks];

            int currentIndex = 0;

            for (int i = 0; i < totalChunks; i++)
            {
                int endIndex = Math.Min(currentIndex + chunkSize, inputArray.Length);
                int chunkLength = endIndex - currentIndex;

                // Create a chunk with 10 sentences
                string[] chunk = new string[chunkLength];

                // Copy sentences to the chunk
                Array.Copy(inputArray, currentIndex, chunk, 0, chunkLength);

                // If not the last chunk, append the last sentence of the next chunk to maintain context
                if (i < totalChunks - 1)
                {
                    chunk[chunkLength - 1] += " " + inputArray[endIndex];
                }

                chunks[i] = string.Join(" ", chunk);
                currentIndex = endIndex;
            }

            return chunks;
        }
    }
}
