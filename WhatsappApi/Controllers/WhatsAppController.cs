using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace WhatsappApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WhatsAppController : ControllerBase
    {

        private static ChromeDriver driver;

        static WhatsAppController()
        {
            // Initialize ChromeDriver once with cached session
            var options = new ChromeOptions();
            options.AddArgument("--user-data-dir=C:\\Temp\\chrome_user_data"); // cached login
            options.AddArgument("--profile-directory=Default");
            options.AddArgument("--disable-logging");

            driver = new ChromeDriver(options);
            driver.Navigate().GoToUrl("https://web.whatsapp.com");
            Console.WriteLine("Please login to WhatsApp Web if not already logged in, then press ENTER in console...");
            Console.ReadLine(); // only first time
        }

        [HttpPost("send-single")]
        public IActionResult SendSingleMessage([FromBody] WhatsAppRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Number) || string.IsNullOrWhiteSpace(request.Message))
                    return BadRequest("Number or Message is missing.");

                string url = $"https://web.whatsapp.com/send?phone={request.Number}&text=&app_absent=0";
                driver.Navigate().GoToUrl(url);

                // Wait until chat input is loaded
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                IWebElement messageBox = wait.Until(d =>
                {
                    var elems = d.FindElements(By.CssSelector("div[contenteditable='true'][role='textbox']"));
                    return elems.Count > 0 ? elems[0] : null;
                });

                // Send message using JS
                string script = @"
            let input = document.querySelector('div[contenteditable=""true""][role=""textbox""]');
            if(input){
                input.focus();
                input.innerHTML = decodeURIComponent('" + Uri.EscapeDataString(request.Message) + @"');
                input.dispatchEvent(new InputEvent('input', {bubbles: true}));
                let sendBtn = document.querySelector('span[data-icon=""send""]')?.closest('button');
                if(sendBtn){ sendBtn.click(); return true; } else { return false; }
            } else { return false; }
        ";
                bool sent = (bool)((IJavaScriptExecutor)driver).ExecuteScript(script);

                if (sent)
                    return Ok($"Message sent successfully to {request.Number}");
                else
                    return BadRequest("Failed to find send button or input box.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error sending message: {ex.Message}");
            }
        }


    }

    public class WhatsAppRequest
    {
        public string Number { get; set; }
        public string Message { get; set; }
    }
}

