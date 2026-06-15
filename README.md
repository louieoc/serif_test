# Louis's submission for Serif Health

Working from this spec:
https://github.com/serif-health/takehome/blob/main/FS_README.md

This would have been my first Go app, and I started that way. Quickly I figured out I couldn't both think through the problem and learn Go at the same time, so I created the backend in .net.

With more time, my next step would be to convert the backend to Go, with the C# logic as a guide.

-- Louis O'Callaghan (louieoc@gmail.com)


## Getting started

### Start the API

- For the .Net API, open the solution in Visual Studio or VS Code.
  - If Visual Studio, set SearchApi as the startup project and hit F5 (or right-click the SearchApi project and select Debug --> Start New Instance).
  - If VS Code, cd into the `./backend_cs/SearchApi` folder and run `dotnet run --launch-profile https`
- Now you can [test the API with Scalar](https://localhost:7200/scalar/v1).
- Note that the first time it runs, it should download and cache the index and rate files to `./backend_cs/test_files`


### Start the frontend

TODO

## Assumptions

- definition of done:
  - input a billing code and display a list of provider groups and rates
  - allow filtering by provider npi or ein (perhaps on UI side)

- need to create a simplified model linking billing code, provider group, and rate/price
  - ein, npi, provider group, negotiated rate
  - plan name, issuer, billing code, facility, location (as per hints)

- provider group is the unit/atomicity, not provider?
- we can ignore the "allowed amount" files as the instructions don't mention them and are explicit about "in network"
- should loop through provider reference block and create a list of provider groups/eins/npis

- assume that provider_references.provider_groups is a list of providers, not groups. I assume this because each entity in the list shares one provider_group_id, and each has one TIN. I'll be naming classes/structs based on this assumption

- assume each provider group has at least one NPI
- assume TIN is always EIN
- assume billing code type and version are always the same (would need a compound index kind of thing otherwise, i.e. billing_code + type + version)
  - might need something like that for additional_information and billing_code_modifier fields? Assume no for now (billing code 300 will return a lot of info)


## Project structure

At the high level:

- `backend_cs` folder (the .net version)
  - `SearchApi` -- ASP.NET Core Web API wrapping the SearchService logic
  - `SearchCli` -- a quick cli I used for trying things out, I can toss it now
  - `SearchCore`
    - `Domain` -- classes to describe the domain, and mapping logic
    - `Services` -- classes for downloading and parsing files, and the SearchService that brings it all together
  - `SearchCoreTests` -- integration tests replacing the cli project. These tests won't pass unless you download index and rate files into the `test_files` folder
  - `test_files` -- index and rate files are stored here. In theory the SearchService will download rate files if they're not cached already but I haven't tested it

- `frontend` folder
  - the Vue UI


## Api

Swagger: [https://localhost:7200/scalar/v1](https://localhost:7200/scalar/v1)

use billCode 10040 for acne surgery

~25 seconds latency to parse the index and 2 rates files


## Observations

sometimes mulitple NPI's per TIN

TIN: Tax Id Number

Reporting entity: Centene Management Company LLC

not fidelis. Fidelis is insurer? Issuer

can I make a dictionary of NPI to EIN? No, an NPI can have multiple EINs

code 278 is interesting, very high and low prices for the same thing

code 300 is like a catchall? It covers a lot of procedures

provider_group_id to ein is NOT 1:1

look for procedure name TRICHOMONAS VAGINALIS AMPLIF, it has tons of rates and prices

lots of negotiated_rates (ties to provider_references) and sometimes prices per rate

billing code 300, type rc, version 2022

"billing_code": "260" IV thereapy general

lots of negotiated_rates, only one price per rate

code 19083: appears in both rates files, same name with different providers and plans. I wonder if those should be consolidated

### NPIs

NPI: National Provider Identifier; 10 digit

why multiple NPIs per provider group? Is it like a doctor? Can one NPI appear in multiple provider groups?

From fact sheet:
- Type 1: For individual health care providers, such as physicians, nurse practitioners and sole proprietors. Individuals are only eligible for one NPI.
- Type 2: For health care organizations, such as hospitals, nursing homes, and physician groups. Organizations can have multiple NPIs.

An individual who is a health care provider and is incorporated, can obtain an NPI for themselves (Type 1) and an NPI for their corporation or LLC (Type 2). 


### Data

#### index file (focus on fields we care about):

```
reporting_entity_name (e.g. Centene Management Company LLC)
last_updated_on
version
  reporting_structure (array)
    in_network_files (array)
      description
      location (url)
    allowed_amount_file (not array) <-- IGNORE
      description
      location (url)
    reporting_plans (array)
      plan_name
      plan_id_type
      plan_id
      plan_market_type
      issuer_name (e.g. fidelis care)

```

#### in network file

```
reporting_entity_name (e.g. centene)
reporting_entity_type (e.g. third party administrator)
version
last_updated_on
provider_references (array)
  provider_group_id (sequential from 1, appears to be unique to the file only)
  network_name (array -- often empty?)
  provider_groups (array)
    npi (array)
    tin (object)
      type: ein e.g. --> how often does this deviate?
      value
      business_name

in_network (array)
  negotiation_arrangement (seems to always be "ffs")
  name (e.g. ACNE SURGERY)
  billing_code_type
  billing_code_type_version
  billing_code
  description (of the procedure)
  negotiated_rates (array)
    negotiated_prices (array)
      negotiated_type (appears to always be "negotiated", perhaps ignore)
      negotiated_rate (currency/decimal)
      expiration_date (all seem to be 9999-12-31 so far)
      service_code (array)
      billing_class (e.g. professional, institutional)
      billing_code_modifier (array -- not always present, I have seen multiples)
      additional_information (number string, not always present, seems to distinguish among prices when there's more than one)
      setting (e.g. outpatient)
  provider_references (array) <-- id link to the provider_references provider_group_id

```


