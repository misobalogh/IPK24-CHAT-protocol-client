PROJECT_PATH = ./src/ChatApp/ChatApp.csproj

all: build
	dotnet run --project $(PROJECT_PATH)

build:
	dotnet build $(PROJECT_PATH)
