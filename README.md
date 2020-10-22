# ReviewsIndexer

## Building

run the command `dotnet build`

## Execute

run the command `dotnet run`. The application is available at `https://localhost:5001`

## How to improve this PoC into a production-ready application

- Replace the local static index with a stored external index based on SQL, NoSQL, Lucene or whatever can handle fulltext indexation.
- Replace the local static queue with an external queuing service that can store non handled items
- On back-end, add a logging system and push pertinent logs into a monitoring system to track issues and performances
- Handle many parralel indexation tasks to improve performance
- In production, split back-end and front-end to be hosted with distinct URLs