APP_NAME = ipk24chat-client
SRC_DIR = ./ChatApp
BIN = bin
OBJ = obj

.PHONY: clean build

all: build

build: 
	dotnet publish -c Release /p:DebugType=None -o .
	
run: build
	./$(APP_NAME) -t tcp -s 127.0.0.1 -p 4567

discord: build
	./$(APP_NAME) -t tcp -s anton5.fit.vutbr.cz -p 4567

help: build
	./$(APP_NAME) -h

restore:
	dotnet nuget locals all --clear
	dotnet restore --verbosity diagnostic
	

clean:
	rm -rf $(APP_NAME)
	rm -rf $(SRC_DIR)/$(BIN)
	rm -rf $(SRC_DIR)/$(OBJ)


