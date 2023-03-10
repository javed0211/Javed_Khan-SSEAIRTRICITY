
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json;
using SSEAIRTRICITY.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TechTalk.SpecFlow;
using SSEAIRTRICITY;
using TestPlan = Microsoft.TeamFoundation.TestManagement.WebApi.TestPlan;
using TestPoint = Microsoft.TeamFoundation.TestManagement.WebApi.TestPoint;

namespace Utilities.AzureDevOpsAPIs
{
    public class AzureDevOpsAPIs
    {

        private readonly string TFUrl = "https://bupaukmudynamics.visualstudio.com/"; // for devops azure 
        private readonly string UserPAT; 
        string dynamicQuery = @"SELECT [System.Id] FROM workitems 
                                WHERE [System.WorkItemType] IN GROUP 'Microsoft.TestCaseCategory' 
                                AND [Id] = 88720 ";

        private static WorkItemTrackingHttpClient WitClient;
        private BuildHttpClient BuildClient;
        private ProjectHttpClient ProjectClient;
        private TestManagementHttpClient TestManagementClient;
        ScenarioContext _scenarioContext;
        private static TestPlanHttpClient TestPlanClient;
        private readonly string _projectId;
        private readonly int _testSuiteId;
        const string FieldSteps = "Microsoft.VSTS.TCM.Steps";
        const string FieldParameters = "Microsoft.VSTS.TCM.Parameters";
        const string FieldDataSource = "Microsoft.VSTS.TCM.LocalDataSource";
        AzureAPIServices AzureInfo;
        VssConnection connection;

        public AzureDevOpsAPIs(ScenarioContext ScenarioContext)
        {
            StreamReader SR = new StreamReader(Directory.GetCurrentDirectory() + "\\AzureAPIConfig.json");
            AzureInfo = JsonConvert.DeserializeObject<AzureAPIServices>(SR.ReadToEnd());
            UserPAT = AzureInfo.AccessToken;
            _scenarioContext = ScenarioContext;
            connection = new VssConnection(new Uri(AzureInfo.EnvironmentURL), new VssBasicCredential(string.Empty, UserPAT));
            WitClient = connection.GetClient<WorkItemTrackingHttpClient>();
            BuildClient = connection.GetClient<BuildHttpClient>();
            ProjectClient = connection.GetClient<ProjectHttpClient>();
            TestManagementClient = connection.GetClient<TestManagementHttpClient>();
            TestPlanClient = connection.GetClient<TestPlanHttpClient>();
        }

        /// <summary>
        /// Create successed test run
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanId"></param>
        /// <param name="StaticSuitePath"></param>
        /// <param name="TestCaseIds"></param>
        public void CreateTestResultCompleted(string TeamProjectName, int TestPlanId, int testSuiteId, List<int> TestCaseIds, string TestcaseName, string OwnerName)
        {
            TestPlan testPlan = TestManagementClient.GetPlanByIdAsync(TeamProjectName, TestPlanId).Result;
            var testPlanRef = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference(testPlan.Id.ToString(), testPlan.Name, testPlan.Url);

            RunCreateModel runCreate = new RunCreateModel(
                name: "Test run from Automation - completed",
                plan: testPlanRef,
                startedDate: DateTime.Now.ToString("o"),
                isAutomated: true

                );

            TestRun testRun = TestManagementClient.CreateTestRunAsync(runCreate, TeamProjectName).Result;
            List<TestCaseResult> testResults = new List<TestCaseResult>();
            foreach (int testCaseId in TestCaseIds)
                testResults.Add(PassedTest(TeamProjectName, TestPlanId, testSuiteId, testCaseId, TestcaseName, OwnerName));

            TestManagementClient.AddTestResultsToTestRunAsync(testResults.ToArray(), TeamProjectName, testRun.Id).Wait();

            RunUpdateModel runUpdateModel = new RunUpdateModel(
                completedDate: DateTime.Now.ToString("o"),


                state: Enum.GetName(typeof(TestRunState), TestRunState.Completed)
                );

            testRun = TestManagementClient.UpdateTestRunAsync(runUpdateModel, TeamProjectName, testRun.Id).Result;
            PrintBasicRunInfo(testRun);
        }

        public IEnumerable<TeamProjectReference> GetProject(string projectName)
        {
            return ProjectClient.GetProjects().Result.Where(x => x.Name == projectName);
        }

        public void CreateTestCaseIfNotExists(string projectId, int testPlanId, int testSuiteId, List<string> stepInfo)
        {
            // Get the scenario details
            var scenarioInfo = _scenarioContext.ScenarioInfo;
            var scenarioTitle = scenarioInfo.Title;
            var scenarioDescription = scenarioInfo.Description;
            var scenarioPriority = "High";

            // Check if a test case with the same title already exists in the test suite
            var testCaseId = GetTestCaseIdByTitle(UserPAT, projectId, testPlanId, testSuiteId, scenarioTitle);

            if (testCaseId == 0)
            {
                // Create a new test case for the scenario
                testCaseId = CreateTest(AzureInfo.ProjectName, stepInfo);

                // Add the test case ID to the scenario context so that it can be used in subsequent steps
                _scenarioContext["TestCaseId"] = testCaseId;
                Console.WriteLine(testCaseId);
            }
        }

        public int GetTestCaseIdByTitle(string personalAccessToken, string projectId, int testPlanId, int testSuiteId, string testCaseTitle)
        {
            // Get the list of test cases in the specified test suite
            var testCases = TestManagementClient.GetTestCasesAsync(projectId, testPlanId, testSuiteId).Result;

            // Find the test case with the specified title
            var testCase = testCases.FirstOrDefault(tc => tc.Workitem.Name == testCaseTitle);

            if (testCase != null)
            {
                return Convert.ToInt32(testCase.Workitem.Id);
            }
            else
            {
                return 0;
            }

        }

        /// <summary>
        /// Create a simple test case
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <returns></returns>
        public int CreateTest(string TeamProjectName, List<string> stepInfo)
        {
            Dictionary<string, object> fields = new Dictionary<string, object>();

            LocalStepsDefinition stepsDefinition = new LocalStepsDefinition();
            foreach (var step in stepInfo)
            {
                stepsDefinition.AddStep($"{step}");
            }

            LocalTestParams testParams = new LocalTestParams();

            fields.Add("Title", _scenarioContext.ScenarioInfo.Title);
            fields.Add(FieldSteps, stepsDefinition.StepsDefinitionStr);
            fields.Add("System.AreaPath", @"\\Autothon");

            return CreateWorkItem(TeamProjectName, "Test Case", fields).Id.Value;
        }

        /// <summary>
        /// Create a work item
        /// </summary>
        /// <param name="ProjectName"></param>
        /// <param name="WorkItemTypeName"></param>
        /// <param name="Fields"></param>
        /// <returns></returns>
        static Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem CreateWorkItem(string ProjectName, string WorkItemTypeName, Dictionary<string, object> Fields)
        {
            JsonPatchDocument patchDocument = new JsonPatchDocument();
            foreach (var key in Fields.Keys)
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/" + key,
                    Value = Fields[key]
                });
            return WitClient.CreateWorkItemAsync(patchDocument, ProjectName, WorkItemTypeName).Result;
        }

        /// <summary>
        /// Create failed test run
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanId"></param>
        /// <param name="StaticSuitePath"></param>
        /// <param name="TestCaseIds"></param>
        public void CreateTestResultFailed(string TeamProjectName, int TestPlanId, int testSuiteId, List<int> TestCaseIds, string TestCaseName, string Error, string OwnerName)
        {
            TestPlan testPlan = TestManagementClient.GetPlanByIdAsync(TeamProjectName, TestPlanId).Result;

            var testPlanRef = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference(testPlan.Id.ToString(), testPlan.Name, testPlan.Url);

            RunCreateModel runCreate = new RunCreateModel(
                name: "Test run from Automation - failed",
                plan: testPlanRef,
                startedDate: DateTime.Now.ToString("o"),
                isAutomated: true
                );

            TestRun testRun = TestManagementClient.CreateTestRunAsync(runCreate, TeamProjectName).Result;

            List<TestCaseResult> testResults = new List<TestCaseResult>();

            foreach (int testCaseId in TestCaseIds)
                testResults.Add(FailedTest(TeamProjectName, TestPlanId, testSuiteId, testCaseId, testRun.Id, TestCaseName, Error, OwnerName));

            testResults = TestManagementClient.AddTestResultsToTestRunAsync(testResults.ToArray(), TeamProjectName, testRun.Id).Result;

            var definedTestResults = TestManagementClient.GetTestResultsAsync(TeamProjectName, testRun.Id).Result;

            RunUpdateModel runUpdateModel = new RunUpdateModel(
                errorMessage: "Test failed",
                completedDate: DateTime.Now.ToString("o"),
                state: Enum.GetName(typeof(TestRunState), TestRunState.NeedsInvestigation)
                );

            testRun = TestManagementClient.UpdateTestRunAsync(runUpdateModel, TeamProjectName, testRun.Id).Result;

            PrintBasicRunInfo(testRun);
        }

        /// <summary>
        /// Create struct for passed test
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanId"></param>
        /// <param name="StaticSuitePath"></param>
        /// <param name="TestCaseId"></param>
        /// <returns></returns>
        public TestCaseResult PassedTest(string TeamProjectName, int TestPlanId, int testSuiteId, int TestCaseId, string TestcaseName, string OwnerName)
        {
            // int testSuiteId = GetSuiteId(TeamProjectName, TestPlanId, StaticSuitePath);
            TestPoint testPoint = TestManagementClient.GetPointsAsync(TeamProjectName, TestPlanId, testSuiteId, testCaseId: TestCaseId.ToString()).Result.FirstOrDefault();

            TestCaseResult testCaseResult = new TestCaseResult();
            testCaseResult.Outcome = Enum.GetName(typeof(TestOutcome), TestOutcome.Passed);
            testCaseResult.TestPoint = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference(testPoint.Id.ToString(), url: testPoint.Url);
            testCaseResult.CompletedDate = DateTime.Now;
            testCaseResult.State = Enum.GetName(typeof(TestRunState), TestRunState.Completed);
            testCaseResult.AutomatedTestName = "Executed via Automation";
            testCaseResult.TestCase = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference(id: TestCaseId.ToString());
            testCaseResult.TestCaseRevision = 12345;
            testCaseResult.TestCaseTitle = TestcaseName;
            testCaseResult.RunBy = new IdentityRef() { DisplayName = "anurag-palkar.bansod" };

            return testCaseResult;
        }

        /// <summary>
        /// Create sruct for failed test
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanId"></param>
        /// <param name="StaticSuitePath"></param>
        /// <param name="TestCaseId"></param>
        /// <param name="TestRunId"></param>
        /// <returns></returns>
        public TestCaseResult FailedTest(string TeamProjectName, int TestPlanId, int testSuiteId, int TestCaseId, int TestRunId, string TestCaseName, string Error, string OwnerName)
        {
            //GetSuiteId(TeamProjectName, TestPlanId, StaticSuitePath);
            TestPoint testPoint = TestManagementClient.GetPointsAsync(TeamProjectName, TestPlanId, testSuiteId, testCaseId: TestCaseId.ToString()).Result.FirstOrDefault();

            TestCaseResult testCaseResult = new TestCaseResult();
            testCaseResult.Outcome = Enum.GetName(typeof(TestOutcome), TestOutcome.Failed);
            testCaseResult.TestPoint = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference(testPoint.Id.ToString(), url: testPoint.Url);
            testCaseResult.CompletedDate = DateTime.Now;
            testCaseResult.ErrorMessage = "Test Case " + TestCaseId + " failed";
            testCaseResult.State = Enum.GetName(typeof(TestRunState), TestRunState.Completed);
            testCaseResult.StackTrace = Error;
            testCaseResult.TestCase = new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference(id: TestCaseId.ToString());
            testCaseResult.TestCaseRevision = 12345;
            testCaseResult.TestCaseTitle = TestCaseName;
            testCaseResult.TestCaseReferenceId = TestCaseId;
            testCaseResult.RunBy = new IdentityRef() { DisplayName = "Javed.Khan" };


            return testCaseResult;
        }

        /// <summary>
        /// Get ID on an existing test suite by path in test plan
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanId"></param>
        /// <param name="SuitePath"></param>
        /// <returns></returns>
        static int GetSuiteId(string TeamProjectName, int TestPlanId, string SuitePath)
        {
            Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan testPlan = TestPlanClient.GetTestPlanByIdAsync(TeamProjectName, TestPlanId).Result;
            if (SuitePath == "") return testPlan.RootSuite.Id;

            List<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite> testPlanSuites = TestPlanClient.GetTestSuitesForPlanAsync(TeamProjectName, TestPlanId, Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.SuiteExpand.Children, asTreeView: true).Result;

            string[] pathArray = SuitePath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

            Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite suiteMarker = testPlanSuites[0]; //first level is the root suite

            for (int i = 0; i < pathArray.Length; i++)
            {
                suiteMarker = (from ts in suiteMarker.Children where ts.Name == pathArray[i] select ts).FirstOrDefault();

                if (suiteMarker == null) return 0;

                if (i == pathArray.Length - 1) return suiteMarker.Id;
            }

            return 0;
        }

        public void PrintBasicRunInfo(TestRun testRun)
        {
            Console.WriteLine("Information for test run:" + testRun.Id);
            Console.WriteLine("Automated - {0}; Start Date - '{1}'; Completed date - '{2}'", (testRun.IsAutomated) ? "Yes" : "No", testRun.StartedDate.ToString(), testRun.CompletedDate.ToString());
            Console.WriteLine("Total tests - {0}; Passed tests - {1}", testRun.TotalTests, testRun.PassedTests);
        }

        /// <summary>
        /// Create a new test plan
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanName"></param>
        /// <param name="StartDate"></param>
        /// <param name="FinishDate"></param>
        /// <param name="AreaPath"></param>
        /// <param name="IterationPath"></param>
        /// <returns></returns>
        public int CreateTestPlan(string TeamProjectName, string TestPlanName, DateTime? StartDate = null, DateTime? FinishDate = null, string AreaPath = "", string IterationPath = "")
        {
            if (IterationPath != "") IterationPath = TeamProjectName + "\\" + IterationPath;
            if (AreaPath != "") AreaPath = TeamProjectName + "\\" + AreaPath;

            TestPlanCreateParams newPlanDef = new TestPlanCreateParams()
            {
                Name = TestPlanName,
                StartDate = StartDate,
                EndDate = FinishDate,
                AreaPath = AreaPath,
                Iteration = IterationPath
            };

            return TestPlanClient.CreateTestPlanAsync(newPlanDef, TeamProjectName).Result.Id;
        }

        /// <summary>
        /// Create a new test suite
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanId"></param>
        /// <param name="TestSuiteName"></param>
        /// <param name="SuiteType"></param>
        /// <param name="ParentPath"></param>
        /// <param name="SuiteQuery"></param>
        /// <param name="RequirementIds"></param>
        /// <returns></returns>
        public Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite CreateTestSuite(string TeamProjectName, int TestPlanId, int ParentSuiteId, string TestSuiteName = "", TestSuiteType SuiteType = TestSuiteType.StaticTestSuite, string ParentPath = "", string SuiteQuery = "", int RequirementId = 0)
        {
            switch (SuiteType)
            {
                case TestSuiteType.StaticTestSuite:
                    if (TestSuiteName == "") { Console.WriteLine("Set the name for the test suite"); return null; }
                    break;
                case TestSuiteType.DynamicTestSuite:
                    if (TestSuiteName == "") { Console.WriteLine("Set the name for the test suite"); return null; }
                    if (SuiteQuery == "") { Console.WriteLine("Set the query for the new a suite"); return null; }
                    break;
                case TestSuiteType.RequirementTestSuite:
                    if (RequirementId == 0) { Console.WriteLine("Set the requrement id for the test suite"); return null; }
                    break;
            }
            int parentsuiteId = ParentSuiteId;//GetParentSuiteId(TeamProjectName, TestPlanId, ParentPath);
            if (parentsuiteId > 0)
            {
                TestSuiteCreateParams newSuite = new TestSuiteCreateParams()
                {
                    Name = TestSuiteName,
                    SuiteType = SuiteType,
                    QueryString = SuiteQuery,
                    RequirementId = RequirementId,
                    ParentSuite = new TestSuiteReference() { Id = parentsuiteId }
                };
                Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite testSuite = TestPlanClient.CreateTestSuiteAsync(newSuite, TeamProjectName, TestPlanId).Result;
                Console.WriteLine("The Test Suite has been created: " + testSuite.Id + " - " + testSuite.Name);
                return testSuite;

            }
            else { Console.WriteLine("Can not find the parent test suite"); return null; }


        }
        /// <summary>
        /// Add test cases to an exisitng static test suite
        /// </summary>
        /// <param name="TeamProjectName"></param>
        /// <param name="TestPlanId"></param>
        /// <param name="StaticSuitePath"></param>
        /// <param name="TestCasesIds"></param>
        public void AddTestCasesToSuite(string TeamProjectName, int TestPlanId, int testSuiteId, List<int> TestCasesIds)
        {
            //int testSuiteId = GetSuiteId(TeamProjectName, TestPlanId, StaticSuitePath);

            if (testSuiteId == 0) { Console.WriteLine("Can not find the suite:"); return; }

            Microsoft.TeamFoundation.TestManagement.WebApi.TestSuite testSuite = TestManagementClient.GetTestSuiteByIdAsync(TeamProjectName, TestPlanId, testSuiteId).Result;

            // var Testcases = TestManagementClient.GetTestCaseByIdAsync(TeamProjectName, TestPlanId, testSuiteId);

            if (testSuite.SuiteType == TestSuiteType.StaticTestSuite.ToString() || testSuite.SuiteType == TestSuiteType.DynamicTestSuite.ToString())
            {
                List<SuiteTestCaseCreateUpdateParameters> suiteTestCaseCreateUpdate = new List<SuiteTestCaseCreateUpdateParameters>();

                foreach (int testCaseId in TestCasesIds)
                {
                    try
                    {
                        var Testcases = TestManagementClient.GetTestCaseByIdAsync(TeamProjectName, TestPlanId, testSuiteId, testCaseId).Result;

                    }
                    catch (Exception)
                    {

                        suiteTestCaseCreateUpdate.Add(new SuiteTestCaseCreateUpdateParameters()
                        {
                            workItem = new Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.WorkItem()
                            {
                                Id = testCaseId


                            },
                            PointAssignments = new List<Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.Configuration>()
                            {
                                new Configuration()
                                {
                                    ConfigurationId=35
                                }

                            }

                        });
                    }


                }

                TestPlanClient.AddTestCasesToSuiteAsync(suiteTestCaseCreateUpdate, TeamProjectName, TestPlanId, testSuiteId).Wait();
            }
            else
                Console.WriteLine("The Test Suite '" + TestPlanId + "' is not static or requirement");
        }

        public int SearchSuite(string ProjectName, int TestPlanId, string ReleaseName, int ReleaseSuiteId, string ModuleSuiteName)
        {
            int CurrentReleaseId = 0;
            int AutomationSuiteId = 0;
            int ModuleSuiteId = 0;
            var ReleaseSuite = TestManagementClient.GetTestSuiteByIdAsync(ProjectName, TestPlanId, ReleaseSuiteId, 1).Result;
            // ReleaseSuite.Children.First().Name;

            //  List<Microsoft.TeamFoundation.TestManagement.WebApi.TestSuite> TestSuites = TestManagementClient.GetTestSuitesForPlanAsync(ProjectName,TestPlanId,(int)SuiteExpand.Children,asTreeView:true).Result;

            var currentReleaseID = ReleaseSuite.Suites
                            .Where(x => x.Name == ReleaseName).ToList();



            if (currentReleaseID.Count() == 0)
                CurrentReleaseId = CreateTestSuite(ProjectName, TestPlanId, ReleaseSuite.Id, ReleaseName).Id;
            else
                CurrentReleaseId = Convert.ToInt32(currentReleaseID.FirstOrDefault().Id);


            var AutomationSuite = TestManagementClient.GetTestSuiteByIdAsync(ProjectName, TestPlanId, CurrentReleaseId, 1)
           .Result.Suites.Where(x => x.Name == "Automation Regression").ToList();


            if (AutomationSuite.Count() == 0)
                AutomationSuiteId = CreateTestSuite(ProjectName, TestPlanId, CurrentReleaseId, "Automation Regression").Id;
            else
                AutomationSuiteId = Convert.ToInt32(AutomationSuite.FirstOrDefault().Id);

            var ModuleSuite = (from ts in TestManagementClient.GetTestSuiteByIdAsync(ProjectName, TestPlanId, AutomationSuiteId, 1)
                                     .Result.Suites
                               where ts.Name == ModuleSuiteName
                               select ts).ToList();


            if (ModuleSuite.Count() == 0)
                ModuleSuiteId = CreateTestSuite(ProjectName, TestPlanId, AutomationSuiteId, ModuleSuiteName).Id;
            else
                ModuleSuiteId = Convert.ToInt32(ModuleSuite.FirstOrDefault().Id);

            return ModuleSuiteId;

        }



        #region create new connections
        public void InitClients(VssConnection Connection)
        {

        }

        public void ConnectWithDefaultCreds(string ServiceURL)
        {
            VssConnection connection = new VssConnection(new Uri(ServiceURL), new VssCredentials());
            InitClients(connection);
        }

        public void ConnectWithCustomCreds(string ServiceURL, string User, string Password)
        {
            VssConnection connection = new VssConnection(new Uri(ServiceURL), new WindowsCredential(new NetworkCredential(User, Password)));
            InitClients(connection);
        }

        public void ConnectWithPAT(string ServiceURL, string PAT)
        {
            VssConnection connection = new VssConnection(new Uri(ServiceURL), new VssBasicCredential(string.Empty, PAT));
            InitClients(connection);
        }
        #endregion


    }
}
