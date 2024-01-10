# Use a base image with Rust installed
FROM rust:latest

# Install required dependencies
RUN apt-get update && \
    apt-get install -y libssl-dev gcc

# Create a directory for your application
WORKDIR /app

# Copy the router code into the container
COPY router /app/router

# Build the router code
RUN cargo install --path /app/router -F candle -F mkl

# Set the default command to run the text-embeddings-router
CMD ["text-embeddings-router", "--model-id", "BAAI/bge-large-en-v1.5", "--revision", "refs/pr/5", "--port", "8080"]
