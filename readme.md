Step 1 dotnet --version

Step 2 mkdir WebTestSuite cd WebTestSuite

Step 3 dotnet new nunit dotnet restore

Step 4 Install Google Chrome (latest stable)

Step 5 dotnet build dotnet test --filter "TestSuite_HealthCheck"
