// See https://aka.ms/new-console-template for more information
using Relewise_assignment.Mappers;
using Relewise_assignment.Mappers.Interfaces;

var rawWorker = new RawDataMapperJob(new System.Globalization.CultureInfo("en-US"));
Console.WriteLine("Raw mapper output: " + await rawWorker.Execute(new JobArguments(Guid.Empty, null, null), null, null, new CancellationToken()));
var jsonWorker = new JSONDataMapperJob(new System.Globalization.CultureInfo("en-US"));
Console.WriteLine("JSON mapper output: " + await jsonWorker.Execute(new JobArguments(Guid.Empty, null, null), null, null, new CancellationToken()));
var shoppingFeedWorker = new ShoppingFeedDataMapperJob(new System.Globalization.CultureInfo("en-US"));
Console.WriteLine("Shopping feed mapper output: " + await shoppingFeedWorker.Execute(new JobArguments(Guid.Empty, null, null), null, null, new CancellationToken()));
Console.ReadLine();