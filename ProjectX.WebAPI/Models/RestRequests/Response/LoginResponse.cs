namespace ProjectX.WebAPI.Models.RestRequests.Response
{

    public record LoginResponse
    {

        /// <summary>
        /// The JWT login token to use as a bearer authorization header for other API requests.
        /// </summary>
        /// <example name="success">eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRG90ZWxlclgiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJEZWZhdWx0IFJvbGUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjdkM2M4YjNlLTE0MDMtNDIxNy1iZGE0LWQwMWUxMjdkZDkxZCIsImV4cCI6MTY2MDgwMzIzNCwiaXNzIjoiaHR0cHM6Ly9hcGkuZ29waGVyaW5kdXN0cmllcy5uZXQiLCJhdWQiOiJodHRwczovL2FwaS5nb3BoZXJpbmR1c3RyaWVzLm5ldCJ9._vs4TzB4FEzTDOdXNRVfqHafIgb_e3JRGyY1db7W1hw</example>
        public string AccessToken { get; init; }

        /// <summary>
        /// The JWT token used to generate a new access token.
        /// </summary>
        /// <example>eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRG90ZWxlclgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjJhY2JmNjY0LTM5MTUtNDNhZS1iOTg4LTYzMWE5YmMxNjU4MCIsIlJlZnJlc2hUb2tlbklkIjoiNTE2YTFjNTEtOTY1Zi00ZDliLThlNTEtNzQyZDJhZDIxMDNiNGNiYjM1M2QtYjMxNS00MDRkLWExZDUtMWM2ODAzMGY3OWIzIiwiZXhwIjoxNjYxMTgyNTE3LCJpc3MiOiJodHRwczovL2FwaS5nb3BoZXJpbmR1c3RyaWVzLm5ldCIsImF1ZCI6Imh0dHBzOi8vYXBpLmdvcGhlcmluZHVzdHJpZXMubmV0In0.nfGeHZ7DolmAnovcp70miWP6h3kL5OYZM6ZMIOejOKePrsiUNZSokaJu49YV-rmR_mbXGG_xZZQA8AQWUQjrjw</example>
        public string RefreshToken { get; init; }

    }

}
