# Project Overiew
<p>This project is an API-based RAG system designed to facilitate communication with text-based PDFs, Word documents, and text files. It leverages Language Model (LLMs) and Embeddings that run locally, ensuring user privacy and control.</p>

## Architecture
<img src="https://github.com/NamanJain-Nash/Semantic-Kernel/blob/Development/Semantic-Kernel-RAG/Images/Architecture.png" alt="Architecture"/>

## Requirements
<p>All Open Sourced LLMs , Vector DB , Models and Tools are Genereally prefered and have used Docker Compose that will start the application to easily</p>

### Components

- .Net API
- Qdrant (Vector Database)
- Hugging Face Embeddings
- Ollama (LLM Provider)


## Start the application

<p>Before starting the application, need to decide which llm , vector model a person want.</p>

### Start LLM
<p>In Application their are major 2 ways to use the LLM that is LMStudio and OLLama and both are configured in appsettigs.json file that can make it easy to use and with no other cofiguration needed.</p>

#### LM Studio
<p>We can run using its own application and then start its server using the application.</p>

#### Ollama
<p>We can directly use the Ollama that can start using the docker compose easilt and then using the command below.</p>

`docker exec -it ollama ollama pull <model name> `

### Start Embedding Model
<p>The Embedding model by default is setup but can changed in the docker compose of the model name. </p>

### Start Qdrant and .Net API
<p>By running the docker compose one can start the application easily.</p>

## Use The APPLICATION
<p>Using Docker Compose a person can start the application after doing all the neccesarry steps.</p>

<p>Use the Below curls to use the application various steps </p>

### CURLS


#### Get Token For File Sending

`curl --location 'http://localhost:3255/antiforgery/token' `

#### Send File To The RAG

```bash
curl --location 'http://localhost:3255/api/file?collection=<CollectionName>' \
--header 'X-XSRF-TOKEN:<Token> ' \
--form '=@"/test.pdf"' 
```

<b>Chat With DATA</b>
``` bash
curl --location 'http://0.0.0.0:3255/api/chat' \
--header 'accept: */*' \
--header 'Content-Type: application/json' \
--data '{
  "chatId": "6abdc",
  "collectionName": "rustdocs",
  "userQuery": "what is cargo"
}' 
```

### Note

<i>Can increase or decrease the Embeddings volume using the Search Paramaeters by changing the limit or the Releveance of them.</i>

<i>The Docker Compose are CPU only to allow GPU need to Use the Docker Files and Configuration of GPUs</i>


## Example

## `Question : what is Cargo In Rust`

### Normal LLM Answer

`Cargo in Rust refers to the process of transferring data, such as structured or unstructured data, between different processing components. Cargo can be either transmitted using network protocols like HTTP, TCP/IP or UDP, or stored as binary files in a database like PostgreSQL, MySQL, or MongoDB. Cargo is typically used for moving large amounts of data from one system to another or passing information between various parts of an application. In Rust, it can also be used for streaming data, such as real-time data feeds or machine learning datasets.`

### RAG Application Answer

`Cargo is RusT's build system and package manager. It automates the process of building, managing, and distributing packages (i.e., binaries or libraries) in a project that uses RusT. This helps to organize and manage projects using RusT by creating a top-level project directory, containing source code files, README files, license information, configuration files, and anything else that is not related to the project's code. By using Cargo, developers can easily organize their projects into packages that provide functionality, and convert existing projects to use Cargo instead of building them manually.`


<i><u>In this Sample have used TinnyLLama with Rust Playbook.</u></i>
