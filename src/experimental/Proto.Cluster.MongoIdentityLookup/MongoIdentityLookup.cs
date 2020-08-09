﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Proto.Cluster.IdentityLookup;
using Proto.Remote;

namespace Proto.Cluster.MongoIdentityLookup
{
    
    public class MongoIdentityLookup : IIdentityLookup 
    {
        private Cluster _cluster;
        private IMongoDatabase _db;
        private string[] _kinds;
        private readonly string _clusterName;
        private IMongoCollection<PidLookup> _pids;

        public MongoIdentityLookup(string clusterName, IMongoDatabase db)
        {
            _clusterName = clusterName;
            _db = db;
            _pids = db.GetCollection<PidLookup>("pids");
        }

        public async Task<(PID?, ResponseStatusCode)> GetAsync(string identity, string kind, CancellationToken ct)
        {
            var key = $"{_clusterName}-{kind}-{identity}";
            var pidLookup = _pids.AsQueryable().FirstOrDefault(x => x.Key == key);
            if (pidLookup != null)
            {
                var pid = new PID(pidLookup.Address,pidLookup.Identity);
                return (pid, ResponseStatusCode.OK);
            }

            return (null, ResponseStatusCode.Unavailable);
        }

        public void Setup(Cluster cluster, string[] kinds)
        {
            _cluster = cluster;
            _kinds = kinds;
        }

        public void Stop()
        {

        }
    }
}