using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revert.Core.Extensions;


namespace Revert.Core.Mathematics
{
    public class DistributionBuilder<TModel, TKey>
    {
        public Func<TModel, TKey> KeyDelegate { get; set; }
        public Func<TModel, bool> ModelValidator { get; set; }

        public DistributionBuilder(Func<TModel, bool> modelValidator, Func<TModel, TKey> keyDelegate)
        {
            ModelValidator = modelValidator;
            KeyDelegate = keyDelegate;
        }

        public Dictionary<TKey, List<TModel>> Samples { get; set; } = new Dictionary<TKey, List<TModel>>();


        public void AddSample(TModel model)
        {
            var key = KeyDelegate(model);
            Samples.AddToCollection(key, model);
        }

        public KeyValuePair<TKey, List<TModel>>[] GetSorted()
        {
            return Samples.OrderBy(item => item.Key).ToArray();
        }

        public KeyValuePair<TKey, float>[] GetSortedPercentage()
        {
            var count = Samples.Sum(sample => sample.Value.Count);
            return Samples.Select(s => new KeyValuePair<TKey, float>(s.Key, s.Value.Count / (float)count)).OrderBy(item => item.Key).ToArray();
        }


    }
}
