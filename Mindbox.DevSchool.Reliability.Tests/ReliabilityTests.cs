using System.Net;

namespace Mindbox.DevSchool.Reliability.Tests;

[TestClass]
public sealed class ReliabilityTests
{
	[TestMethod]
	public async Task ThreadStarvation()
	{
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("http://localhost:5013");
		httpClient.Timeout = TimeSpan.FromSeconds(5);

		var makeApiCallTasks = Enumerable.Range(0, 4)
			.Select(_ => httpClient.GetAsync("weatherForecast/c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"))
			.ToArray();

		var responses = await Task.WhenAll(makeApiCallTasks);

		foreach (var httpResponseMessage in responses)
		{
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
		}
	}
}