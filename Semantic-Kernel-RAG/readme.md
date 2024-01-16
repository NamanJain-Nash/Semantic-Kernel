# Project Overiew
<p>It is a RAG system that helps you to easily chat with your data</p>

## Requirements
<p>We Would be taking a Open Source Approach so will try Local or Hugging Face LLMs for the case of LLM and Embeddng Model and to store these Embeddings into a certain place would store these into a Local Memory based Vector DB Using QDRANT </p>

## Deployment

<p>In This we can define both the LMStudio and The docker based OLLama also. </p>

<p>To start the ollama you just need to start the docker compose using command ie docker compise up -d and the using command [docker exec -it ollama ollama pull <Model name> ] by design in the case of the use case i have used tinnylamma</p>.


<p>After starting the docker compose whole project is ready and setup to question with your data</p>

### CURLS
<p>To get the token <p>
[curl --location 'http://localhost:3255/antiforgery/token']

After getting the token we would need to start the embeding using the Upload file 

[curl --location 'http://localhost:3255/api/file?collection=<CollectionName>' \
--header 'X-XSRF-TOKEN:<Token> ' \
--form '=@"/test.pdf"']

After this can chat with the data when got the embeddings are done
[curl --location 'http://0.0.0.0:3255/api/chat' \
--header 'accept: */*' \
--header 'Content-Type: application/json' \
--data '{
  "chatId": "6abdc",
  "collectionName": "rustdocs",
  "userQuery": "what is cargo"
}']

### Note

<p>According to your data and its debth of it we might need to change the relevance and limit score so that the document context is more accurate</p>

<p>With GPu sytem might need to change the Docker Compose file of the same to easily done the process</p>


## Powe of RAG
Use rust playbook for this case
Question : what is cargo
### No Data Base Answer
Answer :-<p>Cargo in Rust refers to the process of transferring data, such as structured or unstructured data, between different processing components. Cargo can be either transmitted using network protocols like HTTP, TCP/IP or UDP, or stored as binary files in a database like PostgreSQL, MySQL, or MongoDB. Cargo is typically used for moving large amounts of data from one system to another or passing information between various parts of an application. In Rust, it can also be used for streaming data, such as real-time data feeds or machine learning datasets.</p> 

### With Data Answer

<p>Cargo is RusT's build system and package manager. It automates the process of building, managing, and distributing packages (i.e., binaries or libraries) in a project that uses RusT. This helps to organize and manage projects using RusT by creating a top-level project directory, containing source code files, README files, license information, configuration files, and anything else that is not related to the project's code. By using Cargo, developers can easily organize their projects into packages that provide functionality, and convert existing projects to use Cargo instead of building them manually.</p>


<p>We can see that RAG solve many hallisunation and less bounded nature of LLMs as with no data each time it start to give variety of data even after mentioning rust and if we ignore that then </p>