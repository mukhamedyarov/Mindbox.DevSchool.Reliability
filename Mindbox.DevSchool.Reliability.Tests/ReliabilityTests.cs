using System.Net;

namespace Mindbox.DevSchool.Reliability.Tests;

[TestClass]
public sealed class ReliabilityTests
{
	[TestMethod]
	public async Task Reliability()
	{
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("http://localhost:5013");
		httpClient.Timeout = TimeSpan.FromSeconds(3);

		var makeApiCallTasks = Enumerable.Range(0, 4)
			.Select(_ => httpClient.GetAsync("weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		var responses = await Task.WhenAll(makeApiCallTasks);

		foreach (var httpResponseMessage in responses)
		{
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
		}
	}

	[TestMethod]
	public async Task Reliability2()
	{
		using var timeoutHttpClient = new HttpClient();
		timeoutHttpClient.BaseAddress = new Uri("http://localhost:5013");
		timeoutHttpClient.Timeout = TimeSpan.FromMilliseconds(1);
		
		var timeoutTasks = Enumerable.Range(0, 1000)
			.Select(_ => timeoutHttpClient.GetAsync("ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		try
		{
			await Task.WhenAll(timeoutTasks);
		}
		catch
		{
			// ignore
		}

		await Task.Delay(TimeSpan.FromSeconds(1));
		
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("http://localhost:5013");
		httpClient.Timeout = TimeSpan.FromSeconds(3);
		
		var apiCallTasks = Enumerable.Range(0, 4)
			.Select(_ => httpClient.GetAsync("ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		var responses = await Task.WhenAll(apiCallTasks);

		foreach (var httpResponseMessage in responses)
		{
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
		}
	}
}