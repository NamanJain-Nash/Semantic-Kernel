using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Utils
{
    public static class LLMSelector
    {
        public static void SetupLLMParameters(IConfiguration config, string llmUsed, out string apiUrl, out int maxtoken, out double temprature, out string model)
        {
            apiUrl = "";
            maxtoken = -1;
            temprature = 0.1;
            model = "tinyllama";

            if (llmUsed == "LMStudio" || llmUsed == "Ollama")
            {
                apiUrl = config[$"LLM_{llmUsed}:endpoint"] ?? "";
                maxtoken = int.Parse(config[$"LLM_{llmUsed}:maxtoken"] ?? "-1");
                temprature = double.Parse(config[$"LLM_{llmUsed}:temprature"] ?? "0.1");

                if (llmUsed == "Ollama")
                {
                    model = config["LLM_Ollama:model"] ?? "tinyllama";
                }
            }
        }
    }
}
