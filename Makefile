.PHONY: run build test test-unit test-arch clean

run:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000

build:
	dotnet build Corretora.sln

test:
	dotnet test Corretora.sln

test-unit:
	dotnet test tests/Corretora.UnitTests --logger "console;verbosity=normal"

test-arch:
	dotnet test tests/Corretora.ArchitectureTests --logger "console;verbosity=normal"

clean:
	dotnet clean Corretora.sln
