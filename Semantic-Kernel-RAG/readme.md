# Project Overiew
<p>It is a RAG system that helps you to easily chat with your data</p>

## Requirements
<p>We Would be taking a Open Source Approach so will try Local or Hugging Face LLMs for the case of LLM and Embeddng Model and to store these Embeddings into a certain place would store these into a Local Memory based Vector DB Using QUADRANT </p>

## Deployment

<p>All the process used will be taking use of Ollama in case of </p>


model=BAAI/bge-large-en-v1.5
revision=refs/pr/5
volume=$PWD/data

docker run -p 8080:80 -v $volume:/data --pull always ghcr.io/huggingface/text-embeddings-inference:cpu-0.6 --model-id $model --revision $revision