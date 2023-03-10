using CsvHelper;
using Newtonsoft.Json;
using NUnit.Framework;
using SpecFlow.Actions.Playwright;
using SSE.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using static SSEAIRTRICITY.Utilities.SSE;

namespace SSE.Steps
{
    [Binding]
    public sealed class EnergyCostSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly EnergyCostPage _energyCostPage;
        private readonly IPlaywrightConfiguration _playwrightConfiguration;
        public EnergyCostSteps(EnergyCostPage energyCostPage, ScenarioContext scenarioContext, IPlaywrightConfiguration playwrightConfiguration)
        {
            _energyCostPage = energyCostPage;
            _scenarioContext = scenarioContext;
            _playwrightConfiguration = playwrightConfiguration;
        }


        [Given(@"The Home appliance cost calculator is running")]
        public async Task TheHomeappliancecostcalculatorisrunning()
        {
            await _energyCostPage.Goto(_playwrightConfiguration.url);
            Assert.AreEqual("Compare how much electrical appliances cost to use - citizens advice".ToLower(), (await _energyCostPage.GetPageTitle()).ToLower());
        }


        [Given(@"I am a resident from '(.*)'")]
        public async Task GivenIAmAResidentFrom(string country)
        {
            await _energyCostPage.SelectCountry(country);
            _scenarioContext["country"] = country;
        }

        [When(@"I add the '(.*)' appliances and its average usage and the national average rate '(.*)'")]
        public async Task WhenIAddTheAppliancesAndItsAverageUsageAndTheNationalAverageRate(int Noappliances, int avgRate)
        {
            var lstAppliances = _playwrightConfiguration.appliances;
            await _energyCostPage.AddAppliancesAndRate(lstAppliances.Take(Noappliances).ToList(), avgRate);
        }

        [Then(@"I should get the results table with daily, weekly, monthly, and yearly cost")]
        public void ThenIShouldGetTheResultsTableWithDailyWeeklyMonthlyAndYearlyCost()
        {
            var costs = (List<Cost>)_scenarioContext["cost"];
            bool allCostsExist = costs.All(c => c.daily != null && c.weekly != null && c.monthly != null && c.yearly != null);
            var csvData = costs.Select(c => new { c.appliance, c.daily, c.weekly, c.monthly, c.yearly });
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture);
            csv.WriteRecords(csvData);
            writer.Flush();

            var filePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\results", (string)_scenarioContext["country"] + "costs.csv");
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            memoryStream.WriteTo(fileStream);
            _scenarioContext["filePath"] = filePath;
            if (!allCostsExist)
                throw new Exception("Prices are missing");

        }

        [Then(@"I should get the results message as '([^']*)'")]
        public async void ThenIShouldGetTheResultsMessageAs(string error)
        {
            var errMsg = await _energyCostPage.GetErrorMessage();
            Assert.IsTrue(errMsg.Contains(error));

        }

    }
}
