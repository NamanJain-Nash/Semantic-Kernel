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