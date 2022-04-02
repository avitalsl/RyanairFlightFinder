using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RyanairFlightFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            FlightFinder flightFinder = new FlightFinder();
            flightFinder.StartFlightFinder();
        }

        class FlightFinder
        {
            IWebDriver driver;
            WebDriverWait wait;
            Actions action;
            List<Trip> trips;
            string[] validAirports = "Aalborg, Aarhus, Aberdeen, Agadir, Alghero, Alicante, Almeria, Amman Jordan, Amsterdam, Ancona, Aqaba Jordan, Athens, Banja Luka, Barcelona, Barcelona Girona, Barcelona Reus, Bari, Basel, Bergerac, Berlin Brandenburg, Beziers, Biarritz, Billund, Birmingham, Bodrum, Bologna, Bordeaux, Bournemouth, Bratislava, Bremen, Brest, Brindisi, Bristol, Brive, Brno, Brussels, Brussels Charleroi, Bucharest, Budapest, Burgas, Bydgoszcz, Cagliari, Carcassonne, Cardiff, Castellon (Valencia), Catania, Chania, Clermont, Cluj, Cologne, Comiso, Copenhagen, Corfu, Cork, Crotone, Cuneo, Dalaman, Derry, Dole, Dortmund, Dresden, Dublin, Dubrovnik, Dusseldorf Weeze, East Midlands, Edinburgh, Eindhoven, Essaouira, Exeter, Faro, Fez, Figari, Frankfurt Hahn, Frankfurt International, Fuerteventura, Gdansk, Genoa, Glasgow, Glasgow Prestwick, G?teborg Landvetter, Gran Canaria, Grenoble, Hamburg, Haugesund, Helsinki, Heraklion Crete, Ibiza, Jerez, Kalamata, Karlsruhe / Baden-Baden, Katowice, Kaunas, Kefalonia, Kerry, Knock, Kos, Kosice, Krakow, Kyiv, La Palma, La Rochelle, Lamezia, Lanzarote, Lappeenranta, Larnaca, Leeds Bradford, Lille, Limoges, Lisbon, Liverpool, Lodz, London Gatwick, London Luton, London Stansted, Lourdes, Lublin, Lulea, Luxembourg, Maastricht, Madeira Funchal, Madrid, Malaga, Malmo, Malta, Manchester, Marrakesh, Marseille, Memmingen, Menorca, Milan Bergamo, Milan Malpensa, M?nster, Murcia International, Mykonos, Nador, Nantes, Naples, Newcastle, Newquay Cornwall, Nice, Nimes, Nis, Nuremberg, Olsztyn - Mazury, Oradea, Orebro, Oslo, Oslo Torp, Ostrava, Ouarzazate, Oujda, Palanga, Palermo, Palma de Mallorca, Paphos, Pardubice, Paris Beauvais, Paris Vatry, Parma, Perpignan, Perugia, Pescara, Pisa, Plovdiv, Podgorica, Poitiers, Ponta Delgada, Porto, Poznan, Prague, Preveza - Aktion, Pula, Rabat, Rhodes, Riga, Rijeka, Rimini, Rodez, Rome Ciampino, Rome Fiumicino, Rzeszow, Salzburg, Santander, Santiago, Santorini, Seville, Shannon, Sibiu, Skelleftea, Skiathos, Sofia, Split, Stockholm Arlanda, Stockholm V?ster?s, Suceava, Szczecin, Tallinn, Tampere, Tangier, Teesside, Tel Aviv, Tenerife North, Tenerife South, Terceira Lajes, T?touan, Thessaloniki, Timisoara, Toulouse, Tours Loire Valley, Trapani, Trieste, Turin, Valencia, Valladolid, Varna, V?xj? Sm?land, Venice M.Polo, Venice Treviso, Verona, Vienna, Vigo, Vilnius, Visby Gotland, Vitoria (Basque Country), Warsaw Modlin, Wroclaw, Zadar, Zagreb, Zakynthos, Zaragoza".ToLower().Split(',');

            public FlightFinder()
            {
                new WebDriverManager.DriverManager().SetUpDriver(new WebDriverManager.DriverConfigs.Impl.ChromeConfig());
                driver = new ChromeDriver(/*options*/);
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(50));
                action = new Actions(driver);
                trips = new List<Trip>();
            }


            public void StartFlightFinder()
            {
                GoToUrl("https://www.ryanair.com");
                ClosePopUps();
                Console.WriteLine("\nHello, welcome to Flight Finder.\nWhere do you want to depart from?");
                string departureFrom = GetValidAirportName();
                Console.WriteLine("Where do you want to travel to?");
                string destination = GetValidAirportName();
                Console.WriteLine("Enter earliest departure date in the following format: dd/mm/yyyy and press enter.");
                DateTime earliestDate = GetValidDate(Console.ReadLine());
                Console.WriteLine("Enter latest departure date in the following format: dd/mm/yyyy and press enter.");
                DateTime latestDate = GetValidDate(Console.ReadLine());
                Console.WriteLine("For how many days do you want to travel?\nEnter minimum days to travel and press enter.");
                int minDaysToTravel = GetNumOfDays(Console.ReadLine());
                Console.WriteLine("Enter maximum days to travel and press enter.");
                int maxDaysToTravel = GetNumOfDays(Console.ReadLine());
                Console.WriteLine("Please wait a few minutes while we search for convenient flights fo you.\n\n ");
                FindFlights(departureFrom, destination, earliestDate, latestDate, minDaysToTravel, maxDaysToTravel);
            }

            public void FindFlights(string originPort, string destination, DateTime earliestDate, DateTime latestDate,
                                                   int minDaysToTravel, int maxDaysToTravel)
            {
                foreach (DateTime departureDate in EachDay(earliestDate, latestDate))
                    foreach (DateTime returnDate in EachDay(earliestDate, latestDate))
                    {
                        GoToUrl("https://www.ryanair.com");
                        driver.Manage().Window.Maximize();                       
                        ChooseAirport(originPort, true);
                        ChooseAirport(destination, false);
                        if (!ChooseDate(departureDate)) { continue; }
                        if (!ChooseDate(returnDate)) { continue; }
                        Search();
                        FindPossibleTrips(minDaysToTravel, maxDaysToTravel, destination);
                    }
                SortByPrice(trips);
                ReturnInformation();
            }
            public void GoToUrl(string url)
            {
                driver.Url = url;
            }

            public void ClosePopUps()
            {
                bool isPresent = driver.FindElements(By.ClassName("cookie-popup-with-overlay__button")).Count() > 0;
                if (isPresent)
                    driver.FindElement(By.ClassName("cookie-popup-with-overlay__button")).Click();
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//*[@iconid='glyphs/message']")));
                driver.FindElement(By.XPath("//*[@iconid='glyphs/message']")).Click();
            }

            public void ChooseAirport(string airport, bool isDeparture)
            {
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                airport = myTI.ToTitleCase(airport);
                string departureOrDestination = isDeparture ? "departure" : "destination";
                IWebElement airportElement = driver.FindElement(By.Id($"input-button__{departureOrDestination}"));
                airportElement.Click();
                airportElement.SendKeys(Keys.Control + "a" + Keys.Delete);
                airportElement.SendKeys(airport);
                try
                {
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath($"//span[@data-ref='airport-item__name' and contains(text(), '{airport}')]")));
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath($"//span[@data-ref='airport-item__name' and contains(text(), '{airport}')]")));
                    driver.FindElement(By.XPath($"//span[@data-ref='airport-item__name' and contains(text(), '{airport}')]")).Click();
                }
                catch
                {
                    Console.WriteLine("Sorry, there are no flights to the destination you requested.");
                    StartFlightFinder();
                    Environment.Exit(1);
                }
            }

            public bool ChooseDate(DateTime date)
            {
                //driver.FindElement(By.XPath("//div[text(), ' Choose date ']")).Click();
                ClickOnMonth(date);
                if (!ClickOnDay(date)) { return false; }
                return false;
            }

            public void ClickOnMonth(DateTime date)
            {
                OpenDatePicker();
                bool found = false;
                while (!found)
                {
                    string visibleMonth1 = driver.FindElements(By.ClassName("calendar__month-name"))[0].Text;
                    string visibleMonth2 = driver.FindElements(By.ClassName("calendar__month-name"))[1].Text;
                    string month = date.ToString("MMMM yyyy", CultureInfo.InvariantCulture);
                    if (visibleMonth1 == month)
                        found = true;
                    else if (visibleMonth2 == month)
                        found = true;
                    else
                    {
                        IWebElement next = driver.FindElements(By.XPath("//icon[@iconid='glyphs/chevron-right']"))[1];
                        next = next.FindElement(By.XPath("../.."));
                        next.Click();
                    }
                }
            }

            public void Search()
            {
                driver.FindElement(By.XPath("//button[@aria-label='Search']")).Click();
            }

            public void FindPossibleTrips(int minDaysToTravel, int maxDaysToTravel, string destination)
            {
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//flight-list")));
                IList<SingleFlight> departureFlights = new List<SingleFlight>();
                IList<SingleFlight> returnFlights = new List<SingleFlight>();
                foreach (IWebElement flight in driver.FindElements(By.XPath("//flight-card")))
                {
                    if (HasClass(flight, "card--disabled")) { return; }
                    SingleFlight flightElement = new SingleFlight(flight, driver, destination);
                    if (flightElement.isReturn) { returnFlights.Add(flightElement); }
                    else { departureFlights.Add(flightElement); }
                }
                foreach (SingleFlight departureFlight in departureFlights)
                    foreach (SingleFlight returnFlight in returnFlights)
                    {
                        TimeSpan span = TimeSpan.Parse(returnFlight.date.Subtract(departureFlight.date).Days.ToString());
                        string spanS = span.ToString("dd");
                        if (int.Parse(spanS) >= minDaysToTravel && int.Parse(spanS) <= maxDaysToTravel)
                            trips.Add(PlanTrip(departureFlight, returnFlight));
                    }
            }

            public void SortByPrice(List<Trip> trips)
            {
                trips.Sort((x, y) => x.totalPrice.CompareTo(y.totalPrice));
            }

            public void ReturnInformation()
            {
                Console.Clear();
                Console.WriteLine("-------------------------");
                Console.WriteLine("List of possible flights:\n");
                if (trips.Count().Equals(0))
                {
                    Console.WriteLine("Sorry, we couldn't find flights compatible with your demands.");
                    return;
                }
                trips = trips.Distinct().ToList();
                foreach (Trip trip in trips)
                    Console.WriteLine(trip + "\n");
            }

            public bool ClickOnDay(DateTime date)
            {
                string formatedDate = $"{date.ToString("yyyy")}-{date.ToString("MM")}-{date.ToString("dd")}";
                IWebElement dayToCliclick = driver.FindElement(By.CssSelector($"div[data-id='{formatedDate}']"));
                if (!HasClass(dayToCliclick, "calendar-body__cell--disabled"))
                    dayToCliclick.Click();
                else { return false; }
                return true;
            }

            public bool HasClass(IWebElement element, string className)
            {
                string classes = element.GetAttribute("class");
                foreach (string c in classes.Split())
                    if (c.Equals(className))
                        return true;
                return false;
            }

            public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
            {
                for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                    yield return day;
            }

            public string GetValidAirportName()
            {
                while (true)
                {
                    Console.WriteLine("Enter airport name and press enter.");
                    string airportName = " " + Console.ReadLine().ToLower().Trim();
                    if (airportName == " aalborg") { airportName = "aalborg"; }
                    foreach (string element in validAirports)
                        if (element == airportName.ToLower())
                        { return airportName.Trim(); }
                    Console.WriteLine("Airport name does not exist in our data.");
                }
            }

            public DateTime GetValidDate(string date)
            {
                while (true)
                {
                    try
                    {
                        return DateTime.Parse(date);
                    }
                    catch
                    {
                        Console.WriteLine("Date is invalid.\nTry again, the correct format is dd/mm/yyyy.");
                        date = Console.ReadLine();
                    }
                }
            }

            public int GetNumOfDays(string numOfDays)
            {
                while (true)
                {
                    try
                    {
                        return int.Parse(numOfDays);
                    }
                    catch
                    {
                        Console.WriteLine("The number of days must be an integer.");
                        numOfDays = Console.ReadLine();
                    }
                }
            }

            public Trip PlanTrip(SingleFlight departureFlight, SingleFlight returnFlight)
            {
                return new Trip(departureFlight, returnFlight);
            }

            public void OpenDatePicker()
            {
                try
                {
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("calendar__month-name")));
                }
                catch
                {
                    driver.FindElement(By.XPath("//div[contains(text(), ' Choose date ')]")).Click();
                }
            }
        }

        class SingleFlight
        {
            public bool isReturn;
            public string from;
            public string destination;
            public DateTime date;
            public string hour;
            public double price;
            public string flightNo;

            public SingleFlight(IWebElement flight, IWebDriver driver, string destination)
            {
                from = flight.FindElement(By.CssSelector("span[class='time__city b2']")).Text;
                this.destination = flight.FindElement(By.CssSelector("span[data-ref^='destination-airport']")).Text;
                isReturn = this.destination.ToLower().Equals(destination.ToLower()) ? false : true;
                flightNo = flight.FindElement(By.CssSelector("div.card-flight-num__content")).Text;
                price = double.Parse(flight.FindElement(By.CssSelector("span.price-value")).Text.Substring(1));
                InitializeDate(driver);
                hour = flight.FindElement(By.XPath($"//flight-card[@data-ref='{flightNo}']//span[@class='h2']")).Text;
            }

            public override string ToString()
            {
                return $"From {from} to {destination}, {hour} price: ${price}";
            }

            public string GetMonthsRightName(string shortMonth)
            {
                IDictionary<string, string> monthNames = new Dictionary<string, string>()
                {
                    {"Jan", "January"}, {"Feb", "February"}, {"Mar", "March"}, {"Apr", "April"},
                    {"May", "May"}, {"Jun", "June"}, {"Jul", "July"}, {"Aug", "August"},
                    {"Sep", "September"}, {"Oct", "October"}, {"Nov", "November"}, {"Dec", "December"}
                };
                foreach (KeyValuePair<string, string> entry in monthNames)
                    if (entry.Key.Equals(shortMonth))
                        return entry.Value;
                return "";
            }

            public void InitializeDate(IWebDriver driver)
            {
                string day = driver.FindElements(By.XPath("//button[@data-selected='true']//div//div//span"))[isReturn ? 2 : 0].Text;
                string month = GetMonthsRightName(driver.FindElements(By.XPath("//button[@data-selected='true']//div//div//span"))[isReturn ? 3 : 1].Text);
                date = DateTime.Parse($"{day} {month} 2022");
            }
        }

        class Trip
        {
            public SingleFlight departureFlight;
            public SingleFlight returnFlight;
            public double totalPrice;
            public TimeSpan totalDays;

            public Trip(SingleFlight departureFlight, SingleFlight returnFlight)
            {
                this.departureFlight = departureFlight;
                this.returnFlight = returnFlight;
                this.totalPrice = departureFlight.price + returnFlight.price;
                totalDays = TimeSpan.Parse(returnFlight.date.Subtract(departureFlight.date).Days.ToString());
            }

            public override string ToString()
            {
                return $"Departure flight, {departureFlight.date.ToString("d")} at {departureFlight.hour} from {departureFlight.from} to {departureFlight.destination}, \n" +
                    $"Return flight, {returnFlight.date.ToString("d")} at {returnFlight.hour} from {returnFlight.from} to {returnFlight.destination}." +
                    $" \n Total days: {totalDays.ToString("dd")}, Total price: ${totalPrice} ";
            }
        }
    }
}
