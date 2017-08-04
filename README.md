# pricespaidbyward
Using [Land Registry open data on home sales prices](http://landregistry.data.gov.uk/) to calculate the median price paid for UK home sales by ward, local authority, NUTS2 region, and 2017 UK Parliamentary constituency in England & Wales.

## Usage
The software is written in C# and WPF. The included binary should run on any x64 Windows machine. You will need about 5GB of free RAM to parse the full prices paid file but much less to parse a monthly extract. The software expects three files to be in the same folder as the .exe,
1. `allpostcodes.csv`
2. `LAsToRegionCodes.tsv`
3. `Something About Wardcode to constituency`
If they're not there it will crash.
It will ask you to point to a third file containing [prices paid data from The Land Registry](http://landregistry.data.gov.uk/) -- I strongly recommend using one month of data during testing. You can use the full dataset later.

The software creates two outputs,
1. `pricespaid_augmented.tsv` -- a tab-seperated value with the following columns -- Sale Year, Ward Code, Local Authority Code, Constituency Code, NUTS2 region code, Price Paid. Each row represents a home sale.
2. `pricespaidbyward.tsv` -- a tab-seperated value with the following columns -- Region code (ward, local authority, constituency, or NUTS2), Year, Median price paid in that year, Number of recorded sales in that year.

## Cross platform
You should be able to make the software work on other platforms using Mono, but you won't have a UI. You'll have to rewrite bits to work at the commandline. It's probably easier just to find a computer with Windows.

## Median warning
The software works by listing all prices paid in an area from largest to smallest, and then taking the middle figure. This is not strictly the median. With 5 prices, the software correctly picks the third. With 6 prices, the software picks the third, rather than the mean of the third and fourth. With a large enough number of sales in an area the difference is inconsequential and the simplicity in the code and the outputted prices far outweighs the tiny inaccuracy. You can make the correction easily if you want, I think it's best not to.

## Attributions
The code itself relies on no other work, but the included data files have licenses.
1. The Postcode to Ward code lookup is from Code-Point Open by the Ordnance Survey. It contains OS data © Crown copyright and database right 2017. It contains Royal Mail data © Royal Mail copyright and Database right 2017. It contains National Statistics data © Crown copyright and database right 2017.
2. The [Ward to Local Authority and Region code lookup](http://geoportal.statistics.gov.uk/datasets/f52c48ea5cf2494b824d35f4be88f2ec_0) is via The ONS and available under a [custom version of the Open Government License](https://www.ons.gov.uk/methodology/geography/licences).
3. The Land Registry data that this tool is designed to parse is available under the [Open Government License](http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3/). The Data produced by HM Land Registry © Crown copyright 2017.

## License
The software is available under a [CC-BY 4.0 license](https://creativecommons.org/licenses/by/4.0/). You are free to use and adapt the code however you like, with attributions. 
Copyright Thomas Forth, imactivate, 2017
