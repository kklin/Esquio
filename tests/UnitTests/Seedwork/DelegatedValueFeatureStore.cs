﻿using Esquio.Abstractions;
using Esquio.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnitTests.Seedwork
{
    class DelegatedValueFeatureStore
        : IFeatureStore
    {
        private readonly Func<string, string, Feature> _getDelegatedFeatureFunc;

        public bool IsReadOnly => true;

        public DelegatedValueFeatureStore(Func<string, string, Feature> getDelegatedFeatureFunc)
        {
            _getDelegatedFeatureFunc = getDelegatedFeatureFunc;
        }

        public Task<bool> AddFeatureAsync(string applicationName, string featureName, bool enabled = false)
        {
            throw new NotImplementedException();
        }

        public Task<Feature> FindFeatureAsync(string applicationName, string featureName)
        {
            return Task.FromResult(_getDelegatedFeatureFunc(applicationName, featureName));
        }

        public Task<bool> AddFeatureAsync(string applicationName, Feature feature)
        {
            throw new NotImplementedException();
        }
    }
}
