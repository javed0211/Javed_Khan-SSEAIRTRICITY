set module=%1
Echo module
dotnet test .\bin\Debug\net7.0\SSEAIRTRICITY.dll --filter TestCategory=%module%
pause
