using System;
using System.IO;
using System.Text.Json;
using SearchCore.Domain;

/// <summary>
/// CLI to test out the search logic and ensure it works as expected
/// </summary>
class Program
{
	static int Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.Error.WriteLine("need a path and a bill code");
			return 1;
		}

		string path = args[0];
		string billCode = args[1];

		if (!File.Exists(path))
		{
			Console.Error.WriteLine($"File not found: {path}");
			return 1;
		}

		if (string.IsNullOrWhiteSpace(billCode))
		{
			Console.Error.WriteLine("bill code not supplied");
			return 1;
		}

		string json = File.ReadAllText(path);
		InNetworkRoot? root = JsonSerializer.Deserialize<InNetworkRoot>(json);

		if (root?.ProviderReferences is null)
		{
			Console.Error.WriteLine("No provider_references found in file.");
			return 1;
		}

		if (root?.InNetworks is null)
		{
			Console.Error.WriteLine("No in_networks found in file.");
			return 1;
		}

		var mapper = new DomainMapper();
		var mapped = mapper.MapInNetworkRoot(root, billCode, null, null, null, null);

		// write out results

		foreach (var item in mapped)  //var key in codeRatesHash.Keys)
		{
			Console.WriteLine("Procedure:");
			Console.WriteLine($"	Name: {item.Procedure?.Name}");
			Console.WriteLine($"	Bill code: {item.Procedure?.BillingCode}");

			foreach (var rate in item.GroupRates)
			{
				Console.WriteLine("  Providers:");
				foreach (var provider in rate.Providers ?? [])
				{
					Console.WriteLine($"	Name: {provider.Tin?.BusinessName} Ein: {provider?.Tin?.Value}");
				}

				Console.WriteLine("  Prices:");
				foreach (var price in rate.NegotiatedPrices ?? [])
				{
					Console.WriteLine($"	${price.NegotiatedRate}");
				}
			}
		}

		return 0;
	}
}
