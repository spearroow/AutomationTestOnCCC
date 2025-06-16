NuGet Package Dependencies

Core Testing Packages
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="NUnit" Version="3.14.0" />
<PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
<PackageReference Include="NUnit.Analyzers" Version="3.9.0" />

Selenium WebDriver Packages
<PackageReference Include="Selenium.WebDriver" Version="4.15.0" />
<PackageReference Include="Selenium.WebDriver.ChromeDriver" Version="119.0.6045.10500" />
<PackageReference Include="Selenium.Support" Version="4.15.0" />

Configuration & Utilities
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />

Installation Steps

Step 1
dotnet --version

Step 2
mkdir WebTestSuite
cd WebTestSuite

Step 3
dotnet new nunit
dotnet restore

Step 4
Install Google Chrome (latest stable)

Step 5
dotnet build
dotnet test --filter "TestSuite_HealthCheck"

Additional Browser Support
<PackageReference Include="Selenium.WebDriver.GeckoDriver" Version="0.33.0" />
<PackageReference Include="Selenium.WebDriver.MSEdgeDriver" Version="119.0.2151.58" />

dotnet new --install NUnit3.DotNetNew.Template
dotnet tool install --global dotnet-reportgenerator-globaltool

Dockerfile Example
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list \
    && apt-get update \
    && apt-get install -y google-chrome-stable
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet build
CMD ["dotnet", "test", "--logger", "console"]

Troubleshooting

ChromeDriver Version Mismatch
google-chrome --version
dotnet add package Selenium.WebDriver.ChromeDriver --version [matching-version]

.NET Version Issues
dotnet --list-sdks
dotnet new globaljson --sdk-version 8.0.100

Missing Dependencies
dotnet clean
dotnet restore
dotnet build

GitHub Actions
name: Web Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal

Parallel Test Execution
<PropertyGroup>
  <ParallelizeTestCollections>true</ParallelizeTestCollections>
  <MaxCpuCount>4</MaxCpuCount>
</PropertyGroup>
