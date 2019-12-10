using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;


namespace SeleniumDocker
{

    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                try
                {
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(by));
                }
                catch (Exception ex)
                {

                }
                
            }
            return driver.FindElement(by);
        }
    }

    
    class Program
    {
        
        static void Main(string[] args)
        {
            /*
            for (int i = 0; i < 30; i++)
            {
                string nodeName = "Node-" + i.ToString();
                Crawlers c = new Crawlers(nodeName, "Big Brown Fox");

                Thread thread = new Thread(() => c.NormalSelenium());
                thread.Start();

                //Thread.Sleep(1000);
            }
            */
            Console.WriteLine($"Testing...");

            List<Crawler> CrawlerList = new List<Crawler>();

            for (int i=0;i<30; i++)
            {
                string nodeName = "Node-" + i.ToString();
                string driverPort = (i+32880).ToString();
                string VNCPort = (i+42880).ToString();

                Crawler c = new Crawler(nodeName, "Big Brown Fox");

                c.SetupRemoteChrome(nodeName, driverPort, VNCPort,3600);

                CrawlerList.Add(c);
                
            }
            foreach (Crawler c in CrawlerList)
            {
                c.Run();

                Thread.Sleep(3000);
            }
               

        }
    }
}
