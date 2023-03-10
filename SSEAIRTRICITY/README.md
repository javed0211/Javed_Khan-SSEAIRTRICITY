

# SSE Airtricity
---
This is a .Net Core based automation using Playwright which practices BDD with specflow, C# for scripting and NUnit Test framework to identify tests 
Framework Details
| Automation Framework| |
| ------------- | ------------- |
|Automation Tool | Playwright|
|Proramming Language |c#|
| BDD  | Specflow  |
|IDE | Visual Studio|
| Test Framework | NUnit|
| Reporting | Allure |
| API library |  RestSharp|


## Setup
---
1. Install Node.js 
2. Open Command prompt and run command - npm install playwright
3. Clone the repository
4. Open the solution file in Visual Studio.
5. Build the solution by clicking "Build Solution" in the "Build" menu.
6. Open the Package Manager Console by clicking "Tools" > "NuGet Package Manager" > "Package Manager Console".
7. Run the following command to install the required packages:
``` 
Update-Package -reinstall 
```
6. Set the "SSEAIRTRICITY" project as the startup project by right-clicking on it in the Solution Explorer and selecting "Set as StartUp Project".

## How to execute test cases?
---
### Using Visual Studio
- Open the Test Explorer by clicking "Test" > "Windows" > "Test Explorer".
- Run the tests by clicking "Run All" in the Test Explorer. Alternatively, you can run a single test by right-clicking on it in the Test Explorer and selecting "Run Selected Tests".

### Using Dotnet commands :
1. Open a command prompt or PowerShell window and navigate to the root directory of the project.
2. Run the ExecuteTestsInParallel.ps1 PowerShell script by entering the following command:
```
.\ExecuteTestsInParallel.ps1
```
3. The script will launch several instances of the ExecuteTests.bat file, each with a different argument for filtering the tests. The tests will run in parallel, with each instance of ExecuteTests.bat running its own set of tests.
4. Once the tests have completed, you will see the results in the command prompt or PowerShell window. Any failed tests will be highlighted with an error message.

Note that the ExecuteTestsInParallel.ps1 script assumes that the ExecuteTests.bat file is located in the root directory of the project, and that the test DLL file is located in the .\bin\Debug\net7.0\ subdirectory. If your project is located in a different directory or your DLL file has a different name or location, you will need to modify the paths in the script accordingly.

## OR
Naviaget to "path of project\ToDoAPIsTests\bin\Debug\net7.0" and run following command
```
dotnet test .\SSEAIRTRICITY.dll ---filter TestCategory=Costs
```
Above command will run all tests

## Reporting
---
Following each test execution, JSON files are generated, which can be used to produce an Allure Report that contains comprehensive details about the scenario, test data utilized, and copies output as csv. The Allure Report provides an overview of the test outcomes, as well as detailed information about individual test cases, including their duration, status, and associated attachments. In addition, the report includes a summary of test suites that can be used to track test coverage and 
provide an overview of test results. It should be noted that setting up the Allure command-line interface is required to generate the report.

![Overview](https://user-images.githubusercontent.com/37189965/224274178-4642fbea-96af-4513-8b5c-0315e160b0f9.png)
![TestDetails](https://user-images.githubusercontent.com/37189965/224274257-465699a7-9952-40a2-8899-ac0dcdbd260f.png)
![OutPut](https://user-images.githubusercontent.com/37189965/224274421-38578749-9cfa-4f5e-ba52-38a0dd0cff7b.png)

## Pipeline
---
The repository contains a YAML file that is compatible with various DevOps tools such as Azure DevOps and Bamboo. The pipeline is designed to concentrate solely on testing stages and encompasses numerous features. However, it is not restricted to these stages only, and we can expand its functionality to execute additional jobs or tasks. This can also serve as an Azure DevOps template for integration with the release pipeline.

Details of Tasks:
- PowerShell Task - Sets the name of the build and updates the build number.
- NuGetToolInstaller Task - Installs the specified version of NuGet.
- DotNetCoreCLI Task - Restores project dependencies.
- PowerShell Task - Updates the browser being used for testing.
- VSBuild Task - Builds the project.
- Script Task - Runs tests on specified modules.
- AllureGenerate Task - Generates an Allure report.
- PowerShell Task - Attaches an Allure report.
- PublishBuildArtifacts Task - Publishes build artifacts.

## Videos And Images
---
The framework is designed to capture videos during runtime and take screenshots whenever there is a test case failure. The videos can be played on any modern web browser.
