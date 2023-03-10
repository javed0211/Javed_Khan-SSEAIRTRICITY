
using SpecFlow.Actions.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using static SSEAIRTRICITY.Utilities.SSE;

namespace SSE.Pages
{
    [Binding]
    public sealed class EnergyCostPage : BasePage
    {
        private Interaction _interactions;
        readonly ScenarioContext _scenarioContext;

        public EnergyCostPage(BrowserDriver browserDriver, ScenarioContext ScenarioContext) : base(browserDriver)
        {
            _interactions = new Interaction(_page);
            _scenarioContext = ScenarioContext;
        }

        #region Locators
        public string btnCountry => "//div[@class = 'title__subheading']//a[contains(@class,'btn-secondary')]";
        public string lnkCountry => "//li//a[contains(@class,'btn-small')][contains(text(),'[NAME]')]";
        public string lstAppliance => "select#appliance";
        public string txtHours => "//input[@id='hours']";
        public string txtMins => "//input[@id='mins']";
        public string lstFrequency => "//select[@id='frequency']";
        public string txtkWh => "//input[@id='kwhcost']";
        public string btnAddAppliances => "//input[@id='submit']";
        public string lblAppliance => "//div[@id='appliance_running']//table//th[contains(@class,'appname')][contains(text(),'[NAME]')]";
        public string lblError => "//div[@class='cads-prose']/p";
        #endregion



        /// <summary>
        /// Navigate to given URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task Goto(string url)
        {
            await _interactions.GoToUrl(url);
        }

        /// <summary>
        /// Returns Page title
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetPageTitle()
        {
            return await _interactions.GetTitle();
        }

        public async Task SelectCountry(string country)
        {
            var eleCountry = await _interactions.WaitForElement(btnCountry);
            await eleCountry.ClickAsync();
            var xpathCountry = lnkCountry.Replace("[NAME]", country);
            await _interactions.ClickAsync(xpathCountry);
        }

        public async Task AddAppliancesAndRate(List<Time> appliances, int avgRate)
        {
            var costs = new List<Cost>();
            foreach (var appliance in appliances)
            {
                await _interactions.SelectValueFromDropdownAsync(lstAppliance, appliance.name);
                int totalTime = int.Parse(appliance.time);
                var time = TimeSpan.FromMinutes(totalTime);
                await _interactions.SendTextAsync(txtHours, time.Hours.ToString());
                await _interactions.SendTextAsync(txtMins, time.Minutes.ToString());
                if (await _interactions.IsVisible(txtkWh))
                    await _interactions.SendTextAsync(txtkWh, avgRate.ToString());
                await _interactions.ClickAsync(btnAddAppliances);
                costs.Add(await GetCosts(appliance.name));
            }
            _scenarioContext["cost"] = costs;
        }

        public async Task<Cost> GetCosts(string Appliance)
        {
            var appliance = _interactions.GetElement(lblAppliance.Replace("[NAME]", Appliance));
            var daily = await appliance.Result.Locator("//following-sibling::td[@headers='daily']").InnerTextAsync();
            var weekly = await appliance.Result.Locator("//following-sibling::td[@headers='weekly']").InnerTextAsync();
            var monthly = await appliance.Result.Locator("//following-sibling::td[@headers='monthly']").InnerTextAsync();
            var yearly = await appliance.Result.Locator("//following-sibling::td[@headers='yearly']").InnerTextAsync();

            return new Cost { appliance = Appliance, daily = daily, weekly = weekly, monthly = monthly, yearly = yearly };
        }

        public async Task<string> GetErrorMessage()
        {
            return await _interactions.GetElement(lblError).Result.InnerTextAsync();
                
        }
    }
}
