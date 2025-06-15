using System.Net;

namespace Mindbox.DevSchool.Reliability.Tests;

[TestClass]
public sealed class ReliabilityTests
{
	[TestMethod]
	public async Task Reliability()
	{
		// add threads limit

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

	[TestMethod]
	public async Task Reliability3()
	{
		using var httpClientWithoutTimeout = new HttpClient();
		httpClientWithoutTimeout.BaseAddress = new Uri("http://localhost:5013");

		var tasks = Enumerable.Range(0, 100)
			.Select(_ => httpClientWithoutTimeout.GetAsync("ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		await Task.Delay(TimeSpan.FromSeconds(10));

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

	[TestMethod]
	public async Task Reliability4()
	{
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("http://localhost:5013");

		var apiCallTasks = Enumerable.Range(0, 4)
			.Select(_ => httpClient.GetAsync("timeout/ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		var responses = await Task.WhenAll(apiCallTasks);

		foreach (var httpResponseMessage in responses)
		{
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
		}
	}

	[TestMethod]
	public async Task Reliability5()
	{
		using var httpClientWithoutTimeout = new HttpClient();
		httpClientWithoutTimeout.BaseAddress = new Uri("http://localhost:5013");

		var makeTemporaryBrokenTasks = Enumerable.Range(0, 250)
			.Select(_ => httpClientWithoutTimeout.GetAsync(
				"retry/timeout/ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		var breakForeverTasks = Enumerable.Range(0, 150)
			.Select(_ => httpClientWithoutTimeout.GetAsync(
				"retry/timeout/ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		try
		{
			await Task.WhenAll(breakForeverTasks);
		}
		catch
		{
			// ignore
		}

		await Task.Delay(TimeSpan.FromSeconds(40));

		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("http://localhost:5013");

		var response =
			await httpClient.GetAsync("retry/timeout/ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216");
			
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
	}
	
	[TestMethod]
	public async Task Reliability6()
	{
		using var httpClientWithoutTimeout = new HttpClient();
		httpClientWithoutTimeout.BaseAddress = new Uri("http://localhost:5013");

		var makeTemporaryBrokenTasks = Enumerable.Range(0, 250)
			.Select(_ => httpClientWithoutTimeout.GetAsync(
				"cb/retry/timeout/ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		var breakForeverTasks = Enumerable.Range(0, 150)
			.Select(_ => httpClientWithoutTimeout.GetAsync(
				"cb/retry/timeout/ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		try
		{
			await Task.WhenAll(breakForeverTasks);
		}
		catch
		{
			// ignore
		}

		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("http://localhost:5013");

		var response =
			await httpClient.GetAsync("cb/retry/timeout/ct/async/weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216");
			
		Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
	}
}