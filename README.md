# ae-poc-identity-mcpsrv.sln
#	ae-poc-identity-mcp-srvsse.csproj
#	ae-poc-identity-mcp-lib.csproj
This repository contains projects (.net c#, https://github.com/modelcontextprotocol/csharp-sdk) with examples of Model Context Protocol(MCP) server with SSE transport for Identity Claims API
This application communicates with a backend REST Web API to function. 

## Architecture Overview

The solution consists of three main components: the external client application (implemented MCP client), the MCP server (this project), and a backend service (REST API) for identity management. The MCP server acts as an intermediary layer, processing client requests and communicating with the backend API for tasks such as authentication.

```mermaid
graph TD
    subgraph "External Client Application"
        lblMCPClient["MCP Client<br/>(Authentication via Token)"]
    end

    subgraph "MCP Server (this repository)"
        lblMCPServer["MCP Server SSE transport<br/>(Implements MCP)"]
    end

    subgraph "Backend Service (REST API)"
        lblBackendService["External repository<br/>sample-identity-jwt<br/>(ae-sample-identity-webapi.csproj)"]
    end

    lblMCPClient -- "MCP over SSE (http://localhost:3001/identity/mcp)" <--> lblMCPServer
    lblMCPServer -- "HTTP/REST (http://localhost:5023/api/v1/masterdata/claims)" <--> lblBackendService
```

## Communicating with the backend REST API
Before running the application, you need to ensure the required API service is running. Please start the `ae-sample-identity-webapi` service from the `sample-identity-jwt` repository. Refer to the instructions within the `sample-identity-jwt` repository to build and run the service.


