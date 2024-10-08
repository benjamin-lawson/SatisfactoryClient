using Moq;
using Moq.Contrib.HttpClient;
using System.Net;
using SatisfactorySdk;
using System.Text.Json;
using SatisfactorySdk.DTO;

namespace SatisfactoryClientTests
{
    public class QueryServerStateTests
    {
        private const string _ip = "127.0.0.1";

        [Test]
        public async Task QueryServerStateSuccessful()
        {
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .SetupRequest($"https://{_ip}:7777/api/v1", (pred) => TestUtilities.MatchRequestFunction("QueryServerState", pred))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new RequestResponse<QueryServerStateResponse>
                    {
                        Data = new QueryServerStateResponse
                        {
                            ServerState = new ServerState
                            {
                                ActiveSessionName = "Game Session",
                                ConnectedPlayers = 1,
                                PlayerLimit = 4,
                                TechTier = 6,
                                ActiveSchematic = "None",
                                GamePhase = "/Script/FactoryGame.FGGamePhase'/Game/FactoryGame/GamePhases/GP_Project_Assembly_Phase_2.GP_Project_Assembly_Phase_2'",
                                IsGameRunning = true,
                                TotalGameDuration = 352470,
                                IsPaused = true,
                                AverageTickRate = 29.995f,
                                AutoLoadSessionName = "Game Session"
                            }
                        }
                    }))
                });

            var client = new SatisfactoryClient(_ip, 7777, client: mockHandler.CreateClient());
            var response = await client.QueryServerStateAsync();

            Assert.IsTrue(response.IsSuccessful);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.IsNotNull(response.RequestResponse);
            Assert.That(response.RequestResponse.Data.ServerState.PlayerLimit, Is.EqualTo(4));
            Assert.That(response.RequestResponse.Data.ServerState.IsPaused, Is.EqualTo(true));
            Assert.That(response.RequestResponse.Data.ServerState.AutoLoadSessionName, Is.EqualTo("Game Session"));
        }
    }
}
