﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Judge.Infrastructure.Generators;
using Judge.Infrastructure.ProblemsSchema;
using RestSharp;
using ServiceStack.Redis;

namespace Judge.Infrastructure.Data.Repositories
{
    public class CachedProblemsRepository : IProblemsRepository
    {
        private readonly IRedisClient _redisClient;
        private readonly IRestClient _restClient;
        private readonly ISkeletonCodeGenerator _skeletonCodeGenerator;

        public CachedProblemsRepository(IRedisClient redisClient, IRestClient restClient,
            ISkeletonCodeGenerator skeletonCodeGenerator)
        {
            _redisClient = redisClient;
            _restClient = restClient;
            _skeletonCodeGenerator = skeletonCodeGenerator;
        }

        public async Task<List<Problem>> GetAllAsync()
        {
            var request = new RestRequest("problems", Method.GET);
            var r = await Task.Run(() =>
            {
                var response = _restClient.Execute<List<Problem>>(request);

                var result = response?.Data ?? new List<Problem>();
                result.ForEach(problem => _skeletonCodeGenerator.GenerateFor(problem));

                _redisClient.StoreAll(result);

                return result;
            });

            return r;
        }

        public async Task<Problem> FindByIdAsync(string id)
        {
            var r = await Task.Run(() =>
            {
                var problem = _redisClient.GetById<Problem>(id);
                if (problem == null)
                {
                    var request = new RestRequest($"problems/{id}", Method.GET);
                    var response = _restClient.Execute<Problem>(request);

                    problem = response?.Data;
                    if (problem == null)
                        return null;

                    _skeletonCodeGenerator.GenerateFor(problem);
                    _redisClient.Store(problem);
                }
                else
                {
                    _skeletonCodeGenerator.GenerateFor(problem);
                }

                return problem;
            });

            return r;
        }
    }
}