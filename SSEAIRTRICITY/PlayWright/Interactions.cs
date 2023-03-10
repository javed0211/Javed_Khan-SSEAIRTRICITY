using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpecFlow.Actions.Playwright
{
    public class Interaction
    {
        private readonly Task<IPage> _page;

        public Interaction(Task<IPage> page)
        {
            _page = page;
        }

        #region locator
        public string username => "//input[@name='UsrFirstName']";

        #endregion

        /// <summary>
        /// Navigates to the specified URL
        /// </summary>
        /// <param name="url"></param>
        public async Task GoToUrl(string url)
        {
            await (await _page).GotoAsync(url);
        }

        /// <summary>
        /// Return title of current page
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetTitle()
        {
            return await (await _page).TitleAsync();
        }

        /// <summary>
        /// Gets the current URL
        /// </summary>
        /// <returns></returns>
        public async Task<string?> GetUrl()
        {
            return (await _page).Url;
        }

        public async Task<ILocator> GetElement(string locator)
        {
            return (await _page).Locator(locator).First;
        }

        /// <summary>
        /// Sends a string to the specified selector
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="keys"></param>
        /// <param name="pageFillOptions"></param>
        /// <returns></returns>
        public async Task SendTextAsync(string selector, string keys, PageFillOptions? pageFillOptions = null)
        {
            await (await _page).FillAsync(selector, "", pageFillOptions);
            await (await _page).FillAsync(selector, keys, pageFillOptions);
        }

        /// <summary>
        /// Returns list of elements
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<IElementHandle>> GetAllElements(string selector)
        {
            return await (await _page).QuerySelectorAllAsync(selector);
        }

        /// <summary>
        /// bring element in a view
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task ScrollToElement(string selector)
        {
            await (await _page).Locator(selector).ScrollIntoViewIfNeededAsync();
        }

        /// <summary>
        /// Retunrs inner html of element
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public async Task<string> GetInnerHTML(string locator)
        {
            return await (await _page).InnerHTMLAsync(locator);
        }

        /// <summary>
        /// Checks if element exists in set of elements
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public async Task<bool> HasElement(string locator)
        {
            if (GetAllElements(locator).Result.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Wait for page to load 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pageWaitForLoadStateOptions"></param>
        /// <returns></returns>
        public async Task WaitForLoadState(LoadState state, PageWaitForLoadStateOptions? pageWaitForLoadStateOptions = null)
        {
            await (await _page).WaitForLoadStateAsync(state, pageWaitForLoadStateOptions);
        }


        /// <summary>
        /// wait for element to appear in DOM and return the element
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="pageWaitForSelectorOptions"></param>
        /// <returns></returns>
        public async Task<IElementHandle> WaitForElement(string selector, PageWaitForSelectorOptions? pageWaitForSelectorOptions = null)
        {
            return await (await _page).WaitForSelectorAsync(selector, pageWaitForSelectorOptions);
        }

        /// <summary>
        /// verify is element is checked or not
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public async Task<bool> IsChecked(string element)
        {
            return await (await _page).Locator(element).IsCheckedAsync();
        }


        /// <summary>
        /// Sends individual keystrokes to the specified selector
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="keys"></param>
        /// <param name="pageTypeOptions"></param>
        /// <returns></returns>
        public async Task SendKeystrokesAsync(string selector, string keys, PageTypeOptions? pageTypeOptions = null)
        {
            await (await _page).TypeAsync(selector, keys, pageTypeOptions);
        }

        /// <summary>
        /// Sends a click to an element
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="pageClickOptions"></param>
        /// <returns></returns>
        public async Task ClickAsync(string selector, PageClickOptions? pageClickOptions = null)
        {
            await (await _page).ClickAsync(selector, pageClickOptions);
        }

        /// <summary>
        /// Gets the value attribute of an element
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="pageInputValueOptions"></param>
        /// <returns></returns>
        public async Task<string?> GetValueAttributeAsync(string selector, PageInputValueOptions? pageInputValueOptions = null)
        {
            return await (await _page).InputValueAsync(selector, pageInputValueOptions);
        }


        /// <summary>
        /// Returns element's text 
        /// </summary>
        /// <param name="locator"></param>
        /// <returns></returns>
        public async Task<string> GetInnerElementText(string locator)
        {
            return await (await _page).Locator(locator).InnerTextAsync();
        }


        /// <summary>
        /// Waits for the value attribute of an element to not be empty
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task WaitForNonEmptyValue(string selector)
        {
            await (await _page).WaitForFunctionAsync($"document.querySelector(\"{selector}\").value !== \"\"");
        }

        /// <summary>
        /// Verify is element is visible or not
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public async Task<bool> IsVisible(string element)
        {
            return await (await _page).Locator(element).IsVisibleAsync();
        }

        /// <summary>
        /// Waits for the value attribute of an element to be empty
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public async Task WaitForEmptyValue(string selector)
        {
            await (await _page).WaitForFunctionAsync($"document.querySelector(\"{selector}\").value === \"\"");
        }

        /// <summary>
        /// Selects the option from a select element by its value
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="value"></param>
        /// <param name="pageSelectOptionOptions"></param>
        /// <returns></returns>
        public async Task SelectDropdownOptionAsync(string selector, string value, PageSelectOptionOptions? pageSelectOptionOptions = null)
        {
            await (await _page).SelectOptionAsync(selector, new SelectOptionValue { Value = value }, pageSelectOptionOptions);
        }

        /// <summary>
        /// Select value from dropdown list
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SelectValueFromDropdownAsync(string selector, string value)
        {
            var select = (await _page).GetByRole(AriaRole.Combobox, new() { Name = "Add an appliance" });
            await select.SelectOptionAsync(value);
        }

        /// <summary>
        /// Selects the option from a select element by its index
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="index"></param>
        /// <param name="pageSelectOptionOptions"></param>
        /// <returns></returns>
        public async Task SelectDropdownOptionAsync(string selector, int index, PageSelectOptionOptions? pageSelectOptionOptions = null)
        {
            await (await _page).SelectOptionAsync(selector, new SelectOptionValue { Index = index }, pageSelectOptionOptions);
        }

        /// <summary>
        /// Wait for page instance
        /// </summary>
        /// <returns></returns>
        public async Task WaitForNewPage()
        {
            await (await _page).Context.WaitForPageAsync();
        }

        /// <summary>
        /// Wait for defined timeout
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task WaitForTimeOut(float timeout)
        {
            await (await _page).WaitForTimeoutAsync(timeout);
        }

        /// <summary>
        /// Selects frame by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IFrame> SelectFrameByName(string name)
        {
            return (await _page).Frame(name);
        }

        /// <summary>
        /// Selects frame by its URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<IFrame> SelectFrameByURL(string url)
        {
            return (await _page).FrameByUrl(url);
        }

        /// <summary>
        /// Return details of all pages for current browser context
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<IPage>> GetNewPage()
        {
            return (await _page).Context.Pages;
        }
    }
}
