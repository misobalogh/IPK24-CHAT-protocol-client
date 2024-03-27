APP_NAME = ipk24chat-client
SRC_DIR = ./src/ChatApp
BIN = bin
OBJ = obj
TEST_DIR = ./tests/ChatAppTests

.PHONY: clean build test

all: build

build: 
	dotnet publish -c Release /p:DebugType=None -o .



run: build
	./$(APP_NAME) -t tcp -s 127.0.0.1 -p 4567

tcp: build
	./$(APP_NAME) -t tcp -s 127.0.0.1 -p 4567

udp: build
	./$(APP_NAME) -t udp -s 127.0.0.1 -p 4567

discord: build
	./$(APP_NAME) -t udp -s anton5.fit.vutbr.cz -p 4567

help: build
	./$(APP_NAME) -h

test: build buildTests
	dotnet test $(TEST_DIR)

buildTests:
	dotnet build -c Debug /p:DebugType=None

restore:
	dotnet nuget locals all --clear
	dotnet restore --verbosity diagnostic

clear:
	dotnet nuget locals all --clear

clean:
	rm -rf $(APP_NAME)
	rm -rf $(SRC_DIR)/$(BIN)
	rm -rf $(SRC_DIR)/$(OBJ)
	rm -rf $(TEST_DIR)/$(BIN)
	rm -rf $(TEST_DIR)/$(OBJ)
