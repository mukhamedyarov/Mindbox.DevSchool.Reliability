using System.Net;

namespace Mindbox.DevSchool.Reliability.Tests;

[TestClass]
public sealed class ReliabilityTests
{
	[TestMethod]
	public async Task ThreadStarvation_Test()
	{
		using var httpClient = new HttpClient();
		httpClient.BaseAddress = new Uri("http://localhost:5013");
		httpClient.Timeout = TimeSpan.FromSeconds(2);

		var makeApiCallTasks = Enumerable.Range(0, 999)
			.Select(_ => httpClient.GetAsync("/weatherForecast/1"))
			.ToArray();

		var responses = await Task.WhenAll(makeApiCallTasks);

		foreach (var httpResponseMessage in responses)
		{
			Assert.AreEqual(HttpStatusCode.OK, httpResponseMessage.StatusCode);
		}
	}
}