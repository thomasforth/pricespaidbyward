# pricespaidbyward
Using [Land Registry open data on home sales prices](http://landregistry.data.gov.uk/) to calculate the median price paid for UK home sales by ward, local authority, NUTS2 region, and 2017 UK Parliamentary constituency in England & Wales.

## Usage
The software is written in C# and WPF. The included binary should run on any x64 Windows machine. You will need about 5GB of free RAM to parse the full prices paid file but much less to parse a monthly extract. The software expects four files to be in the same folder as the .exe,
1. `Postcode_to_LocalAuthorityCode_to_Wardcode.csv`
2. `2015.04.03.postcode_to_constituency_lookup.tsv`
3. `Ward_to_Local_Authority_District_to_County_to_Region_to_Country_December_2016_Lookup_in_United_Kingdom_V2.csv`
4. `CPI_deflator_2017_October2017.csv`

If they're not there it will crash.
It will ask you to point to a fifth file containing [prices paid data from The Land Registry](http://landregistry.data.gov.uk/) -- I strongly recommend using the most recent year of data during testing. You can use the full dataset later.

The software creates two outputs,
1. `pricespaid_augmented.tsv` -- a tab-seperated value with the following columns -- Sale Year, Ward Code, Local Authority Code, Constituency Code, NUTS2 region code, Price Paid. Each row represents a home sale.
2. `pricespaidbyward.tsv` -- a tab-seperated value with the following columns -- Region code (ward, local authority, constituency, or NUTS2), Year, Median price paid in that year, Number of recorded sales in that year.

## Cross platform
You should be able to make the software work on other platforms using Mono, but you won't have a UI. You'll have to rewrite bits to work at the commandline. It's probably easier just to find a computer with Windows.

## Median warning
The software works by listing all prices paid in an area from largest to smallest, and then taking the middle figure. This is not strictly the median. No price is reported if there are fewer than 10 sales in a ward in a year. With 11 prices, the software correctly picks the sixth. With 12 prices, the software picks the sixth, rather than the mean of the sixth and seventh. With a large enough number of sales in an area the difference is inconsequential and the simplicity in the code and the outputted prices far outweighs the tiny inaccuracy. You can make the correction easily if you want. I think it's best not to. Differences are at most a few percent. As an example, in a list of prices where the two central prices were £125000 and £125995 I think that returning £125000 rather than the true median of £125497.50 is a simplification worth making. There is an extra benefit of doing this; it means that the median price for each ward now always refers to a specific house sale. For each ward, in each year there is a unique "median house".

## Type of sales included
The prices paid dataset reports sales data for five types of housing. Specifically, [`D = Detached, S = Semi-Detached, T = Terraced, F = Flats/Maisonettes, O = Other`](https://www.gov.uk/guidance/about-the-price-paid-data#explanations-of-column-headers-in-the-ppd). We exclude sales ot type O. There are very few such sales. They are regularly extremely expensive and often refer to sales that don't seem residential.
There are two possible values for PPD Category Type in the prices paid dataset. We exclude type "B" as these homes are sold at something other than the full market value. [Further explanation](https://www.gov.uk/guidance/about-the-price-paid-data#explanations-of-column-headers-in-the-ppd).

## Inflation
Prices are expressed in constant 2017 £s (July 2017 = 100), deflated using [The ONS monthly CPI deflator](https://www.ons.gov.uk/economy/inflationandpriceindices/timeseries/d7bt/mm23).

## Attributions
The code itself relies on no other work, but the included data files have licenses.
1. The Postcode to Ward code lookup is from [Code-Point Open](https://www.ordnancesurvey.co.uk/business-and-government/products/code-point-open.html) by the Ordnance Survey. It contains OS data © Crown copyright and database right 2017. It contains Royal Mail data © Royal Mail copyright and Database right 2017. It contains National Statistics data © Crown copyright and database right 2017.
2. The [Postcode to Parliamentary Constituency lookup](https://github.com/flashton2003/postcode_to_constituency) is via Flashtron2003 on GitHub.
3. The [Ward to Region Code lookup](http://geoportal.statistics.gov.uk/datasets/ward-to-local-authority-district-to-county-to-region-to-country-december-2016-lookup-in-united-kingdom-v2) is via The ONS and available under a [custom version of the Open Government License](https://www.ons.gov.uk/methodology/geography/licences).
4. The Land Registry data that this tool is designed to parse is available under the [Open Government License](http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/). The Data produced by HM Land Registry © Crown copyright 2017.
5. The inflation data is from  [The ONS monthly CPI deflator](https://www.ons.gov.uk/economy/inflationandpriceindices/timeseries/d7bt/mm23).

## License
The software is available under a [CC-BY 4.0 license](https://creativecommons.org/licenses/by/4.0/). You are free to use and adapt the code however you like, with attributions. 
Copyright Thomas Forth, imactivate, 2017
