using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextGeneration;
namespace Semantic_Kernel;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Testing Tiny Lama");
        Console.WriteLine("\n======== Custom LLM - Text Completion - KernelFunction ========");
        //Using LM Studio
        //Intitalizing The Kernel
        IKernelBuilder lmStudiobuilder = Kernel.CreateBuilder();
        // Add your text generation service as a singleton instance of the Kernel
        lmStudiobuilder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new LMStudioTextGenerationService("url",-1,0.1));
        // Add your text generation service as a factory method
        lmStudiobuilder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new LMStudioTextGenerationService("url",-1,0.1));
        //Build the Kernel
        Kernel kernel = lmStudiobuilder.Build();
        //Define The Prompt
        const string FunctionDefinition = "Write Step By Step Answer on {{$input}}";
        //Function Defined
        var paragraphWritingFunction = kernel.CreateFunctionFromPrompt(FunctionDefinition);
        //Question Asked
        const string Input = "What is the Capital of France";
        Console.WriteLine($"Function input: {Input}\n");
        //Run the Prompt
        try{
        var result =  await paragraphWritingFunction.InvokeAsync(kernel, new() { ["input"] = Input });
        Console.WriteLine("Lmstudioresponse: "+result);}
        catch(Exception e){
            Console.WriteLine(e.Message);}

        //Using Ollama
        IKernelBuilder ollamabuilder = Kernel.CreateBuilder();
        // Add your text generation service as a singleton instance of the Kernel
        ollamabuilder.Services.AddKeyedSingleton<ITextGenerationService>("myService1", new OllamaTextGeneration("url",-1,0.1,"model"));
        // Add your text generation service as a factory method
        ollamabuilder.Services.AddKeyedSingleton<ITextGenerationService>("myService2", (_, _) => new OllamaTextGeneration("url",-1,0.1,"model"));
        //Build the Kernel
        kernel = ollamabuilder.Build();
        paragraphWritingFunction = kernel.CreateFunctionFromPrompt(FunctionDefinition);
        Console.WriteLine($"Function input: {Input}\n");
        //Run the Prompt
        try{
        var result =  await paragraphWritingFunction.InvokeAsync(kernel, new() { ["input"] = Input });
        Console.WriteLine("Ollama Response: "+result);}
        catch(Exception e){
            Console.WriteLine(e.Message);}

        
    }
    }
