using Microsoft.Playwright;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpecFlow.Actions.Playwright
{
    /// <summary>
    /// Manages a browser instance using Playwright
    /// </summary>
    public class BrowserDriver : IDisposable
    {
        private readonly IPlaywrightConfiguration _playwrightConfiguration;
        private readonly IDriverInitialiser _driverInitialiser;
        protected readonly AsyncLazy<IBrowser> _currentBrowserLazy;
        protected bool _isDisposed;

        public BrowserDriver(IPlaywrightConfiguration playwrightConfiguration, IDriverInitialiser driverInitialiser)
        {
            _playwrightConfiguration = playwrightConfiguration;
            _driverInitialiser = driverInitialiser;
            if (_currentBrowserLazy == null)
                _currentBrowserLazy = new AsyncLazy<IBrowser>(CreatePlaywrightAsync);
        }

        /// <summary>
        /// The current Playwright instance
        /// </summary>
        public Task<IBrowser> Current => _currentBrowserLazy.Value;

        /// <summary>
        /// Creates a new instance of Playwright (opens a browser)
        /// </summary>
        /// <returns></returns>
        private async Task<IBrowser> CreatePlaywrightAsync()
        {
            var options = new List<object>() { _playwrightConfiguration.Arguments, _playwrightConfiguration.DefaultTimeout, _playwrightConfiguration.Headless, _playwrightConfiguration.SlowMo, _playwrightConfiguration.TraceDir };
            if (_playwrightConfiguration.runOnBrowserstack.Value)
            {
                options.AddRange(new object[] { _playwrightConfiguration.userKey, _playwrightConfiguration.accessToken, _playwrightConfiguration.local, _playwrightConfiguration.build });
            }
            return _playwrightConfiguration.Browser switch
            {
                Browser.Chrome => await _driverInitialiser.GetChromeDriverAsync(_playwrightConfiguration.Arguments, _playwrightConfiguration.DefaultTimeout, _playwrightConfiguration.Headless, _playwrightConfiguration.SlowMo, _playwrightConfiguration.TraceDir),
                Browser.Firefox => await _driverInitialiser.GetFirefoxDriverAsync(_playwrightConfiguration.Arguments, _playwrightConfiguration.DefaultTimeout, _playwrightConfiguration.Headless, _playwrightConfiguration.SlowMo, _playwrightConfiguration.TraceDir),
                Browser.Edge => await _driverInitialiser.GetEdgeDriverAsync(_playwrightConfiguration.Arguments, _playwrightConfiguration.DefaultTimeout, _playwrightConfiguration.Headless, _playwrightConfiguration.SlowMo, _playwrightConfiguration.TraceDir),
                Browser.Chromium => await _driverInitialiser.GetChromiumDriverAsync(_playwrightConfiguration.Arguments, _playwrightConfiguration.DefaultTimeout, _playwrightConfiguration.Headless, _playwrightConfiguration.SlowMo, _playwrightConfiguration.TraceDir),
                Browser.Webkit => await _driverInitialiser.GetWebKitDriverAsync(_playwrightConfiguration.Arguments, _playwrightConfiguration.DefaultTimeout, _playwrightConfiguration.Headless, _playwrightConfiguration.SlowMo, _playwrightConfiguration.TraceDir),
                _ => throw new NotImplementedException($"Support for browser {_playwrightConfiguration.Browser} is not implemented yet"),
            };

        }

        /// <summary>
        /// Disposes the Playwright instance (closing the browser)
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_currentBrowserLazy.IsValueCreated)
            {
                Task.Run(async delegate
                {
                    await (await Current).CloseAsync();
                    await (await Current).DisposeAsync();
                });
            }

            _isDisposed = true;
        }
    }
}
