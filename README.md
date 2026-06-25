# Louis's submission for Serif Health

Working from this spec:
https://github.com/serif-health/takehome/blob/main/FS_README.md

This would have been my first Go app, and I started that way. Quickly I figured out I couldn't both think through the problem and learn Go at the same time, so I created the backend in .net.

With more time, my next step would be to convert the backend to Go, with the C# logic as a guide.

-- Louis O'Callaghan (louieoc@gmail.com)


## Getting started

1. Clone this repo.
1. Install the [.NET SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
1. Install [Node.js](https://nodejs.org/en)


### Start the API

- For the .Net API, open the solution in Visual Studio or VS Code.
    - If Visual Studio, set SearchApi as the startup project and hit F5 (or right-click the SearchApi project and select Debug --> Start New Instance).
    - If VS Code, cd into the `./backend_cs/SearchApi` folder and run `dotnet run`
    - Now you can test the API with Scalar at [http://localhost:5189/scalar/v1](http://localhost:5189/scalar/v1).
- Note: the first time it's queried, the API should download and cache the index and rate files to `./backend_cs/test_files`
- Also note: for https you could run `dotnet run --launch-profile https` and the port will be `7200`, but the frontend isn't set up for that


### Start the frontend

- open a separate terminal window
- cd into `./frontend`
- run `npm run dev`
- visit [http://localhost:5173/](http://localhost:5173/)


## Project structure

At the high level:

- `backend_cs` folder (the .net version)
    - `SearchApi` -- ASP.NET Core Web API wrapping the SearchService logic
        - Swagger/Scalar: [https://localhost:7200/scalar/v1](https://localhost:7200/scalar/v1)
    - `SearchCli` -- a quick cli I used for trying things out, I can toss it now
    - `SearchCore`
        - `Domain` -- classes to describe the domain, and mapping logic
        - `Services` -- classes for downloading and parsing files, and the SearchService that brings it all together
    - `SearchCoreTests`
        - test category "Integration" effectively replace the cli project. These tests may not pass unless you download index and rate files into the `test_files` folder
        - there are also some unit tests that should pass anytime
    - `test_files` -- index and rate files are stored here. This folder is ignored in source control but the SearchApi will create it and download files into it
- `frontend` -- the Vue UI

### Data flow

1. The frontend receives search parameters from the user and makes a request of the API
    - bill code is required; npi and ein are optional
1. The API downloads and parses CMS rate files
    1. Parse the ToC/index file
        - the API has a hard-coded reference to the index file URL
        - if the index file is not already downloaded, download and cache it
        - parse the json file contents
        - get the list of rate files
    1. Parse the rate files
        - if the rate file is not already downloaded, download and cache it
        - parse the json file contents
        - first loop through the bill code references
            - capture pricing data as it's related to the bill code/medical procedure and to the list of provider group ids
            - maintain a dictionary linking the provider group id to a list of objects representing the rates
            - maintain a separate list of the output shapes which link a bill code/procedure to the list of objects representing the rates
            - if ein or npi filtering is present, ensure each group id has at least one provider matching the filters
        - loop through the provider references
            - retrieve the object lists from the provider group id dictionary
            - add the provider data to the objects, unless the provider is being filtered out by ein or npi. Since they are linked by reference, the output shape object will also be updated            
    1. Return the output as json
1. The frontend receives the json and displays the data in table form


## What I would do with more time

1. write the backend in Go. It was taking me long enough to understand the data that I couldn't also start thinking in a new language.
1. make the mapping and filtering more elegant. First, make it work...
1. create entirely separate output classes from the file ingestion classes
    - I'm currently returning some of the same models as they appear in the source files, which in at least one instance has more fields than I want


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

- http is ok vs self-signed https. I understand some things like geolocation would require https, so that's something to be aware of
    - we get a warning from asp.net when it tries to redirect an http request to https, but I'm assured this is harmless

## Observations/notes

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

use billCode 10040 for acne surgery

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


