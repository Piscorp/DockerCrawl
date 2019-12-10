using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace SeleniumDocker
{


    class CustomTimer : System.Timers.Timer
    {
        public IWebDriver _driver;
        public Thread _currentThread;
        public DockerController _DC;
        public string _containerID;
        public string _name;

  
        public void OnTimedEvent(Object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine($"{_name} Raised: {e.SignalTime} quitting driver for thread {((CustomTimer)sender)._currentThread.ManagedThreadId} ");
                _driver.Quit();

                if (_containerID != null)
                {
                    Console.WriteLine($"{_name} Raised: {e.SignalTime} Killing Container");

                    Task d = _DC.DisposeContainerAsync(_containerID);
                    d.Wait();

                }

                _currentThread.Abort();
                
                Console.WriteLine($"{_name} Raised: {e.SignalTime} Abort thread {((CustomTimer)sender)._currentThread.ManagedThreadId}  done");
                Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{_name} {ex.Message}");
            }

        }

    }

    public class Crawler
    {
       
        private string ContainerID_1=null;

        
        static DockerController DC = new DockerController("tcp://192.168.10.235:2375");
        //static  DockerController DC = new DockerController("npipe://./pipe/docker_engine");
        private string SearchString;
        private string Name;
        private string DriverPort;
        private string VNCPort;
        private int TTL;
        private bool DoneTakeSS = false;


        public RemoteWebDriver driver = null;

        private static readonly Object locker = new Object();

        public Crawler(string name,string SearchStr)
        {
            Name = name;
            SearchString = SearchStr;
        }

        public void CreateContainer(string Name, string DriverPort, string VNCPort)
        {
            try
            {
                Task<string> t = DC.StartContainerNode("selenium/standalone-chrome-debug", Name, DriverPort, VNCPort);
                //Task<string> t = DC.StartContainerNode("standalone-chrome-debug105", Name, DriverPort, VNCPort);
                t.Wait();
                ContainerID_1 = t.Result;
            }
            catch (Exception ex)
            {
                return;
            }
           
        }
        public void SetupRemoteChrome(string name, string driverPort, string vNCPort,int tTL)
        {

            Name = name;
            DriverPort = driverPort;
            VNCPort = vNCPort;
            TTL = tTL;

            CreateContainer(Name, DriverPort, VNCPort);
            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = "D:\\chromedriver" + Name + ".log";
            service.EnableVerboseLogging = true;

            string HUB_URL = "http://192.168.10.235:" + DriverPort + "/wd/hub";
          
            //string HUB_URL = "http://localhost:" + DriverPort + "/wd/hub";
            //CHROME and IE  

            ChromeOptions Options = new ChromeOptions();
            //Options.AddArguments("--proxy-server=117.2.17.26:53281");
            Options.AddArgument("--whitlisted-ips");
            Options.AddArgument("--disable-cache");
            Options.AddArgument("--disable-infobars");
            Options.AddArgument("--disable-impl-side-painting");
            Options.AddArgument("--disable-notifications");
            Options.AddArgument("--disable-plugins-discovery");
            Options.AddArgument("--disable-popup-blocking");
            Options.AddArgument("--disable-translate");
            Options.AddArgument("--disable-web-security");
            Options.AddArgument("--disable-fill-on-account-select");
            Options.AddArgument("--enable-multiprocess");
            Options.AddArgument("--enable-smooth-scrolling");
            Options.AddArgument("--fast-start");
            Options.AddArgument("--ignore-certificate-errors");
            Options.AddArgument("--mute-audio");
            Options.AddArgument("--start-maximized");
            Options.AddArgument("--no-sandbox");
            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(5000);

                try
                {
                   
                    //chrome
                    
                    driver = new RemoteWebDriver(
                new Uri(HUB_URL), Options.ToCapabilities(), TimeSpan.FromSeconds(600));// NOTE: connection timeout of 600 seconds or more required for time to launch grid nodes if non are available.

                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(300);

                    Console.WriteLine($"{Name}:{VNCPort} RemoteDriver created: {DateTime.Now}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{Name}:{VNCPort} {ex.Message}");
                }

            }

        }

        public void  Run()
        {
            
            Thread thread = new Thread(() => RemoteSelenium(Name, DriverPort, VNCPort, TTL));

            thread.Start();
            Console.WriteLine($"{Name}:{VNCPort} thread started.");
        }


        public void RemoteSelenium(string Name,string DriverPort, string VNCPort, int TTL)
        {

            string[] SearchDict = new string[4] { "World Cup", "Mac Miller", "Donald Trump", "Kate Spade" };


            try
            {
             
                var aTimer = new CustomTimer
                    {
                        _name = Name,
                        _DC = DC,
                        _containerID = ContainerID_1,
                        _currentThread = Thread.CurrentThread,
                        _driver = driver,
                         Interval = TTL * 1000  //ms

                    };

                Random r = new Random();
                SearchString = SearchDict[r.Next(0, 4)];

                aTimer.Elapsed += aTimer.OnTimedEvent;
                    aTimer.Start();


                driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(5);

                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromMinutes(5);

                driver.Manage().Window.Maximize(); // WINDOWS, DO NOT WORK FOR LINUX/firefox. If Linux/firefox set window size, max 1920x1080, like driver.Manage().Window.Size = new Size(1920, 1080);
                                                   // driver.Manage().Window.Size = new Size(1920, 1080); // LINUX/firefox			 

                driver.Navigate().GoToUrl("https://www.bing.com/");

                var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(5));

                wait.Until(ExpectedConditions.ElementExists((By.XPath("//input[@id='sb_form_q']"))));
                            

                IJavaScriptExecutor ex = (IJavaScriptExecutor)driver;

                string str = driver.PageSource;

                IWebElement element = driver.FindElement(By.XPath("//input[@id='sb_form_q']"));

                element.Click();
                element.Clear();
                //Enter some text in search text box
                element.SendKeys(SearchString);

                wait.Until(ExpectedConditions.ElementExists((By.XPath("//input[@id='sb_form_go']"))));
                element = driver.FindElement(By.XPath("//input[@id='sb_form_go']"));
                element.Submit();
                
          
                wait.Until(ExpectedConditions.ElementExists((By.Id("b_content"))));

                if(!DoneTakeSS)
                {
                    try
                    {
                        Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                        ss.SaveAsFile(@"D:\data\SS\" + Name + ".jpg", ScreenshotImageFormat.Jpeg);
                        DoneTakeSS = true;

                    }
                    catch (Exception exSS) { }
                   
                }
                for(int i=0;i<50;i++)
                {
                    int triesNo = 0;

                    for (int j=0;j<4;j++)
                    { 
                        try
                        {
                            IWebElement NextPageElement = wait.Until(ExpectedConditions.ElementToBeClickable((By.XPath("//a[contains(@title,'Next page')]"))));
                            ex.ExecuteScript("arguments[0].click();", NextPageElement);
                            break;
                        }
                        catch (Exception ex1)
                        {
                            driver.Navigate().Refresh();
                            wait.Until(ExpectedConditions.ElementExists((By.Id("b_content"))));
                            triesNo++;
                        }
                    }

                    if (triesNo > 2)
                    {
                        break;
                    }

                    wait.Until(ExpectedConditions.ElementExists((By.Id("b_results"))));
                    Thread.Sleep(1000);

                    var webElements = driver.FindElements(By.TagName("h2"));
                    for(int j=0;j<webElements.Count;j++)
                    {
                        using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(@"D:\data\"+ Name+".log",true))
                        {
                            file.WriteLine($"{webElements[j].GetAttribute("innerText")}");
                        }
                    }
                 
                    Thread.Sleep(10000);
                }


                Console.WriteLine($"{Name}:{VNCPort} done capturing!");

                Thread.Sleep(3600000);

                driver.Quit();
                Task d = DC.DisposeContainerAsync(ContainerID_1);
                d.Wait();

            }
            catch (Exception ex)
            {

                if (ex is ThreadAbortException) { }
                else
                {
                    if (driver != null)
                        driver.Quit();
                }
                Task d = DC.DisposeContainerAsync(ContainerID_1);
                d.Wait();

                Console.WriteLine($"{Name}:{VNCPort} {ex.Message}");

            }


        }

        public void NormalSelenium()
        {

            IWebDriver driver = null;

            try
            {

                var service = ChromeDriverService.CreateDefaultService();
                service.LogPath = "D:\\chromedriver"+Name+".log";
                service.EnableVerboseLogging = true;

                // Create a driver instance for chromedriver
                driver = new ChromeDriver(service);

                var aTimer = new CustomTimer
                {
                    _name = Name,
                    _DC = DC,
                    _containerID = ContainerID_1,
                    _currentThread = Thread.CurrentThread, 
                    _driver = driver,
                    Interval = 20000

                };

                aTimer.Elapsed += aTimer.OnTimedEvent;
                aTimer.Start();

                //Navigate to google page
                driver.Navigate().GoToUrl("http:www.google.com");

                //Maximize the window
                driver.Manage().Window.Maximize();

                //Find the Search text box using xpath
                IWebElement element = driver.FindElement(By.XPath("//*[@title='Search']"));

                //Enter some text in search text box
                element.SendKeys(SearchString);

                var btn = driver.FindElement(By.XPath("//*/input[@name='btnK']"));
                btn.Click();


                //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(50);
                //IWebElement ele = driver.FindElement(By.Id("target"));

                //driver.FindElement(By.Id("locator"), 10);
                Thread.Sleep(60000);

                driver.Quit();



            }
            catch (Exception ex)
            {
                {
                    Console.WriteLine($"{Name} {ex.Message}");
                    if (ex is ThreadAbortException) { }
                    else
                    {
                        if(driver!=null)
                            driver.Quit();
                    }

                }

            }
        }

    }
}
