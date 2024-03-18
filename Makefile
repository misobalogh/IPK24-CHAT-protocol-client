APP_NAME = ipk24chat-client
SRC_DIR = ./src/ChatApp
BIN = bin
OBJ = obj

.PHONY: clean build

all: build

build: 
	dotnet publish -c Release /p:DebugType=None -o .
	
run: build
	./ipk24chat-client -t tcp -s 4040 

help: build
	./ipk24chat-client -h

restore:
	dotnet nuget locals all --clear
	dotnet restore --verbosity diagnostic
	

clean:
	rm -rf $(APP_NAME)
	rm -rf $(SRC_DIR)/$(BIN)
	rm -rf $(SRC_DIR)/$(OBJ)


