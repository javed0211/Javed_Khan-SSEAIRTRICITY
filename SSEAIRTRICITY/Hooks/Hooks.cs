using Allure.Commons;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using SpecFlow.Actions.Playwright;

using SSE.Pages;

namespace PlaywrightSharpDemo.Hooks
{
    [Binding]
    public sealed class Hooks : BasePage
    {
        private readonly string _traceName;
        private static ScenarioContext _scenarioContext;
        private static FeatureContext _featureContext;

        public static List<string> StepInfo = new();
        public bool tcFound = true;
        private static IPlaywrightConfiguration _playwrightConfiguration;

        public Hooks(ScenarioContext scenarioContext, BrowserDriver browserDriver, FeatureContext featureContext, IPlaywrightConfiguration playwrightConfiguration) : base(browserDriver)
        {
            _traceName = scenarioContext.ScenarioInfo.Title.Replace(" ", "_");
            _scenarioContext = scenarioContext;
            _featureContext = featureContext;
            _playwrightConfiguration = playwrightConfiguration;
        }



        [BeforeScenario]
        public async Task StartTracingAsync()
        {
            var tracing = await Tracing;
            await tracing.StartAsync(new TracingStartOptions
            {
                Name = _traceName,
                Screenshots = true,
                Snapshots = true
            });
        }


        [AfterScenario(Order = 2)]
        public async Task StopTracingAsync()
        {
            var tracing = await Tracing;
            await tracing.StopAsync(new TracingStopOptions()
            {
                Path = $"traces/{_traceName}.zip"
            });
            _page.Dispose();
        }

        public async Task StepScreenShot(string path)
        {

            var fileName = DateTime.Now.ToString("ddMMyyyyhhssmm");
            var type = new PageScreenshotOptions();
            await _page.Result.
                    ScreenshotAsync(
                        new PageScreenshotOptions
                        {
                            Path = path,
                            Type = ScreenshotType.Png
                        });
        }

        [AfterScenario(Order = 1)]
        public void TakeScreenShotIffailed()
        {
            if (_scenarioContext.TestError != null)
            {
                try
                {
                    var filename = _scenarioContext.ScenarioInfo.Title + DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss") + ".png";
                    var filepath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Screenshots\\" + filename;

                    if (!Directory.Exists(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Screenshot\\"))
                        Directory.CreateDirectory(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Screenshots\\");

                    AllureLifecycle.Instance.AddAttachment(filename, "image/png", filepath);
                }
                catch (Exception e)
                {
                }
            }
            var StoryTags = _scenarioContext.ScenarioInfo.Tags.Where(x => x.Contains("Story:"));
            var TestCaseTags = _scenarioContext.ScenarioInfo.Tags.Where(x => x.Contains("TC:"));
            _scenarioContext.TryGetValue(out Allure.Commons.TestResult testResult);
            foreach (var tag in StoryTags)
            {
                var name = tag.Split(new string[] { "StoryID_" }, StringSplitOptions.None).LastOrDefault();
                AllureLifecycle.Instance.UpdateTestCase(testResult.uuid, tc =>
                {
                    tc.links.Add(new Link()
                    {
                        name = "Story-" + name,
                        url = $"https://dev.azure.com/_workitems/edit/{name}",
                        type = "Story"
                    });
                });
            }
            foreach (var tag in TestCaseTags)
            {
                var name = tag.Split(new string[] { "TC_" }, StringSplitOptions.None).LastOrDefault();
                AllureLifecycle.Instance.UpdateTestCase(testResult.uuid, tc =>
                {
                    tc.links.Add(new Link()
                    {
                        name = "TC-" + name,
                        url = $"https://dev.azure.com/_workitems/edit/{name}",
                        type = "TestCase"
                    });
                });
            }
            if (_scenarioContext.Keys.Contains("filePath"))
                AllureLifecycle.Instance.AddAttachment("Costs", "text/csv", (string)_scenarioContext["filePath"]);
        }
    }
}
