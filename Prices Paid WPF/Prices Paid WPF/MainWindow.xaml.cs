﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Prices_Paid_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            if (File.Exists("./pricespaid_augmented.tsv"))
            {
                LoadPostcodeButton.IsEnabled = false;
                Status.Text = "Data will be loaded from pricespaid_augmented.tsv -- if this is out-of-date, delete it and restart this application.";
            }
        }

        private Dictionary<string, string> WardLookup = new Dictionary<string, string>();
        private Dictionary<string, string> DistrictLookup = new Dictionary<string, string>();
        private Dictionary<string, string> NUTSLookup = new Dictionary<string, string>();
        private Dictionary<string, string> ConstituencyLookup = new Dictionary<string, string>();

        private Dictionary<string, string> DistrictToNUTSLookup = new Dictionary<string, string>();

        private async void LoadPostcodeButton_Tapped(object sender, RoutedEventArgs e)
        {
            Status.Text = "Reading local authorities file.";
            // regions files has columns -- LA code     LA name 	Region code 	Region name 	NUTS region
            string regionsfilename = @"./Local_Authority_District_to_Region.tsv";
            //StorageFile regionsFile = await File StorageFile.GetFileFromApplicationUriAsync(new Uri(regionsfilename));

            List<string> regionlines = new List<string>();
            await Task.Run(() =>
            {
                regionlines = File.ReadAllLines(regionsfilename).ToList();
            });

            foreach (string regionline in regionlines)
            {
                string[] splitregionline = regionline.Split('\t');
                string districtcode = splitregionline[0];
                string NUTscode = splitregionline[4];
                DistrictToNUTSLookup.Add(districtcode, NUTscode);
            }

            regionlines.Clear();


            Status.Text = "Reading constituencies file.";
            // constituency file has columns -- postcode    constituencycode    constituencyname

            string constituenciesfilename = @"./2015.04.03.postcode_to_constituency_lookup.tsv";
            //StorageFile regionsFile = await File StorageFile.GetFileFromApplicationUriAsync(new Uri(regionsfilename));
            //IList<string> regionlines = await FileIO.ReadLinesAsync(regionsFile);
            string[] constituencylines = File.ReadAllLines(constituenciesfilename);

            foreach (string constituencyline in constituencylines)
            {
                string[] splitregionline = constituencyline.Split('\t');
                string postcode = splitregionline[0].Replace(" ", String.Empty);
                string constituencycode = splitregionline[1];
                ConstituencyLookup.Add(postcode, constituencycode);
            }

            constituencylines = null;

            // postcode file has columns -- Postcode	Positional_quality_indicator	Eastings	Northings	Country_code	NHS_regional_HA_code	NHS_HA_code	Admin_county_code	Admin_district_code	Admin_ward_code

            Status.Text = "Reading postcode file.";
            //string filename = @"./Postcode_to_LocalAuthorityCode_to_Wardcode.csv";
            string filename = @"./postcode_toward_tolocalauthority.tsv";
            //StorageFile sFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(filename));
            //IList<string> lines = await FileIO.ReadLinesAsync(sFile);
            string[] lines = File.ReadAllLines(filename);
            Status.Text = "Loading postcodes...";
            int loadedCounter = 0;
            foreach (string line in lines)
            {
                try
                {
                    string[] splitLine = line.Split('\t');
                    string postcode = splitLine[0].Replace("\"", String.Empty).Replace(" ", String.Empty);
                    string districtcode = splitLine[2].Replace("\"", String.Empty);
                    string wardcode = splitLine[1].Replace("\"", String.Empty);
                    WardLookup.Add(postcode, wardcode);
                    DistrictLookup.Add(postcode, districtcode);

                    string regioncode = "";
                    DistrictToNUTSLookup.TryGetValue(districtcode, out regioncode);
                    NUTSLookup.Add(postcode, regioncode);
                }
                catch
                {
                    Debug.WriteLine("Skipped line: " + line);
                }
                loadedCounter++;
                if (loadedCounter % 10000 == 0)
                {
                    await Task.Delay(1);
                    Status.Text = string.Concat(loadedCounter.ToString(), " of ", lines.Count(), " postcodes loaded.");
                }

            }
            Status.Text = WardLookup.Count + " postcodes loaded.";
        }

        List<PricePaid> ListOfPrices = new List<PricePaid>();
        private async void LoadPricesPaidButton_Tapped(object sender, RoutedEventArgs e)
        {
            if (File.Exists("./pricespaid_augmented.tsv"))
            {
                Status.Text = "Loading pricespaid from existing pricespaid_augmented.tsv file";
                int loadedCounter = 0;
                File.OpenRead("./pricespaid_augmented.tsv");
                FileStream ppfilestream = File.OpenRead("./pricespaid_augmented.tsv");
                using (StreamReader inputStream = new StreamReader(ppfilestream))
                {
                    long estimatedLengthInLines = ppfilestream.Length / 40;
                    while (inputStream.Peek() >= 0)
                    {
                        string line = inputStream.ReadLine();
                        string[] splitLine = line.Split('\t');
                        PricePaid _pricePaid = new PricePaid();
                        _pricePaid.year = int.Parse(splitLine[0]);
                        _pricePaid.wardcode = splitLine[1];
                        _pricePaid.districtcode = splitLine[2];
                        _pricePaid.constituencycode = splitLine[3];
                        _pricePaid.NUTScode = splitLine[4];
                        _pricePaid.price = int.Parse(splitLine[5]);
                        _pricePaid.deflatedPrice = int.Parse(splitLine[6]);

                        ListOfPrices.Add(_pricePaid);

                        loadedCounter++;
                        if (loadedCounter % 100000 == 0)
                        {
                            await Task.Delay(1);
                            Status.Text = loadedCounter.ToString() + " of about " + estimatedLengthInLines + " prices paid loaded.";
                        }
                    }
                }
                Status.Text = loadedCounter.ToString() + " prices paid loaded.";
            }
            else
            {

                // prices paid file has columns -- "{4E95D757-461E-EDA1-E050-A8C0630539E2}","227500","2017-03-28 00:00","BH1 3QR","F","N","L","KINGS COURTYARD, 30 - 32","FLAT 21","KNYVETON ROAD","","BOURNEMOUTH","BOURNEMOUTH","BOURNEMOUTH","A","A"
                Status.Text = "Reading prices paid file.";
                // string filename = "ms-appx:///pp-2017.csv"; // During testing only
                //string filename = "ms-appx:///pp-complete.csv";

                var openPicker = new OpenFileDialog();
                openPicker.InitialDirectory = Environment.CurrentDirectory;
                openPicker.Filter = "csv files(*.csv) | *.csv";
                openPicker.ShowDialog();

                // Application now has read/write access to the picked file             

                // StorageFile sFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(filename));
                //IList<string> lines = await FileIO.ReadLinesAsync(ppfile); // haha -- reading the whole file 4GB will break everything

                Stream ppfilestream = openPicker.OpenFile();
                Status.Text = "Loading prices paid...";
                List<string> ppoutputText = new List<string>();

                int loadedCounter = 0;
                using (StreamReader inputStream = new StreamReader(ppfilestream))
                {
                    long estimatedLenghtInLines = ppfilestream.Length / 160;
                    while (inputStream.Peek() >= 0)
                    {
                        string line = inputStream.ReadLine();
                        try
                        {
                            string[] splitLine = line.Split(',');
                            DateTime SaleDate = DateTime.Parse(splitLine[2].Replace("\"", String.Empty));
                            int pricePaid = int.Parse(splitLine[1].Replace("\"", String.Empty));
                            string wardcode = "";
                            string districtcode = "";
                            string NUTScode = "";
                            string constituencycode = "";

                            string trimmedPostcode = splitLine[3].Replace("\"", String.Empty).Replace(" ", String.Empty);

                            string saleType = splitLine[4].Replace("\"", String.Empty);
                            string atFullValue = splitLine[14].Replace("\"", String.Empty);
                            if (saleType == "O" || atFullValue == "B") {
                                // saleType O are not residential sales and are excluded.
                                // atFullValue B is below full value and is excluded on advice from Land Registry and the BBC
                            }
                            else
                            {
                                if (WardLookup.TryGetValue(trimmedPostcode, out wardcode) == false)
                                {
                                }
                                else
                                {
                                    DistrictLookup.TryGetValue(trimmedPostcode, out districtcode);
                                    NUTSLookup.TryGetValue(trimmedPostcode, out NUTScode);
                                    ConstituencyLookup.TryGetValue(trimmedPostcode, out constituencycode);

                                    PricePaid _pricePaid = new PricePaid();
                                    //_pricePaid.saleDate = SaleDate;
                                    _pricePaid.year = SaleDate.Year;
                                    _pricePaid.month = SaleDate.Month;
                                    _pricePaid.wardcode = wardcode;
                                    _pricePaid.districtcode = districtcode;
                                    _pricePaid.NUTScode = NUTScode;
                                    _pricePaid.price = pricePaid;
                                    _pricePaid.deflatedPrice = calculateDeflatedPrice(SaleDate.Month, SaleDate.Year, pricePaid);
                                    _pricePaid.constituencycode = constituencycode;
                                    ListOfPrices.Add(_pricePaid);

                                    ppoutputText.Add(SaleDate.Year + "\t" + wardcode + "\t" + districtcode + "\t" + constituencycode + "\t" + NUTScode + "\t" + pricePaid + "\t" + _pricePaid.deflatedPrice);
                                }
                            }

                        }
                        catch
                        {
                            Debug.WriteLine("Skipped line: " + line);
                        }
                        loadedCounter++;
                        if (loadedCounter % 10000 == 0)
                        {
                            await Task.Delay(1);
                            Status.Text = loadedCounter.ToString() + " of about " + estimatedLenghtInLines + " prices paid loaded.";
                        }
                    }
                    Status.Text = "Writing pricespaid_augmented.tsv. Be patient it's " + loadedCounter + " lines of text.";
                    await Task.Run(() =>
                    {
                        File.WriteAllLines("./pricespaid_augmented.tsv", ppoutputText);
                    });
                    Status.Text = loadedCounter + " prices paid loaded, and saved as pricespaid_augmented.tsv.";

                }
            }
        }

        List<Deflators> deflatorsList = new List<Deflators>();
        List<string> ONSMonthNames = new List<string> { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };
        private int calculateDeflatedPrice(int month, int year, int nominalPrice)
        {
            // if not loaded, load the deflators
            if (deflatorsList.Count == 0)
            {
                string regionsfilename = @"./CPI_deflator_2017_October2017.csv";
                //StorageFile regionsFile = await File StorageFile.GetFileFromApplicationUriAsync(new Uri(regionsfilename));
                List<string> deflatorLines = File.ReadAllLines(regionsfilename).ToList();

                for (int i= 8; i < deflatorLines.Count; i++)
                {
                    string[] splitline = deflatorLines[i].Split(',');
                    string yeararea = splitline[0];
                    string[] splityear = yeararea.Split(' ');
                    string monthName = splityear[1];
                    int monthIndex = ONSMonthNames.IndexOf(monthName);
                    Deflators monthlyDeflator = new Deflators();
                    monthlyDeflator.month = monthIndex;
                    monthlyDeflator.year = int.Parse(splityear[0]);
                    monthlyDeflator.deflator = decimal.Parse(splitline[1]);
                    deflatorsList.Add(monthlyDeflator);
                }
            }

            // NOT IMPLEMENTED YET
            decimal deflator = deflatorsList.Where(x => x.month == month && x.year == year).First().deflator;
            decimal deflatedPrice = nominalPrice * 100 / deflator;
            return (int)deflatedPrice;
        }

        public class Deflators
        {
            public int month { get; set; }
            public int year { get; set; }
            public decimal deflator { get; set; }
        }

        List<string> RealOutputText = new List<string>();
        List<string> NominalOutputText = new List<string>();

        private async void CreateOutputButton_Tapped(object sender, RoutedEventArgs e)
        {
            // get list of unique wards
            Status.Text = "Listing unique ward codes...";
            List<string> UniqueWardCodes = new List<string>();
            await Task.Run(() =>
            {
                UniqueWardCodes = ListOfPrices.Select(x => x.wardcode).Distinct().ToList();
            });

            // get list of unique districts
            Status.Text = "Listing unique local authority codes...";
            List<string> UniqueDistrictCodes = new List<string>();
            await Task.Run(() =>
            {
                UniqueDistrictCodes = ListOfPrices.Select(x => x.districtcode).Distinct().ToList();
            });

            // get list of unique regions
            Status.Text = "Listing unique region codes...";
            List<string> UniqueRegionCodes = new List<string>();
            await Task.Run(() =>
            {
                UniqueRegionCodes = ListOfPrices.Select(x => x.NUTScode).Distinct().ToList();
            });

            // get list of unique constituencies
            Status.Text = "Listing unique constituency codes...";
            List<string> UniqueConstituencyCodes = new List<string>();
            await Task.Run(() =>
            {
                UniqueConstituencyCodes = ListOfPrices.Select(x => x.constituencycode).Distinct().ToList();
            });

            // get list of unique years
            Status.Text = "Listing unique years in the data...";
            List<int> UniqueYears = new List<int>();
            await Task.Run(() =>
            {
                UniqueYears = ListOfPrices.Select(x => x.year).Distinct().ToList();
                UniqueYears.Sort();
            });

            // loop through all years
            foreach (int Year in UniqueYears)
            {
                RealOutputText.Clear();
                NominalOutputText.Clear();

                Status.Text = "Analysing sales for " + Year;

                // List method
                List<PricePaid> SalesThisYear = ListOfPrices.Where(x => x.year == Year).ToList();

                if (WardCheckBox.IsChecked == true) {
                    // for every ward, calculate the median price
                    int count = 0;
                    UniqueWardCodes.Remove("");
                    foreach (string wardcode in UniqueWardCodes)
                    {
                        List<PricePaid> SalesIntheWard = SalesThisYear.Where(x => x.wardcode == wardcode).OrderBy(x => x.deflatedPrice).ToList();

                        int medianRealPrice = 0;
                        int medianNominalPrice = 0;

                        if (SalesIntheWard.Count < 10)
                        {
                            // fewer than 10 sales in a ward and we don't output a value
                            NominalOutputText.Add(wardcode + "\t" + Year + "\t" + "" + "\t" + SalesIntheWard.Count);
                            RealOutputText.Add(wardcode + "\t" + Year + "\t" + "" + "\t" + SalesIntheWard.Count);
                        }
                        else { 
                            medianNominalPrice = SalesIntheWard.ElementAt((int)Math.Floor((decimal)SalesIntheWard.Count / 2)).deflatedPrice;
                            medianRealPrice = SalesIntheWard.ElementAt((int)Math.Floor((decimal)SalesIntheWard.Count / 2)).price;
                            NominalOutputText.Add(wardcode + "\t" + Year + "\t" + medianNominalPrice + "\t" + SalesIntheWard.Count);
                            RealOutputText.Add(wardcode + "\t" + Year + "\t" + medianRealPrice + "\t" + SalesIntheWard.Count);
                        }


                        count++;
                        if (count % 10 == 0)
                        {
                            await Task.Delay(1);
                            Status.Text = count.ToString() + " of " + UniqueWardCodes.Count + " wards calculated for " + Year + ".";
                        }
                    }
                }

                if (DistrictsCheckBox.IsChecked == true)
                {
                    // for every district, calculate the median price
                    int districtcount = 0;
                    UniqueDistrictCodes.Remove("");
                    foreach (string districtcode in UniqueDistrictCodes)
                    {
                        List<PricePaid> SalesIntheDistrict = SalesThisYear.Where(x => x.districtcode == districtcode).OrderBy(x => x.deflatedPrice).ToList();

                        int medianRealPrice = 0;
                        int medianNominalPrice = 0;

                        if (SalesIntheDistrict.Count > 1)
                        {
                            // this isn't precisely the median -- but the tiny error it introduces is so small that it doesn't matter. And it's worth it for the simplicity of the outcome.
                            medianRealPrice = SalesIntheDistrict.ElementAt((int)Math.Floor((decimal)SalesIntheDistrict.Count / 2)).deflatedPrice;
                            medianNominalPrice = SalesIntheDistrict.ElementAt((int)Math.Floor((decimal)SalesIntheDistrict.Count / 2)).price;
                        }

                        RealOutputText.Add(districtcode + "\t" + Year + "\t" + medianRealPrice + "\t" + SalesIntheDistrict.Count);
                        NominalOutputText.Add(districtcode + "\t" + Year + "\t" + medianNominalPrice + "\t" + SalesIntheDistrict.Count);

                        //await FileIO.AppendTextAsync(OutputFile, districtcode + "\t" + Year + "\t" + medianPrice + "\n");
                        districtcount++;
                        if (districtcount % 10 == 0)
                        {
                            await Task.Delay(1);
                            Status.Text = districtcount.ToString() + " of " + UniqueDistrictCodes.Count + " local authorities calculated for " + Year + ".";
                        }
                    }
                }

                if (ConstituenciesCheckBox.IsChecked == true)
                {
                    // for every constituency, calculate the median price
                    int constituencycount = 0;
                    UniqueConstituencyCodes.Remove("");
                    foreach (string constituencycode in UniqueConstituencyCodes)
                    {
                        List<PricePaid> SalesIntheRegion = SalesThisYear.Where(x => x.constituencycode == constituencycode).OrderBy(x => x.deflatedPrice).ToList();

                        int medianRealPrice = 0;
                        int medianNominalPrice = 0;
                        if (SalesIntheRegion.Count > 1)
                        {
                            // this isn't precisely the median -- but the tiny error it introduces is so small that it doesn't matter. And it's worth it for the simplicity of the outcome.
                            medianRealPrice = SalesIntheRegion.ElementAt((int)Math.Floor((decimal)SalesIntheRegion.Count / 2)).deflatedPrice;
                            medianNominalPrice = SalesIntheRegion.ElementAt((int)Math.Floor((decimal)SalesIntheRegion.Count / 2)).price;
                        }
                        RealOutputText.Add(constituencycode + "\t" + Year + "\t" + medianRealPrice + "\t" + SalesIntheRegion.Count);
                        NominalOutputText.Add(constituencycode + "\t" + Year + "\t" + medianNominalPrice + "\t" + SalesIntheRegion.Count);

                        //await FileIO.AppendTextAsync(OutputFile, regioncode + "\t" + Year + "\t" + medianPrice + "\n");

                        constituencycount++;
                        if (constituencycount % 1 == 0)
                        {
                            await Task.Delay(1);
                            Status.Text = constituencycount.ToString() + " of " + UniqueConstituencyCodes.Count + " constituencies calculated for " + Year + ". (There is no data for Scotland and Northern Ireland)";
                        }
                    }
                }

                if (RegionsCheckBox.IsChecked == true)
                {
                    // for every NUTScode, calculate the median price
                    int regioncount = 0;
                    UniqueRegionCodes.Remove("");
                    foreach (string regioncode in UniqueRegionCodes)
                    {
                        List<PricePaid> RealSalesIntheRegion = SalesThisYear.Where(x => x.NUTScode == regioncode).OrderBy(x => x.deflatedPrice).ToList();
                        List<PricePaid> NominalSalesIntheRegion = SalesThisYear.Where(x => x.NUTScode == regioncode).OrderBy(x => x.price).ToList();

                        int medianNominalPrice = 0;
                        int medianRealPrice = 0;
                        if (RealSalesIntheRegion.Count > 1)
                        {
                            // this isn't precisely the median -- but the tiny error it introduces is so small that it doesn't matter. And it's worth it for the simplicity of the outcome.
                            medianNominalPrice = NominalSalesIntheRegion.ElementAt((int)Math.Floor((decimal)NominalSalesIntheRegion.Count / 2)).price;
                            medianRealPrice = RealSalesIntheRegion.ElementAt((int)Math.Floor((decimal)RealSalesIntheRegion.Count / 2)).deflatedPrice;
                        }
                        RealOutputText.Add(regioncode + "\t" + Year + "\t" + medianRealPrice + "\t" + RealSalesIntheRegion.Count);
                        NominalOutputText.Add(regioncode + "\t" + Year + "\t" + medianNominalPrice + "\t" + NominalSalesIntheRegion.Count);
                        //await FileIO.AppendTextAsync(OutputFile, regioncode + "\t" + Year + "\t" + medianPrice + "\n");
                        regioncount++;
                        if (regioncount % 1 == 0)
                        {
                            await Task.Delay(1);
                            Status.Text = regioncount.ToString() + " of " + UniqueRegionCodes.Count + " regions calculated for " + Year + ".";
                        }
                    }
                }
                File.AppendAllLines("./pricespaidbyward_2017£s.tsv", RealOutputText);
                File.AppendAllLines("./pricespaidbyward_nominal.tsv", NominalOutputText);
            }
            Status.Text = "Calculations done. Output written to pricespaidbyward_2017£s.tsv and pricespaidbyward_nominal.tsv.";
        }

        public class PricePaid
        {
            //public DateTime saleDate { get; set; }
            public int year { get; set; }
            public int month { get; set; }
            public int price { get; set; }
            public int deflatedPrice { get; set; }
            public string wardcode { get; set; }
            public string districtcode { get; set; }
            public string NUTScode { get; set; }
            public string constituencycode { get; set; }
        }
    }
}