using Microsoft.Playwright;
using SpecFlow.Actions.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SSE.Pages
{
    public class BasePage
    {
        private static Task<IBrowserContext> _browserContext;
        private readonly Task<ITracing> _tracing;
        public static Task<IPage> _page;
        ScenarioContext _ScenarioContext;


        public Task<ITracing> Tracing => _tracing;

        public BasePage(BrowserDriver browserDriver)
        {
            if (_browserContext == null)
            {
                _browserContext = CreateBrowserContextAsync(browserDriver.Current);
                
            }
                
            _tracing = _browserContext.ContinueWith(t => t.Result.Tracing);
            if (_page == null)
            {
                _page = CreatePageAsync(_browserContext);
            }
                
        }

        public async Task<IBrowserContext> CreateBrowserContextAsync(Task<IBrowser> browser)
        {
            return await (await browser).NewContextAsync(new()
            {
                RecordVideoDir= Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "Videos/"),
                RecordVideoSize = new RecordVideoSize() { Height = 1080, Width = 1920 },
                ScreenSize = new ScreenSize() { Height = 1080, Width = 1920 },
                ViewportSize = ViewportSize.NoViewport
            }
            ).ConfigureAwait(false);
        }

        public async Task<IPage> CreatePageAsync(Task<IBrowserContext> browserContext)
        {
            return await (await browserContext).NewPageAsync().ConfigureAwait(false);

        }
    }
}
