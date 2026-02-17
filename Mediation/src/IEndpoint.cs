using Microsoft.AspNetCore.Routing;

namespace RD.AspNetCore.Mediation;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}