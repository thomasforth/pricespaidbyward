# pricespaidbyward
Using [Land Registry open data on home sales prices](http://landregistry.data.gov.uk/) to calculate the median price paid for UK home sales by ward, local authority, NUTS2 region, and 2017 UK Parliamentary constituency in England & Wales.

## Usage
The software is written in C# and WPF. The included binary should run on any x64 Windows machine. You will need about leasat 5GB of free RAM. The software expects two files to be in the same folder as the .exe,
1. `2015.04.03.postcode_to_constituency_lookup.tsv`
2. `LAsToRegionCodes.tsv`
If they're not there it will crash.
It will ask you to point to a third file containing [prices paid data from The Land Registry](http://landregistry.data.gov.uk/).

The software creates two outputs,
1. `pricespaid_augmented.tsv` -- a tab-seperated value with the following columns -- Sale Year, Ward Code, Local Authority Code, Constituency Code, NUTS2 region code, Price Paid. Each row represents a home sale.
2. `pricespaidbyward.tsv` -- a tab-separte value with the following columns -- Region code (ward, local authority, constituency, or NUTS2), Year, Median price paid in that year, Number of recorded sales in that year.

## Cross platform
You should be able to make the software work on other platforms using Mono, but you won't have a UI. You'll have to rewrite bits to work at the commandline. It's probably easier just to find a computer with Windows.

## Median warning
The software works by listing all prices paid in an area from largest to smallest, and then taking the middle figure. This is not strictly the median. With 5 prices, the software correctly picks the third. With 6 prices, the software picks the third, rather than the mean of the third and fourth. With a large enough number of sales in an area the difference is inconsequential and the simplicity in the code and the outputted prices far outweighs the tiny inaccuracy. You can make the correction easily if you want, I think it's best not to.

## License
The software is available under a [CC-BY 4.0 license](https://creativecommons.org/licenses/by/4.0/). You are free to use and adapt the code however you like, with attributions. 
Copyright Thomas Forth, imactivate, 2017
