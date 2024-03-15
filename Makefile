OUTPUT = ipk24chat-client

BUILD_NAME = ipk24chat-client
SRC_DIR = ./src/ChatApp
OUT_DIR = .

all: build 

build:
	dotnet build -c Release /p:AssemblyName=$(BUILD_NAME)
	mv $(SRC_DIR)/bin/Release/net8.0

run:
	mono $(OUT_DIR)/$(OUTPUT)

clean:
	rm -rf $(OUT_DIR)

help:
	@echo "Available targets:"
	@echo "  - build:   Build the C# application"
	@echo "  - run:     Run the C# application"
	@echo "  - clean:   Clean the build"
	@echo "  - help:    Display this help message"
