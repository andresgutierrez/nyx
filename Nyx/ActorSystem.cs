﻿
using System.Collections.Concurrent;

namespace Nyx;

public sealed class ActorSystem
{
    private readonly ConcurrentDictionary<Type, IActorRepositoryRunnable> repositories = new();

    public IActorRef<TActor, TRequest, TResponse> Create<TActor, TRequest, TResponse>(string? name = null) where TActor : IActor<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        ActorRepository<TActor, TRequest, TResponse> repository = GetRepository<TActor, TRequest, TResponse>();

        return repository.Create(name);
    }

    public IActorRef<TActor, TRequest> Create<TActor, TRequest>(string? name = null) where TActor : IActor<TRequest> where TRequest : class
    {
        ActorRepository<TActor, TRequest> repository = GetRepository<TActor, TRequest>();

        return repository.Create(name);
    }

    public ActorRepository<TActor, TRequest, TResponse> GetRepository<TActor, TRequest, TResponse>() where TActor : IActor<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        if (!repositories.TryGetValue(typeof(TActor), out IActorRepositoryRunnable? unitOfWorker))
        {
            ActorRepository<TActor, TRequest, TResponse> newUnitOfWork = new(this);
            repositories.TryAdd(typeof(TActor), newUnitOfWork);
            return newUnitOfWork;
        }

        return (ActorRepository<TActor, TRequest, TResponse>)unitOfWorker;
    }

    public ActorRepository<TActor, TRequest> GetRepository<TActor, TRequest>() where TActor : IActor<TRequest> where TRequest : class
    {
        if (!repositories.TryGetValue(typeof(TActor), out IActorRepositoryRunnable? unitOfWorker))
        {
            ActorRepository<TActor, TRequest> newUnitOfWork = new(this);
            repositories.TryAdd(typeof(TActor), newUnitOfWork);
            return newUnitOfWork;
        }

        return (ActorRepository<TActor, TRequest>)unitOfWorker;
    }

    public async Task Run()
    {
        foreach (KeyValuePair<Type, IActorRepositoryRunnable> keyValuePair in repositories)
        {
            while (keyValuePair.Value.HasPendingMessages())
                await keyValuePair.Value.Run();
        }
    }
}
